using System;
using System.Text;
using System.Timers;

namespace MohawkTerminalGame
{
    /// <summary>
    ///     The underlying program. 🤫
    /// </summary>
    internal class Program
    {
        private static readonly System.Timers.Timer gameLoopTimer = new();
        private static TerminalGame? game;
        private static bool CanGameExecuteTick = true;
        private static int targetFPS = 20;
        private static TerminalExecuteMode terminalMode = TerminalExecuteMode.ExecuteOnce;

        /// <summary>
        ///     The target frames per second the terminal aims to run at.
        ///     Note that frame timing is somewhat inconsistent
        ///     (off by a few milliseconds each frame).
        /// </summary>
        public static int TargetFPS
        {
            get
            {
                return targetFPS;
            }
            set
            {
                // Set target as reference
                targetFPS = value;
                // Timer is in milliseconds!
                gameLoopTimer.Interval = 1000.0 / targetFPS;
            }
        }

        /// <summary>
        ///     How the <see cref="TerminalGame.Execute"/> function is run.
        /// </summary>
        public static TerminalExecuteMode TerminalExecuteMode
        {
            get
            {
                return terminalMode;
            }
            set
            {
                // Enable / disable timer as needed
                gameLoopTimer.Enabled = value == TerminalExecuteMode.ExecuteTime;
                // Set as usual
                terminalMode = value;
            }
        }

        public static TerminalInputMode TerminalInputMode { get; set; } = TerminalInputMode.KeyboardReadAndReadLine;

        static void Main(string[] args)
        {

            // Set IO and clear window.
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            Console.Clear();

            // Prep
            Terminal.CursorVisible = true;
            Input.InitInputThread();

            // Create the game instance
            game = new TerminalGame();

            // Show the intro screen before setup
            game.ShowIntroWithDialogue(); // <- NEW If you want to skip intro comment this out

            // Set up the game
            game.Setup();
            
            // Set up Time helper
            if (Time.AutoStart)
                Time.Start();

            // Core "loop"
            bool doLoop = true;
            while (doLoop && !Input.IsKeyPressed(ConsoleKey.Escape))
            {
                Input.PreparePollNextInput();
                switch (TerminalExecuteMode)
                {
                    case TerminalExecuteMode.ExecuteOnce:
                        game.Execute();
                        if (TerminalExecuteMode == TerminalExecuteMode.ExecuteOnce)
                            doLoop = false;
                        break;
                    case TerminalExecuteMode.ExecuteLoop:
                        game.Execute();
                        break;
                    case TerminalExecuteMode.ExecuteTime:
                        TargetFPS = targetFPS; // Force update interval
                        gameLoopTimer.Elapsed += GameLoopTimerEvents;
                        gameLoopTimer.Start();
                        while (TerminalExecuteMode == TerminalExecuteMode.ExecuteTime &&
                               !Input.IsKeyPressed(ConsoleKey.Escape))
                        {
                            if (CanGameExecuteTick)
                            {
                                CanGameExecuteTick = false;
                                game.Execute();
                                Input.PreparePollNextInput();
                            }
                        }
                        gameLoopTimer.Stop();
                        gameLoopTimer.Elapsed -= GameLoopTimerEvents;
                        break;
                    default:
                        string msg = $"{nameof(MohawkTerminalGame.TerminalExecuteMode)}{TerminalExecuteMode}";
                        throw new NotImplementedException(msg);
                }
            }

            Console.ResetColor();
            Environment.Exit(0);
        }


        private static void GameLoopTimerEvents(object? o, ElapsedEventArgs sender)
        {
            CanGameExecuteTick = true;
        }
    }
}
