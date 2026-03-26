namespace ACNH.LookupBot {

  internal static class Program {
    private static void PauseForUserRead() {
      Console.WriteLine();
      if (Console.IsInputRedirected) {
        return;
      }

      Console.WriteLine("Press any key to exit...");
      Console.ReadKey(intercept: true);
    }

    private static async Task<int> Main(string[] args) {
      string configPath = args.Length > 0
          ? args[0]
          : Path.Combine(AppContext.BaseDirectory, "lookup_config.json");

      if (!File.Exists(configPath)) {
        try {
          LookupConfig.WriteDefaultConfigFile(configPath);
        } catch (Exception ex) {
          Console.Error.WriteLine($"Could not create config file: {ex.Message}");
          PauseForUserRead();
          return 1;
        }

        Console.WriteLine(
            $"Created {configPath}. Set BotToken in that file, then start the bot again.");
        PauseForUserRead();
        return 1;
      }

      LookupConfig config;
      try {
        config = LookupConfig.Load(configPath);
      } catch (Exception ex) {
        Console.Error.WriteLine($"Failed to load config: {ex.Message}");
        PauseForUserRead();
        return 1;
      }

      if (string.IsNullOrWhiteSpace(config.BotToken)) {
        Console.Error.WriteLine("BotToken is empty. Set it in lookup_config.json.");
        PauseForUserRead();
        return 1;
      }

      using var shutdown = new CancellationTokenSource();
      Console.CancelKeyPress += (_, eventArgs) => {
        eventArgs.Cancel = true;
        shutdown.Cancel();
      };

      using var bot = new LookupBotService(config);
      await bot.RunAsync(shutdown.Token).ConfigureAwait(false);
      return 0;
    }
  }
}
