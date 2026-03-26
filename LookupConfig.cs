using System.Text.Json;

namespace ACNH.LookupBot {

  /// <summary>
  /// JSON configuration for the lookup bot (token, prefix, and optional access control).
  /// </summary>
  public sealed class LookupConfig {
    /// <summary>Discord bot token.</summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>Command prefix (default is $).</summary>
    public string Prefix { get; set; } = "$";

    /// <summary>Optional activity name shown as the bot's playing status.</summary>
    public string? ActivityName { get; set; }

    /// <summary>When false, prefix commands are rejected before execution.</summary>
    public bool EnableCommands { get; set; } = true;

    /// <summary>
    /// When non-zero, treated as owner for permission bypass. When zero, the application owner is used.
    /// </summary>
    public ulong OwnerUserId { get; set; }

    /// <summary>When non-empty, only these users may use commands (owner always allowed).</summary>
    public List<ulong> AllowedUserIds { get; set; } = new();

    /// <summary>When non-empty, commands are only accepted in these channels (owner always allowed).</summary>
    public List<ulong> AllowedChannelIds { get; set; } = new();

    /// <summary>
    /// When non-empty, the user must have this guild role name (case-sensitive match to the role name).
    /// </summary>
    public string? RequiredRoleName { get; set; }

    /// <summary>When true, messages from other bots are ignored for command handling.</summary>
    public bool IgnoreOtherBots { get; set; } = true;

    public static LookupConfig Load(string path) {
      string json = File.ReadAllText(path);
      var options = new JsonSerializerOptions {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
      };
      LookupConfig? config = JsonSerializer.Deserialize<LookupConfig>(json, options);
      if (config == null) {
        throw new InvalidOperationException("Config deserialized to null.");
      }

      return config;
    }

    /// <summary>
    /// Writes a new config file with default values. The file is valid JSON with leading // comments
    /// that <see cref="Load"/> accepts.
    /// </summary>
    public static void WriteDefaultConfigFile(string path) {
      string? directory = Path.GetDirectoryName(path);
      if (!string.IsNullOrEmpty(directory)) {
        Directory.CreateDirectory(directory);
      }

      var defaults = new LookupConfig {
        BotToken = string.Empty,
        Prefix = "$",
        ActivityName = "ACNH lookup",
        EnableCommands = true,
        OwnerUserId = 0,
        AllowedUserIds = new List<ulong>(),
        AllowedChannelIds = new List<ulong>(),
        RequiredRoleName = string.Empty,
        IgnoreOtherBots = true,
      };

      var serializeOptions = new JsonSerializerOptions { WriteIndented = true };
      string body = JsonSerializer.Serialize(defaults, serializeOptions);
      const string Header =
          "// Set BotToken below, then start the bot again.\n" +
          "// In the Discord application settings, enable the Message Content Intent for prefix commands in servers.\n";
      File.WriteAllText(path, Header + body);
    }
  }
}
