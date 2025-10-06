using System;

namespace MohawkTerminalGame
{
    public class TerminalGame
    {
        // Place your variables here


        /// Run once before Execute begins
        public void Setup()
        {
            Program.TerminalExecuteMode = TerminalExecuteMode.ExecuteLoop;
            Program.TerminalInputMode = TerminalInputMode.KeyboardReadAndReadLine;

            Terminal.SetTitle("Text Adventure Sample");
            Terminal.RoboTypeIntervalMilliseconds = 50; // 50 milliseconds
            Terminal.UseRoboType = true; // use slow character typing
            Terminal.WriteWithWordBreaks = true; // donbreak around wors, don't cut them off
            Terminal.WordBreakCharacter = ' '; // break on spaces
        }

        // Execute() runs based on Program.TerminalExecuteMode (assign to it in Setup).
        //  ExecuteOnce: runs only once. Once Execute() is done, program closes.
        //  ExecuteLoop: runs in infinite loop. Next iteration starts at the top of Execute().
        //  ExecuteTime: runs at timed intervals (eg. "FPS"). Code tries to run at Program.TargetFPS.
        //               Code must finish within the alloted time frame for this to work well.
        public void Execute()
        {
            Terminal.RoboTypeIntervalMilliseconds = 50;
            Terminal.Beep();
            Terminal.WriteLine("DANGER. AREA INFECTED WITH VIRUS.", ConsoleColor.Black, ConsoleColor.DarkYellow);
            Terminal.Beep();
            Terminal.RoboTypeIntervalMilliseconds = 10;
            Terminal.WriteLine("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
            Terminal.WriteLine("Do you wish to proceed? (y/n)", ConsoleColor.Black, ConsoleColor.Red);
            string answer = Terminal.ReadAndClearLine();
            if (answer.ToLower().Equals("y"))
            {
                Terminal.RoboTypeIntervalMilliseconds = 50;
                Terminal.WriteLine("Mankind thanks you, Elite McGeet.");
            }
            else if (answer.ToLower().Equals("n"))
            {
                Terminal.RoboTypeIntervalMilliseconds = 100;
                Terminal.WriteLine("Mission failed before it even began.");
            }
            else
            {
                Terminal.RoboTypeIntervalMilliseconds = 150;
                Terminal.WriteLine("MISCHIEVIOUS ARE WE?");
            }
            Terminal.WriteLine();
            Terminal.WriteLine("Press CTRL+C to force quit any program.");
            Terminal.WriteLine();
        }

    }
}
