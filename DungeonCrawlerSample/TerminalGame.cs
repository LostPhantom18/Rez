using DungeonCrawlerSample;
using System;

namespace MohawkTerminalGame
{
    public class TerminalGame
    {
        // ─────────────────────────────────────────────────────────────────────
        // SETTINGS WE WILL TWEAK IN THE GAME
        // ─────────────────────────────────────────────────────────────────────

        // Each cell is made only using two things, the · and • 
        const int CELL_W = 2;

        // Outer grid is the one highlighted brighter and will change throughout the game while inner grid will always be 3
        const int OUTER_GRID = 5; // number of big tiles 
        const int INNER_GRID = 3; // number of mini-cells inside of each big tile

        // Map size OUTER_GRID * INNER_GRID (+1 because for some reason it displays one less)
        const int MAP_WIDTH = OUTER_GRID * INNER_GRID + 1; // = 15 in the game
        const int MAP_HEIGHT = OUTER_GRID * INNER_GRID + 1; // = 15 in the game

        // Dot colors (console only supports 16 colors theres no way for hexidecimals; grayscale is fastest and looks the best imo)

        // ─────────────────────────────────────────────────────────────────────
        // GAME RUNTIME STATE
        // ─────────────────────────────────────────────────────────────────────

        TerminalGridWithColor map = null!; // The 2D array we are drawing into, then rendering

        // ─────────────────────────────────────────────────────────────────────
        // EMOJI STORAGE
        // ─────────────────────────────────────────────────────────────────────

        // --- Floor and Wall ---
        // Floor and wall tiles are 2 characters wide because Emojis are 2 characters wide
        ColoredText floorTile = new(@"· ", ConsoleColor.White, ConsoleColor.Black);
        ColoredText wallTile = new(@"• ", ConsoleColor.White, ConsoleColor.Black);

        // --- Player ---
        ColoredText player = new(@"💀", ConsoleColor.White, ConsoleColor.Black);
        ColoredText gem = new(@"💎", ConsoleColor.White, ConsoleColor.Black);
        ColoredText shield = new(@"🛡️", ConsoleColor.White, ConsoleColor.Black);
        ColoredText sword = new(@"⚔️", ConsoleColor.White, ConsoleColor.Black);

        // --- Boss ---
        public ColoredText warning = new(@"⚠️", ConsoleColor.Yellow, ConsoleColor.Black);
        public ColoredText spike = new(@"💥", ConsoleColor.Red, ConsoleColor.Black);
        public ColoredText lightning = new(@"⚡", ConsoleColor.Yellow, ConsoleColor.Black);
        public ColoredText wave = new(@"🌊", ConsoleColor.Blue, ConsoleColor.Black);

        // Input recording so we only need to redraw when neccessary
        bool inputChanged;
        int oldPlayerX, oldPlayerY;
        int playerX = MAP_WIDTH / 2; // Start in center (no real center cause its 15x15)
        int playerY = MAP_HEIGHT / 2;

        // Temp boss code
        int counter;
        int counter2;
        int bossAttackY;
        int bossAttackY2;
        int bossAttackX;

        // ─────────────────────────────────────────────────────────────────────
        // ENGINE STUFF
        // ─────────────────────────────────────────────────────────────────────

        /// Run once before Execute begins
        public void Setup()
        {
            // Run the game steady by using the timer loop (made by raph)
            Program.TerminalExecuteMode = TerminalExecuteMode.ExecuteTime;
            Program.TerminalInputMode = TerminalInputMode.EnableInputDisableReadLine;
            Program.TargetFPS = 60;

            // Making the console perty
            Terminal.SetTitle("Wizard Tower");
            Terminal.CursorVisible = false;
            Terminal.BackgroundColor = ConsoleColor.Black;

            // Build a new 15×15 grid (each cell makes two columns)
            // floorTile = new ColoredText("  ", FLOOR_FG, FLOOR_BG);
            map = new TerminalGridWithColor(MAP_WIDTH, MAP_HEIGHT, floorTile);

            for (int i = 0; i < MAP_WIDTH; i += 3)
            {
                map.SetCol(wallTile, i);
                map.SetRow(wallTile, i);
            }

            // Rendering
            map.ClearWrite();

            // Put player on top 
            DrawCharacter(playerX, playerY, player);

            counter = -Program.TargetFPS;
            counter2 = -Program.TargetFPS * 2;
            bossAttackY = 0;
            bossAttackY2 = 0;
            RandomizeBossX();
        }

        public void Execute()
        {
            // Does input and move player one cell at a time using WASD or the arrows
            CheckMovePlayer();

            if (inputChanged)
            {
                // Make sure to replace what was under the old player like the floor and dots and stuff
                ResetCell(oldPlayerX, oldPlayerY);
                // Then put player in new spot
                DrawCharacter(playerX, playerY, player);
                inputChanged = false;
            }

            // Basic HUD will be changed for sure
            // Terminal.SetCursorPosition(0, MAP_HEIGHT + 1);
            // Terminal.ResetColor();
            // Terminal.ForegroundColor = ConsoleColor.Black;
            // Terminal.Write(Time.DisplayText);

            // ─────────────────────────────────────────────────────────────────────
            // BOSS ATTACK CODE LOOPS
            // ─────────────────────────────────────────────────────────────────────

            // Boss warning
            // Scuffed hardcoded timer code in the if
            // ISAAC - FIX THE COUNTER CODE TO MAKE IT MODULAR
            if (counter % 3 == 0 && counter > -0 && bossAttackY < map.Height)
            {
                BossAttackSpike(bossAttackX, bossAttackY, warning);
                bossAttackY++;
            }

            // Boss attack
            // ISAAC - FIX THE COUNTER CODE TO MAKE IT MODULAR
            if (counter2 % 3 == 0 && counter2 >= 0 && bossAttackY2 < map.Height)
            {
                BossAttackSpike(bossAttackX, bossAttackY2, spike);
                bossAttackY2++;
            }

            // Reset boss attack tiles
            // ISAAC - FIX THE COUNTER CODE TO MAKE IT MODULAR
            if (counter2 >= Program.TargetFPS)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    ResetBossAttacks(bossAttackX, y);
                }
                RandomizeBossX();
            }

            // ISAAC - FIX THE COUNTER CODE TO MAKE IT MODULAR
            counter++;
            counter2++;

            // ─────────────────────────────────────────────────────────────────────
            // DISPLAY
            // ─────────────────────────────────────────────────────────────────────

            Terminal.SetCursorPosition(0, MAP_HEIGHT + 1);
            Terminal.ResetColor();
            Terminal.ForegroundColor = ConsoleColor.Black;
            Terminal.Write($"Time: {Time.DisplayText}   Pos({playerX + 1},{playerY + 1})   ");
        }

        // ─────────────────────────────────────────────────────────────────────
        // BOSS ATTACKS / AI METHODS
        // ─────────────────────────────────────────────────────────────────────

        // The Boss uses the spike attack
        void BossAttackSpike(int x, int y, ColoredText emoji)
        {
            map.Poke(x, y, emoji);
        }

        // Set the tiles the boss just attacked back to the normal tileset
        void ResetBossAttacks(int x, int y)
        {
            map.Poke(x, y, map.Get(x / 2, y));
            // ISAAC - FIX THE COUNTER CODE TO MAKE IT MODULAR
            counter = -60;
            counter2 = -120;
            bossAttackY = 0;
            bossAttackY2 = 0;
        }

        // Randomize boss attack position
        // Still kinda scuffed, should be fixed when attacks are modular
        // ISAAC - FIX THE COUNTER CODE TO MAKE IT MODULAR
        void RandomizeBossX()
        {
            // MAP_WIDTH * 2 because emojis are 2-wide characters
            bossAttackX = Random.Integer(0, MAP_WIDTH) * 2;
        }

        // ─────────────────────────────────────────────────────────────────────
        // INPUT / MOVEMENT
        // ─────────────────────────────────────────────────────────────────────

        // Player movement stuff made by Raph; Modified by Isaac
        void CheckMovePlayer()
        {
            inputChanged = false;
            oldPlayerX = playerX;
            oldPlayerY = playerY;

            if (Input.IsKeyPressed(ConsoleKey.RightArrow) || Input.IsKeyPressed(ConsoleKey.D)) playerX++;
            if (Input.IsKeyPressed(ConsoleKey.LeftArrow) || Input.IsKeyPressed(ConsoleKey.A)) playerX--;
            if (Input.IsKeyPressed(ConsoleKey.DownArrow) || Input.IsKeyPressed(ConsoleKey.S)) playerY++;
            if (Input.IsKeyPressed(ConsoleKey.UpArrow) || Input.IsKeyPressed(ConsoleKey.W)) playerY--;

            playerX = Math.Clamp(playerX, 0, MAP_WIDTH - 1);
            playerY = Math.Clamp(playerY, 0, MAP_HEIGHT - 1);

            if (oldPlayerX != playerX || oldPlayerY != playerY)
                inputChanged = true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // DRAWING HELP
        // ─────────────────────────────────────────────────────────────────────

        // Raph Code
        void DrawCharacter(int x, int y, ColoredText character)
        {
            if (x < 0 || x >= MAP_WIDTH || y < 0 || y >= MAP_HEIGHT) return;

            var under = map.Get(x, y);           // Read what’s in the backing array
            character.bgColor = under.bgColor;   // Then match the background
            map.Poke(x * CELL_W, y, character);  // Draw at the correct screen column
        }

        // Raph Code
        void ResetCell(int x, int y)
        {
            if (x < 0 || x >= MAP_WIDTH || y < 0 || y >= MAP_HEIGHT) return;

            var tile = map.Get(x, y);
            map.Poke(x * CELL_W, y, tile);
        }
    }
}