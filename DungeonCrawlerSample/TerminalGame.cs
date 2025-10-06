namespace MohawkTerminalGame
{
    public class TerminalGame
    {
        // Place your variables here
        TerminalGridWithColor map;
        ColoredText tree = new(@"/\", ConsoleColor.Green, ConsoleColor.DarkGreen);
        ColoredText riverNS = new(@"||", ConsoleColor.Blue, ConsoleColor.DarkBlue);
        ColoredText riverEW = new(@"==", ConsoleColor.Blue, ConsoleColor.DarkBlue);
        ColoredText player = new(@"😎", ConsoleColor.White, ConsoleColor.Black);
        bool inputChanged;
        int oldPlayerX;
        int oldPlayerY;
        int playerX = 5;
        int playerY = 0;

        /// Run once before Execute begins
        public void Setup()
        {
            // Run program at timed intervals.
            Program.TerminalExecuteMode = TerminalExecuteMode.ExecuteTime;
            Program.TerminalInputMode = TerminalInputMode.EnableInputDisableReadLine;
            Program.TargetFPS = 60;
            // Prepare some terminal settings
            Terminal.SetTitle("Dungeon Crawler Sample");
            Terminal.CursorVisible = false; // hide cursor

            // Set map to some values
            map = new(10, 10, tree);
            map.SetCol(riverNS, 3);
            map.SetRow(riverEW, 8);

            // Clear window and draw map
            map.ClearWrite();
            // Draw player. x2 because my tileset is 2 columns wide.
            DrawCharacter(playerX, playerY, player);
        }

        // Execute() runs based on Program.TerminalExecuteMode (assign to it in Setup).
        //  ExecuteOnce: runs only once. Once Execute() is done, program closes.
        //  ExecuteLoop: runs in infinite loop. Next iteration starts at the top of Execute().
        //  ExecuteTime: runs at timed intervals (eg. "FPS"). Code tries to run at Program.TargetFPS.
        //               Code must finish within the alloted time frame for this to work well.
        public void Execute()
        {
            // Move player
            CheckMovePlayer();

            // Naive approach, works but it's much but slower
            //map.Overwrite(0,0);
            //map.Poke(playerX * 2, playerY, player);

            // Only move player if needed
            if (inputChanged)
            {
                ResetCell(oldPlayerX, oldPlayerY);
                DrawCharacter(playerX, playerY, player);
                inputChanged = false;
            }

            // Write time below game
            Terminal.SetCursorPosition(0, 12);
            Terminal.ResetColor();
            Terminal.Write(Time.DisplayText);
        }

        void CheckMovePlayer()
        {
            //
            inputChanged = false;
            oldPlayerX = playerX;
            oldPlayerY = playerY;

            if (Input.IsKeyPressed(ConsoleKey.RightArrow))
                playerX++;
            if (Input.IsKeyPressed(ConsoleKey.LeftArrow))
                playerX--;
            if (Input.IsKeyPressed(ConsoleKey.DownArrow))
                playerY++;
            if (Input.IsKeyPressed(ConsoleKey.UpArrow))
                playerY--;

            playerX = Math.Clamp(playerX, 0, map.Width - 1);
            playerY = Math.Clamp(playerY, 0, map.Height - 1);

            if (oldPlayerX != playerX || oldPlayerY != playerY)
                inputChanged = true;
        }

        void DrawCharacter(int x, int y, ColoredText character)
        {
            ColoredText mapTile = map.Get(x, y);
            // Copy BG color. This assumes emoji.
            player.bgColor = mapTile.bgColor;
            // Character (eg. player) and grid are 2-width characters
            map.Poke(x * 2, y, player);
        }

        void ResetCell(int x, int y)
        {
            ColoredText mapTile = map.Get(x, y);
            // Player and grid are 2-width characters
            map.Poke(x * 2, oldPlayerY, mapTile);
        }

    }
}
