using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using NHSE.Core;

namespace ACNH.LookupBot {

  public sealed class VillagerLookupModule : ModuleBase<SocketCommandContext> {
    [Command("villagerName")]
    [Alias("vn", "nv", "name")]
    [Summary(
        "Map villager display name to internal id. Default NHSE strings, or language code as first arg then name.")]
    public async Task GetVillagerInternalNameAsync(
        [Summary("Language code to search with")]
        string language,
        [Summary("Villager name")]
        [Remainder]
        string villagerName) {
      var strings = GameInfo.GetStrings(language);
      await ReplyVillagerNameAsync(strings, villagerName).ConfigureAwait(false);
    }

    [Command("villagerName")]
    [Alias("vn", "nv", "name")]
    [Summary(
        "Map villager display name to internal id. Default NHSE strings, or language code as first arg then name.")]
    public async Task GetVillagerInternalNameAsync(
        [Summary("Villager name")]
        [Remainder]
        string villagerName) {
      var strings = GameInfo.Strings;
      await ReplyVillagerNameAsync(strings, villagerName).ConfigureAwait(false);
    }

    private async Task ReplyVillagerNameAsync(GameStrings strings, string villagerName) {
      var map = strings.VillagerMap;
      var result = map.FirstOrDefault(z => string.Equals(
          villagerName,
          z.Value.Replace(" ", string.Empty),
          StringComparison.InvariantCultureIgnoreCase));
      if (string.IsNullOrWhiteSpace(result.Key)) {
        await ReplyAsync($"No villager found of name {villagerName}.").ConfigureAwait(false);
        return;
      }

      await ReplyAsync($"{villagerName}={result.Key}").ConfigureAwait(false);
    }
  }
}
