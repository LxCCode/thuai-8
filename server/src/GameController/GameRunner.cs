using Serilog;

namespace Thuai.Server.GameController;

/// <summary>
/// Runs and manages the game.
/// </summary>
public partial class GameRunner(Utility.Config.GameSettings gameSettings)
{
    public Utility.Config.GameSettings GameSettings = gameSettings;
    public GameLogic.Game Game { get; private set; } = new(gameSettings);

    public bool IsRunning { get; private set; } = false;
    private readonly Utility.ClockProvider _clockProvider = new(1000 / gameSettings.TicksPerSecond);

    private readonly ILogger _logger = Utility.Tools.LogHandler.CreateLogger("GameRunner");

    public void Start()
    {
        if (IsRunning)
        {
            _logger.Warning("Cannot run a running game again.");
            return;
        }

        _logger.Information("Starting...");

        Game.Initialize();

        IsRunning = true;
        Task.Run(() =>
        {
            while (IsRunning)
            {
                Task clock = _clockProvider.CreateClock();
                Game.Tick();

                if (Game.Stage == GameLogic.Game.GameStage.Finished)
                {
                    IsRunning = false;
                }

                clock.Wait();
            }

            _logger.Information("Game finished.");
        });

        _logger.Information("Started.");
    }
}
