using DungeonCrawlerSample;

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

        int counter;
        int counter2;
        int y;
        int y2;

        BossAI boss = new BossAI();

        /// Run once before Execute begins
        public void Setup()
        {
            // Run program at timed intervals.
            Program.TerminalExecuteMode = TerminalExecuteMode.ExecuteTime;
            Program.TerminalInputMode = TerminalInputMode.EnableInputDisableReadLine;
            Program.TargetFPS = 60;
            // Prepare some terminal settings
            Terminal.SetTitle("REZ");
            Terminal.CursorVisible = false; // hide cursor

            // Set map to some values
            map = new(10, 10, tree);
            // map.SetCol(riverNS, 3);
            // map.SetRow(riverEW, 8);

            // Clear window and draw map
            map.ClearWrite();
            // Draw player. x2 because my tileset is 2 columns wide.
            DrawCharacter(playerX, playerY, player);

            counter = -60;
            counter2 = -120;
            y = 0;
            y2 = 0;
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
            Terminal.ForegroundColor = ConsoleColor.Black;
            // Terminal.Write(Time.DisplayText);

            // Boss warning
            if (counter % 3 == 0 && counter >- 0 && y < map.Height)
            {
                BossAttackSpike(2, y, boss.warning);
                y++;
            }

            // Boss attack
            if (counter2 % 3 == 0 && counter2 >= 0 && y2 < map.Height)
            {
                BossAttackSpike(2, y2, boss.spike);
                y2++;
            }

            // Reset boss attack tiles
            if (counter2 >= 45)
            {                
                for (int y = 0; y < map.Height; y++)
                {
                    ResetBossAttacks(2, y);
                }
            }

            counter++;
            counter2++;
        }

        void BossAttackSpike(int x, int y, ColoredText emoji)
        {
            map.Poke(x, y, emoji);
        }

        void ResetBossAttacks(int x, int y)
        {
            map.Poke(x, y, map.Get(x, y));
        }
        void CheckMovePlayer()
        {
            //
            inputChanged = false;
            oldPlayerX = playerX;
            oldPlayerY = playerY;

            if (Input.IsKeyPressed(ConsoleKey.RightArrow) || Input.IsKeyPressed(ConsoleKey.D)) playerX++;
            if (Input.IsKeyPressed(ConsoleKey.LeftArrow)  || Input.IsKeyPressed(ConsoleKey.A)) playerX--;
            if (Input.IsKeyPressed(ConsoleKey.DownArrow)  || Input.IsKeyPressed(ConsoleKey.S)) playerY++;
            if (Input.IsKeyPressed(ConsoleKey.UpArrow)    || Input.IsKeyPressed(ConsoleKey.W)) playerY--;

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
