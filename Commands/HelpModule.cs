using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace ACNH.LookupBot {

  public sealed class HelpModule : ModuleBase<SocketCommandContext> {
    private const int _maxEmbedFields = 25;
    private const int _maxFieldValueLength = 1024;
    private const int _maxFieldNameLength = 256;

    private readonly CommandService _commandService;
    private readonly LookupConfig _config;

    public HelpModule(CommandService commandService, LookupConfig config) {
      _commandService = commandService;
      _config = config;
    }

    [Command("help")]
    [Alias("h", "?")]
    [Summary("Lists available bot commands.")]
    public async Task HelpAsync() {
      string prefix = _config.Prefix;
      var embed = new EmbedBuilder()
          .WithTitle("Available commands")
          .WithDescription($"Use the prefix `{prefix}` before each command name below.")
          .WithColor(new Color(52, 152, 219))
          .WithCurrentTimestamp();

      int fieldCount = 0;
      foreach (var group in _commandService.Commands
          .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
          .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)) {
        if (fieldCount >= _maxEmbedFields) {
          break;
        }

        string name = group.Key;
        var aliasSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (CommandInfo cmd in group) {
          foreach (string alias in cmd.Aliases) {
            if (!string.Equals(alias, name, StringComparison.OrdinalIgnoreCase)) {
              aliasSet.Add(alias);
            }
          }
        }

        string fieldName = $"{prefix}{name}";
        if (fieldName.Length > _maxFieldNameLength) {
          fieldName = fieldName.Substring(0, _maxFieldNameLength - 3) + "...";
        }

        string? summary = group.Select(c => c.Summary).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        if (string.IsNullOrWhiteSpace(summary)) {
          summary = "No summary.";
        }

        var valueText = new StringBuilder();
        if (aliasSet.Count > 0) {
          valueText.Append("**Aliases:** ");
          valueText.Append(string.Join(", ", aliasSet.OrderBy(a => a)));
          valueText.AppendLine();
          valueText.AppendLine();
        }

        valueText.Append(summary);
        string fieldValue = valueText.ToString();
        if (fieldValue.Length > _maxFieldValueLength) {
          fieldValue = fieldValue.Substring(0, _maxFieldValueLength - 3) + "...";
        }

        embed.AddField(fieldName, fieldValue, inline: false);
        fieldCount++;
      }

      if (fieldCount >= _maxEmbedFields) {
        embed.WithFooter("Additional commands omitted (embed field limit).");
      }

      await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
    }
  }
}
