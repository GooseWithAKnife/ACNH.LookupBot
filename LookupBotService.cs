using System.Linq;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using static Discord.GatewayIntents;

namespace ACNH.LookupBot {

  /// <summary>
  /// Hosts <see cref="DiscordSocketClient"/> and <see cref="CommandService"/> with prefix routing.
  /// </summary>
  public sealed class LookupBotService : IDisposable {
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly LookupConfig _config;
    private readonly IServiceProvider _services;
    private ulong _ownerUserId;

    public LookupBotService(LookupConfig config) {
      _config = config;
      _client = new DiscordSocketClient(new DiscordSocketConfig {
        LogLevel = LogSeverity.Info,
        GatewayIntents = Guilds | GuildMessages | DirectMessages | MessageContent,
      });
      _commands = new CommandService(new CommandServiceConfig {
        LogLevel = LogSeverity.Info,
        DefaultRunMode = RunMode.Sync,
        CaseSensitiveCommands = false,
      });
      _client.Log += LogAsync;
      _commands.Log += LogAsync;
      _services = BuildServices();
    }

    public void Dispose() {
      _client.Log -= LogAsync;
      _commands.Log -= LogAsync;
      _client.Dispose();
    }

    public async Task RunAsync(CancellationToken cancellationToken) {
      await InitCommandsAsync().ConfigureAwait(false);
      await _client.LoginAsync(TokenType.Bot, _config.BotToken).ConfigureAwait(false);
      await _client.StartAsync().ConfigureAwait(false);

      _client.Ready += OnClientReadyAsync;

      try {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
      } catch (OperationCanceledException) {
      }

      await _client.StopAsync().ConfigureAwait(false);
    }

    private IServiceProvider BuildServices() {
      var collection = new ServiceCollection();
      collection.AddSingleton(_config);
      collection.AddSingleton(_commands);
      return collection.BuildServiceProvider();
    }

    private async Task HandleMessageAsync(SocketMessage rawMessage) {
      if (rawMessage is not SocketUserMessage message) {
        return;
      }

      if (message.Author.Id == _client.CurrentUser?.Id) {
        return;
      }

      if (_config.IgnoreOtherBots && message.Author.IsBot) {
        return;
      }

      if (ShouldIgnoreChannelForCommands(message)) {
        return;
      }

      int argPos = 0;
      if (!message.HasStringPrefix(_config.Prefix, ref argPos)) {
        return;
      }

      var context = new SocketCommandContext(_client, message);
      if (!IsAuthorized(message, context, out string denyReason)) {
        await message.Channel.SendMessageAsync(denyReason).ConfigureAwait(false);
        return;
      }

      var result = await _commands.ExecuteAsync(context, argPos, _services).ConfigureAwait(false);
      if (result.Error == CommandError.UnknownCommand) {
        return;
      }

      if (!result.IsSuccess) {
        await message.Channel.SendMessageAsync(result.ErrorReason).ConfigureAwait(false);
      }
    }

    private async Task InitCommandsAsync() {
      var assembly = Assembly.GetExecutingAssembly();
      await _commands.AddModulesAsync(assembly, _services).ConfigureAwait(false);
      _client.MessageReceived += HandleMessageAsync;
    }

    private bool IsAuthorized(SocketUserMessage message, SocketCommandContext context,
        out string denyReason) {
      if (!_config.EnableCommands) {
        denyReason = "Commands are currently disabled.";
        return false;
      }

      if (message.Author.Id == _ownerUserId) {
        denyReason = string.Empty;
        return true;
      }

      if (_config.AllowedUserIds.Count > 0 && !_config.AllowedUserIds.Contains(message.Author.Id)) {
        denyReason = "You are not permitted to use this command.";
        return false;
      }

      if (_config.AllowedChannelIds.Count > 0 && context.Channel is SocketGuildChannel) {
        ulong channelId = GetGuildWhitelistChannelId(context.Channel);
        if (!_config.AllowedChannelIds.Contains(channelId)) {
          denyReason = "You can't use that command here.";
          return false;
        }
      }

      if (!string.IsNullOrWhiteSpace(_config.RequiredRoleName)) {
        if (context.User is not SocketGuildUser guildUser) {
          denyReason = "You must be in a guild to run this command.";
          return false;
        }

        bool hasRole = guildUser.Roles.Any(r => r.Name == _config.RequiredRoleName);
        if (!hasRole) {
          denyReason = "You do not have the required role to run this command.";
          return false;
        }
      }

      denyReason = string.Empty;
      return true;
    }

    /// <summary>
    /// Guild channel to match against <see cref="LookupConfig.AllowedChannelIds"/> (parent for threads).
    /// </summary>
    private static ulong GetGuildWhitelistChannelId(ISocketMessageChannel channel) {
      if (channel is SocketThreadChannel thread) {
        return thread.ParentChannel.Id;
      }

      return channel.Id;
    }

    /// <summary>
    /// When <see cref="LookupConfig.AllowedChannelIds"/> is set, ignore guild traffic outside that list
    /// (no replies). DMs are always considered. Owner matches <see cref="IsAuthorized"/> bypass.
    /// </summary>
    private bool ShouldIgnoreChannelForCommands(SocketUserMessage message) {
      if (message.Channel is not SocketGuildChannel) {
        return false;
      }

      if (_config.AllowedChannelIds.Count == 0) {
        return false;
      }

      if (message.Author.Id == _ownerUserId) {
        return false;
      }

      ulong channelId = GetGuildWhitelistChannelId(message.Channel);
      return !_config.AllowedChannelIds.Contains(channelId);
    }

    private static Task LogAsync(LogMessage message) {
      Console.ForegroundColor = message.Severity switch {
        LogSeverity.Critical => ConsoleColor.Red,
        LogSeverity.Error => ConsoleColor.Red,
        LogSeverity.Warning => ConsoleColor.Yellow,
        LogSeverity.Info => ConsoleColor.White,
        LogSeverity.Verbose => ConsoleColor.DarkGray,
        LogSeverity.Debug => ConsoleColor.DarkGray,
        _ => Console.ForegroundColor,
      };
      string line = $"[{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}";
      Console.WriteLine($"{DateTime.Now:u} {line}");
      Console.ResetColor();
      return Task.CompletedTask;
    }

    private async Task OnClientReadyAsync() {
      var app = await _client.GetApplicationInfoAsync().ConfigureAwait(false);
      _ownerUserId = _config.OwnerUserId != 0 ? _config.OwnerUserId : app.Owner.Id;
      if (!string.IsNullOrWhiteSpace(_config.ActivityName)) {
        await _client.SetGameAsync(_config.ActivityName).ConfigureAwait(false);
      }

      await _client.SetStatusAsync(UserStatus.Online).ConfigureAwait(false);
    }
  }
}
