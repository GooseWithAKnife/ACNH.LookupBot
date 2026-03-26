# ACNH.LookupBot

A small Discord bot for **Animal Crossing: New Horizons** item, recipe, and villager name lookups. It answers prefix commands such as `$lookup`, `$recipe`, `$vn`, `$item`, and `$customize` using game data from NHSE. It does not connect to a Switch or handle orders; it is only for lookup-style commands in Discord.

![License](https://img.shields.io/badge/License-AGPLv3-blue.svg)

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or a runtime if you only run a published build)
- A [Discord application](https://discord.com/developers/applications) with a bot token
- **`NHSE.Core.dll`** placed under `deps/` (see [Other dependencies](#other-dependencies))

## Setup

1. Clone or copy this repository.
2. Build the project:
   - **Development:** `dotnet build` from the repository root.
   - **Release publish (Windows):** run `build.bat`. Output is written to the `publish/` folder.
3. Setup your discord bot token with the instructions below.

## Discord application

1. In the Discord Developer Portal, create your application, then **Bot**.
2. Under **Privileged Gateway Intents**, enable **Message Content Intent** so prefix commands work in servers.
3. Invite the bot with the scopes and permissions you need for reading and sending messages in your channels.
4. Reset and copy your Token into the `lookup_config.json` in the `BotToken` key.

## Configuration and first run

On the first run, if `lookup_config.json` is missing next to the executable (or at the path you pass on the command line), the bot **creates** that file with defaults. Edit it, set **`BotToken`**, then start the bot again.

| Field | Description |
| ----- | ----------- |
| `BotToken` | Discord bot token (required). |
| `Prefix` | Command prefix (default `$`). |
| `ActivityName` | Optional "playing" status text. |
| `EnableCommands` | Set `false` to reject all prefix commands. |
| `OwnerUserId` | `0` uses the Discord application owner as owner; otherwise this user ID bypasses allowlists and role checks. |
| `AllowedUserIds` | If non-empty, only these users may use commands (owner excluded from restriction). |
| `AllowedChannelIds` | If non-empty, commands only work in these channels (owner excluded from restriction). |
| `RequiredRoleName` | If set, users in a guild must have a role whose **name** matches exactly (case-sensitive). |
| `IgnoreOtherBots` | If `true`, other bots are ignored for command handling. |

Optional config path:

```text
ACNH.LookupBot.exe path\to\lookup_config.json
```

Or with the SDK:

```text
dotnet run --project ACNH.LookupBot.csproj -- path\to\lookup_config.json
```

## How to run

- After `dotnet build`, run the executable from `bin/Debug/net10.0/` or `bin/Release/net10.0/`, with `lookup_config.json` in the same folder (or use the path argument above).
- After `build.bat`, run `publish/ACNH.LookupBot.exe` and keep `lookup_config.json` beside it (or pass a path).

Stop the process with **Ctrl+C** in the terminal.

## Commands (default prefix `$`)

NHSE stores display names per language. Commands **without** `Lang` in the name use the **default** string set from your `NHSE.Core.dll` build (`GameInfo.Strings`). The **`*Lang`** form takes an **NHSE language code** as the **first** argument, then the same text you would pass to the non-`Lang` command (`GameInfo.GetStrings(language)`). Valid codes depend on your NHSE build (often short locale tags such as language or region codes NHSE defines).

| Command | Aliases | Description |
| ------- | ------- | ----------- |
| `lookup` | `li`, `search` | Search item names using the **default** NHSE language; substring and exact match behavior as implemented in NHSE. |
| `lookupLang` | `ll` | Search item names using the language from your **first** argument, then the search text (same logic as `lookup`, different locale for names). |
| `recipe` | `ri`, `searchDIY` | DIY recipe order codes by item name using **default** NHSE strings. |
| `recipeLang` | `rl` | DIY recipe lookup using an **NHSE language code** first, then the item name text. |
| `villagerName` | `vn`, `nv`, `name` | Map a villager **display name** to an internal key. Either use **default** NHSE strings, or pass a **language code** first then the name (same pattern as `lookup` / `lookupLang`). |
| `item` | | Item customization info from a hex item id. |
| `customize` | | Build customized item hex from item id and customization sum. |
| `stack` | | Stack count for an item id (optional helper). |
| `help` | `h`, `?` | List all commands in a Discord embed (one field per command, with aliases and summaries). |

## Other dependencies

- **[Discord.Net](https://github.com/discord-net/Discord.Net)** (NuGet) for the Discord client and command framework.
- **[NHSE](https://github.com/kwsch/NHSE/)** provides **NHSE.Core** (referenced as `NHSE.Core.dll`): item strings, parsing, recipes, remake info, and related game logic. Thanks to [kwsch](https://github.com/kwsch) and NHSE contributors for that project.

## License

This program is licensed under the **GNU Affero General Public License v3**. See `LICENSE` and `license.txt` for the full text.
