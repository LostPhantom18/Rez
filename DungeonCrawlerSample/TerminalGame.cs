using DungeonCrawlerSample;
using DungeonCrawlerSample.MohawkTerminalGame.NewClasses;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Dynamic;

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

        // Jonahs Stuff
        bool testDying = false; // Turn to true if you want to take damage over time
        private bool gameOverScreenDrawn = false;
        int startTimeToDie = 0;
        int endTimeToDie = 3;
        int maxHealth = 5;
        int health = 5;
        int xValueForHearts = 15;
        private int lastDrawnHealth = -1;
        bool gameOver = false;
        private DateTime damageTimerStart; // Tracks when last damage was applied
        private float damageInterval = 1f; // every 3 seconds
        private float damageElapsed = 0f; // total seconds passed
        bool gameOverPrinted = false;
        // Jonahs Stuff

        // HUD tracking to prevent flicker
        private int hudRowTimePos = MAP_HEIGHT + 1; // row for Time / Pos (fixed)
        private int hudRowColumnRow = MAP_HEIGHT + 2; // row for Column/Row display
        private string lastTimePosText = string.Empty;
        private string lastColRowText = string.Empty;

        string[] rightCharacterArt = new string[]
{
    "   ^  ",
    "_/  \\_ ",
    "(0 = 0)"
};

        // Joanhs Stuff

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
        ColoredText floorTile = new(@"  ", ConsoleColor.White, ConsoleColor.Black);
        ColoredText wallTile = new(@"██", ConsoleColor.White, ConsoleColor.Black);

        // --- Player ---
        ColoredText player = new(@"💀", ConsoleColor.White, ConsoleColor.Black);
        ColoredText gem = new(@"💎", ConsoleColor.White, ConsoleColor.Black);
        ColoredText shield = new(@"🛡️", ConsoleColor.White, ConsoleColor.Black);
        ColoredText sword = new(@"⚔️", ConsoleColor.White, ConsoleColor.Black);

        // --- Boss ---
        ColoredText warning = new(@"⚠️", ConsoleColor.Yellow, ConsoleColor.Black);
        ColoredText spike = new(@"💥", ConsoleColor.Red, ConsoleColor.Black);
        ColoredText lightning = new(@"⚡", ConsoleColor.Yellow, ConsoleColor.Black);
        ColoredText wave = new(@"🌊", ConsoleColor.Blue, ConsoleColor.Black);

        // --- Player Health ---
        ColoredText playerHeart = new(@"💀", ConsoleColor.Red, ConsoleColor.Black);

        // Input recording so we only need to redraw when neccessary
        bool inputChanged;
        int oldPlayerX, oldPlayerY;
        int playerX = MAP_WIDTH / 2; // Start in center (no real center cause its 15x15)
        int playerY = MAP_HEIGHT / 2;

        // ─────────────────────────────────────────────────────────────────────
        // BOSS AI STORAGE
        // ─────────────────────────────────────────────────────────────────────

        int bossSpikeTimer;      // Timer for spike attack
        int bossLightningTimer;  // Timer for lightning attack
        int bossWaveTimer;       // Timer for wave attack
        int bossAttackCounter;   // Counter for attackinf parts of attacks

        int bossWarningRow;      // Warning row (the 'Y' value)
        int bossWarningCol;      // Warning column (the 'X' value)
        int bossAttackRow;       // Attack row (the 'Y' value)
        int bossAttackCol;       // Attack column (the 'X' value)
        int bossAttackColPos;    // Attack column position (the 'X' value)
        int bossAttackRowPos;    // Attck row position (the 'Y' value)

        bool isBossAttacking;               // Boss attacking state for lockout
        bool isSpikeVertical;               // Which way the spike attack should go
        int spikePositionCounter;           // Counts what row or column the spikes are on for removal
        int lightningSize;                  // How many rows should the lightning affect
        int waveRow;                        // The current (haha) row the wave is at
        bool isWaveAttackOver;              // The wave attack progress status
        String currentAttack = "";          // Which attack the boss is currently using

        bool[,] attackArray;                 // Memory for where the boss is currently attacking

        // ─────────────────────────────────────────────────────────────────────
        // ENGINE STUFF
        // ─────────────────────────────────────────────────────────────────────

        /// Run once before Execute begins
        public void Setup()
        {
            damageTimerStart = DateTime.Now;
            // Run the game steady by using the timer loop (made by raph)
            Program.TerminalExecuteMode = TerminalExecuteMode.ExecuteTime;
            Program.TerminalInputMode = TerminalInputMode.EnableInputDisableReadLine;
            Program.TargetFPS = 60;

            // Making the console perty
            Terminal.SetTitle("Wizard Tower");
            Terminal.CursorVisible = false;
            Terminal.BackgroundColor = ConsoleColor.Black;

            if (gameOver == false)
            {


                // Build a new 15×15 grid (each cell makes two columns)
                map = new TerminalGridWithColor(MAP_WIDTH, MAP_HEIGHT, floorTile);

                //for (int i = 0; i < MAP_WIDTH; i += 3)
                //{
                map.SetCol(wallTile, 0);
                map.SetCol(wallTile, MAP_WIDTH - 1);
                map.SetRow(wallTile, 0);
                map.SetRow(wallTile, MAP_HEIGHT - 1);
                //}

                // Initialize the attack array to false
                attackArray = new bool[map.Width * 2, map.Height];
                for (int x = 0; x < map.Width * 2; x++)
                {
                    for (int y = 0; y < map.Height; y++)
                    {
                        attackArray[x, y] = false;
                    }
                }

                // Rendering
                map.ClearWrite();

                // Put player on top 
                DrawCharacter(playerX, playerY, player);

                // Set up boss attacks
                RandomizeBossColumn();
                RandomizeBossRow();
                isSpikeVertical = true;

                // Set up sword items
                swordParts[0] = gem;
                swordParts[1] = shield;
                swordParts[2] = sword;

                nextSwordSpawn = Random.Integer(10 * Program.TargetFPS, 20 * Program.TargetFPS + 1);

                // Draw static right-side ASCII character (only once)
                int mapWidth = MAP_WIDTH * CELL_W;
                int margin = 5;
                int characterX = mapWidth + margin;
                int characterY = 5;
                DrawAsciiCharacter(characterX, characterY, rightCharacterArt, ConsoleColor.Red);
            }

        }

        public void Execute()
        {

            if (testDying)
            {
                damageElapsed += 1f / Program.TargetFPS;

                if (damageElapsed >= damageInterval && health > 0)
                {
                    ChangeHealth(-1);
                    damageElapsed = 0f;
                }
            }
            
            if (gameOver)
            {
                DrawGameOverScreen();
                return; // Stop all other updates
            }
            CheckIfDead();
            if (gameOver == false)
            {


                // Does input and move player one cell at a time using WASD or the arrows
                CheckMovePlayer();

                if (inputChanged)
                {
                    // Make sure to replace what was under the old player like the floor and dots and stuff
                    ResetCell(oldPlayerX, oldPlayerY);
                    // Then put player in new spot
                    inputChanged = false;
                }

                SwordPartsTick();

                // Basic HUD will be changed for sure
                // Terminal.SetCursorPosition(0, MAP_HEIGHT + 1);
                // Terminal.ResetColor();
                // Terminal.ForegroundColor = ConsoleColor.Black;
                // Terminal.Write(Time.DisplayText);

                // ─────────────────────────────────────────────────────────────────────
                // BOSS ATTACK CODE LOOPS
                // ATTACKS HAVE INFO INSIDE
                // ─────────────────────────────────────────────────────────────────────

                // Boss choosing what attack to do
                // Check if the boss can attack

                // Dev button for boss attack toggles
                if (Input.IsKeyDown(ConsoleKey.H))
                {
                    // Randomize what attack the boss is going to use
                    // !!! CHANGE THE 3 TO CHECK WHAT STAGE OF THE FIGHT THE BOSS IS IN !!!
                    int attackToUse = Random.Integer(0, 3);
                    //int attackToUse = 0;

                    if (!isBossAttacking)
                    {
                        ResetBossAttackingState();
                        // Spike attack settings
                        if (attackToUse == 0)
                        {
                            currentAttack = "spike";
                            // Randomize where the boss will attack
                            RandomizeBossColumn();
                            RandomizeBossRow();
                            spikePositionCounter = 0;
                            // Determine which direction to shoot the spikes
                            isSpikeVertical = Random.CoinFlip();
                        }

                        // Lightning attack settings
                        if (attackToUse == 1)
                        {
                            currentAttack = "lightning";
                            // Randomize where the boss will attack
                            RandomizeBossColumn();
                            // Determine how far down to shoot the lightning
                            lightningSize = Random.Integer(7, 13);
                        }

                        // Wave attack settings
                        if (attackToUse == 2)
                        {
                            currentAttack = "wave";
                            // Randomize where the boss will attack
                            RandomizeBossColumn();
                            // Reset the wave row
                            waveRow = MAP_HEIGHT - 1;
                        }

                        isBossAttacking = true;
                    }
                }

                // Many nested ifs for attacks because I don't want to work with classes or state machine for this - Isaac
                // Only use attacks if the boss is attacking
                if (isBossAttacking)
                {
                    // Start of Spike attack
                    if (currentAttack == "spike")
                    {
                        /**
                         * Attack Steps:
                         * 1. Coin flip to choose vertical or horizontal spike line
                         *    Spike line is 3 columns wide
                         * 2. Warn where the spikes will show up
                         * 3. Spikes show up in warning spots
                         * 4. Reset the tiles affected and boss attacking state
                         */

                        // Boss SPIKE ATTACK - VERTICAL
                        if (isSpikeVertical)
                        {
                            // Boss warning
                            if (bossSpikeTimer % 3 == 0 && bossAttackCounter > 0 && bossWarningRow < map.Height)
                            {
                                // Warn the left row, selected row, and right row
                                BossAttackEmoji(bossAttackColPos - 2, bossWarningRow, warning, false);
                                BossAttackEmoji(bossAttackColPos, bossWarningRow, warning, false);
                                BossAttackEmoji(bossAttackColPos + 2, bossWarningRow, warning, false);
                                bossWarningRow++;
                            }

                            // Boss attack
                            if (bossSpikeTimer % 3 == 0 && bossAttackCounter >= 60 && bossAttackRow < map.Height)
                            {
                                // Attack the left row, selected row, and right row
                                BossAttackEmoji(bossAttackColPos - 2, bossAttackRow, spike, true);
                                BossAttackEmoji(bossAttackColPos, bossAttackRow, spike, true);
                                BossAttackEmoji(bossAttackColPos + 2, bossAttackRow, spike, true);
                                bossAttackRow++;
                            }

                            // Reset boss attack tiles
                            if (bossSpikeTimer % 3 == 0 && bossAttackCounter >= 120 && spikePositionCounter < map.Height)
                            {
                                // Reset the left row, selected row, and right row
                                ResetBossAttackTiles(bossAttackColPos - 2, spikePositionCounter);
                                ResetBossAttackTiles(bossAttackColPos, spikePositionCounter);
                                ResetBossAttackTiles(bossAttackColPos + 2, spikePositionCounter);
                                spikePositionCounter++;

                                if (spikePositionCounter >= map.Height) isBossAttacking = false;
                            }

                            // Increase time counter while the boss is using an attack
                            bossSpikeTimer++;
                            bossAttackCounter++;
                        }
                        // Boss SPIKE ATTACK - HORIZONTAL
                        else
                        {
                            // Boss warning
                            if (bossSpikeTimer % 3 == 0 && bossAttackCounter > 0 && bossWarningCol < map.Width)
                            {
                                // Warn the left column, selected column, and right column
                                BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos - 1, warning, false);
                                BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos, warning, false);
                                BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos + 1, warning, false);
                                bossWarningCol++;
                            }

                            // Boss attack
                            if (bossSpikeTimer % 3 == 0 && bossAttackCounter >= 60 && bossAttackCol < map.Width)
                            {
                                // Attack the left column, selected column, and right column
                                BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos - 1, spike, true);
                                BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos, spike, true);
                                BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos + 1, spike, true);
                                bossAttackCol++;
                            }

                            // Reset boss attack tiles
                            if (bossSpikeTimer % 3 == 0 && bossAttackCounter >= 120 && spikePositionCounter < map.Width * 2)
                            {
                                // Reset the left column, selected column, and right columny
                                ResetBossAttackTiles(spikePositionCounter * 2, bossAttackRowPos - 1);
                                ResetBossAttackTiles(spikePositionCounter * 2, bossAttackRowPos);
                                ResetBossAttackTiles(spikePositionCounter * 2, bossAttackRowPos + 1);
                                spikePositionCounter++;

                                if (spikePositionCounter >= map.Width * 2) isBossAttacking = false;
                            }

                            // Increase time counter while the boss is using an attack
                            bossSpikeTimer++;
                            bossAttackCounter++;
                        }
                    } // End of Spike attack

                    // Start of Lightning attack
                    if (currentAttack == "lightning")
                    {
                        /**
                         * Attack Steps:
                         * 1. Choose which columns to affect (choose 1, then every other for x amount of columns)
                         *    Lightning is 1 tile wide, but affects 9 rows at every other row
                         * 2. Warn where the lightning will strike
                         * 3. Shoot lightning on the warning tiles
                         * 4. Reset tiles affected by attack
                         */

                        // Boss warning
                        if (bossLightningTimer % 2 == 0 && bossAttackCounter > 0 && bossWarningRow < lightningSize)
                        {
                            // Warn the far left column, left column, selected column, right column, and far right column
                            BossAttackEmoji(bossAttackColPos - 8, bossWarningRow, warning, false);
                            BossAttackEmoji(bossAttackColPos - 4, bossWarningRow, warning, false);
                            BossAttackEmoji(bossAttackColPos, bossWarningRow, warning, false);
                            BossAttackEmoji(bossAttackColPos + 4, bossWarningRow, warning, false);
                            BossAttackEmoji(bossAttackColPos + 8, bossWarningRow, warning, false);
                            bossWarningRow++;
                        }

                        // Boss attack
                        if (bossLightningTimer % 2 == 0 && bossAttackCounter >= 45 && bossAttackRow < lightningSize)
                        {
                            // Attack the far left column, left column, selected column, right column, and far right column
                            BossAttackEmoji(bossAttackColPos - 8, bossAttackRow, lightning, true);
                            BossAttackEmoji(bossAttackColPos - 4, bossAttackRow, lightning, true);
                            BossAttackEmoji(bossAttackColPos, bossAttackRow, lightning, true);
                            BossAttackEmoji(bossAttackColPos + 4, bossAttackRow, lightning, true);
                            BossAttackEmoji(bossAttackColPos + 8, bossAttackRow, lightning, true);
                            bossAttackRow++;
                        }

                        // Reset boss attack tiles
                        if (bossAttackCounter >= 70)
                        {
                            for (int y = 0; y < lightningSize; y++)
                            {
                                // Reset the far left column, left column, selected column, right column, and far right column
                                ResetBossAttackTiles(bossAttackColPos - 8, y);
                                ResetBossAttackTiles(bossAttackColPos - 4, y);
                                ResetBossAttackTiles(bossAttackColPos, y);
                                ResetBossAttackTiles(bossAttackColPos + 4, y);
                                ResetBossAttackTiles(bossAttackColPos + 8, y);
                            }
                            isBossAttacking = false;
                        }

                        // Increase time counter while the boss is using an attack
                        bossLightningTimer++;
                        bossAttackCounter++;

                    } // End of Lightning attack

                    // Start of Wave attack
                    if (currentAttack == "wave")
                    {
                        /**
                         * Attack Steps:
                         * 1. Choose vertical area to be safe
                         * 2. Warning on all other rows or columns that will be affected
                         * 3. Waves on the warnings
                         * 4. Recede the waves
                         * 5. Finish attack
                         */
                        // Boss warning
                        if (bossWaveTimer % 4 == 0 && bossAttackCounter > 0 && bossWarningRow < map.Height)
                        {
                            // Warn all columns except the 2 to the left and right where the attack was chosen
                            for (int i = 0; i < map.Width * 2; i += 2)
                            {
                                // Skip the safe area
                                if (i == bossAttackColPos - 4 ||
                                    i == bossAttackColPos - 2 ||
                                    i == bossAttackColPos ||
                                    i == bossAttackColPos + 2 ||
                                    i == bossAttackColPos + 4)
                                    continue;
                                BossAttackEmoji(i, bossWarningRow, warning, false);
                            }
                            bossWarningRow++;
                        }

                        // Boss attack
                        if (bossWaveTimer % 6 == 0 && bossAttackCounter >= 80 && bossAttackRow < map.Height)
                        {
                            // Attack the warned rows from above
                            for (int i = 0; i < map.Width * 2; i += 2)
                            {
                                // Skip the safe area
                                if (i == bossAttackColPos - 4 ||
                                    i == bossAttackColPos - 2 ||
                                    i == bossAttackColPos ||
                                    i == bossAttackColPos + 2 ||
                                    i == bossAttackColPos + 4)
                                    continue;
                                BossAttackEmoji(i, bossAttackRow, wave, true);
                            }
                            bossAttackRow++;
                        }

                        // Reset boss attack tiles by reversing the attack
                        if (bossWaveTimer % 4 == 0 && bossAttackCounter >= 200 && waveRow >= 0)
                        {
                            for (int i = 0; i < map.Width * 2; i += 2)
                            {
                                // Skip the safe area
                                if (i == bossAttackColPos - 4 ||
                                    i == bossAttackColPos - 2 ||
                                    i == bossAttackColPos ||
                                    i == bossAttackColPos + 2 ||
                                    i == bossAttackColPos + 4)
                                    continue;

                                ResetBossAttackTiles(i, waveRow);
                            }
                            waveRow--;

                            if (waveRow < 0) isBossAttacking = false; // When the wave has fully retracted
                        }

                        // Increase time counter while the boss is using an attack
                        bossWaveTimer++;
                        bossAttackCounter++;

                    } // End of Wave attack

                }
                // ─────────────────────────────────────────────────────────────────────
                // DISPLAY
                // ─────────────────────────────────────────────────────────────────────

                // Draw the player so they are always on top of the action
                DrawCharacter(playerX, playerY, player);

                // Player takes damage if they are in a cell with an attack (buffer needed?)
                if (PlayerInDanger())
                {
                    ChangeHealth(-1);
                    ResetBossAttackTiles(playerX * 2, playerY);
                }

                /*
                Terminal.SetCursorPosition(0, MAP_HEIGHT + 1);
                Terminal.ResetColor();
                Terminal.ForegroundColor = ConsoleColor.White;
                Terminal.WriteLine($"Time: {Time.DisplayText}   Pos({playerX + 1},{playerY + 1})   ");
                Terminal.ClearLine();
                Terminal.Write($"Column:{bossAttackColPos} Row:{bossAttackRowPos}");
                Terminal.ForegroundColor = ConsoleColor.Black;
                Terminal.SetCursorPosition(0, MAP_HEIGHT + 5);
                */
                UpdateHUD();

                if (health != lastDrawnHealth)
                {
                    UpdateHearts();
                    lastDrawnHealth = health;
                }

                // Clear stray input characters
                Terminal.ForegroundColor = ConsoleColor.Black;
                Console.SetCursorPosition(0, MAP_HEIGHT + 6);
                Console.Write(new string(' ', 800));
                Console.SetCursorPosition(0, MAP_HEIGHT + 6);
            }
            else
            {
                Debug.Write("Game Over");
                //return;
            }

        }

        private void DrawGameOverScreen()
        {
            if (!gameOverScreenDrawn)
            {
                Console.Clear();
                int centerX = MAP_WIDTH;
                int centerY = MAP_HEIGHT / 2;

                Console.ForegroundColor = ConsoleColor.Red;

                string[] gameOverText = new string[]
                {
            "█████████████████████",
            "█     GAME OVER     █",
            "█  Press R to Retry █",
            "█  Press Q to Quit  █",
            "█████████████████████"
                };

                for (int i = 0; i < gameOverText.Length; i++)
                {
                    Console.SetCursorPosition(centerX - gameOverText[i].Length / 2, centerY - gameOverText.Length / 2 + i);
                    Console.Write(gameOverText[i]);
                }

                Console.ResetColor();
                gameOverScreenDrawn = true;
            }

            // Input handling stays the same
            if (Input.IsKeyPressed(ConsoleKey.R))
            {
                RestartGame();
                gameOverScreenDrawn = false;
            }
            else if (Input.IsKeyPressed(ConsoleKey.Q))
            {
                Environment.Exit(0);
            }
        }
        private void RestartGame()
        {
            health = maxHealth;
            lastDrawnHealth = -1;
            damageElapsed = 0f;
            gameOver = false;

            // Reset player position
            playerX = MAP_WIDTH / 2;
            playerY = MAP_HEIGHT / 2;

            // Reset boss
            ResetBossAttackingState();

            // Clear map manually
            ClearMap();

            // Re-setup player, sword, etc.
            DrawCharacter(playerX, playerY, player);
            Setup();
        }
        private void ClearMap()
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                for (int x = 0; x < MAP_WIDTH; x++)
                {
                    map.Poke(x * CELL_W, y, map.Get(x, y));
                }
            }
        }
        void DrawAsciiCharacter(int startX, int startY, string[] art, ConsoleColor color)
        {
            Console.ForegroundColor = color;

            for (int y = 0; y < art.Length; y++)
            {
                Console.SetCursorPosition(startX, startY + y);
                Console.Write(art[y]);
            }

            Console.ResetColor();

            // Fix: move cursor somewhere safe (below HUD)
            Console.SetCursorPosition(0, MAP_HEIGHT + 6);
        }

        private void UpdateHUD(bool force = false)
        {
            // Build the two HUD lines
            string timeText = Time.DisplayText;
            string posText = $"Pos({playerX + 1},{playerY + 1})";
            string timePosFull = $"Time: {timeText}   {posText}";

            string colRowText = $"Is player hit:";

            // Only update if changed (or forced)
            if (force || timePosFull != lastTimePosText)
            {
                lastTimePosText = timePosFull;
                // write padded text so leftover chars are cleared
                string padded = timePosFull.PadRight(60, ' ');

                Terminal.SetCursorPosition(0, hudRowTimePos);
                Terminal.ResetColor();
                Terminal.ForegroundColor = ConsoleColor.White;
                Terminal.Write(padded);
            }

            if (force || colRowText != lastColRowText)
            {
                lastColRowText = colRowText;
                string padded2 = colRowText.PadRight(60, ' ');

                Terminal.SetCursorPosition(0, hudRowColumnRow);
                Terminal.ResetColor();
                Terminal.ForegroundColor = ConsoleColor.White;
                Terminal.Write(padded2);
            }
        }
        // ─────────────────────────────────────────────────────────────────────
        // BOSS ATTACKS / AI METHODS
        // ─────────────────────────────────────────────────────────────────────

        // The Boss uses an attack
        void BossAttackEmoji(int x, int y, ColoredText emoji, bool isAttack)
        {
            // Check if the position is in bounds, quit if not
            if (x < 0 || x > (map.Width - 1) * 2 || y < 0 || y > map.Height) return;

            map.Poke(x, y, emoji);
            if (isAttack) attackArray[x, y] = true;
        }

        // Set the tiles the boss just attacked back to the normal tileset
        void ResetBossAttackTiles(int x, int y)
        {
            // Check if the position is in bounds, quit if not
            if (x < 0 || x > (map.Width - 1) * 2 || y < 0 || y > map.Height) return;
            map.Poke(x, y, map.Get(x / 2, y));
            attackArray[x, y] = false;
        }

        // Randomize boss attack column (X value)
        void RandomizeBossColumn()
        {
            // Multiply by 2 because emojis are 2-wide characters
            bossAttackColPos = Random.Integer(0, MAP_WIDTH) * 2;
        }

        // Randomize boss attack row (Y value)
        void RandomizeBossRow()
        {
            bossAttackRowPos = Random.Integer(0, 15);
        }

        // Resets the boss attacking state so it can attack again
        void ResetBossAttackingState()
        {
            isBossAttacking = false;
            currentAttack = "";
            bossAttackCounter = 0;

            bossWarningRow = 0;
            bossAttackRow = 0;

            bossWarningCol = 0;
            bossAttackCol = 0;

            bossSpikeTimer = 0;
            bossLightningTimer = 0;
            bossWaveTimer = 0;
        }

        // Checks if the player is in the same square as an attack
        public bool PlayerInDanger()
        {
            // 
            if (attackArray[playerX * 2, playerY]) return true; 
            return false;
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

        // ─────────────────────────────────────────────────────────────────────
        // SWORD SPAWNING SYSTEM
        // ─────────────────────────────────────────────────────────────────────

        int swordTimer = 0; // Counts frames (converts to seconds)                
        int nextSwordSpawn = 0; // Frames (seconds) until next spawn
        int swordX = -1; // Cell position -1 just off the map 
        int swordY = -1; // Cell position -1 just off the map     
        ColoredText currentSword = null!; // Which emoji is spawned (while the current item is not nothing)

        ColoredText[] swordParts = new ColoredText[3]; // Array for emojis (sword parts)
        int swordIndex = 0; // To know what part of the sword you are on

        void SwordPartsTick()
        {
            // If a part is on the map then make it and allow it to be picked up
            if (swordX != -1)
            {

                // Each cell is two columns wide
                map.Poke(swordX * CELL_W, swordY, currentSword);

                // Pick it up when the player is on it
                if (playerX == swordX && playerY == swordY)
                {
                    DrawCharacter(swordX, swordY, player);  // Reset it to original .
                    swordX = -1; // Cell position back to -1 just off the map
                    swordY = -1; // Cell position back to -1 just off the map

                    swordIndex++; // +1 to swordIndex for next sword part

                    if (swordIndex >= swordParts.Length)
                    {
                        swordIndex = 0;
                    }
                }

                return; // Dont allow the timer to go when the sword part is on the grid
            }

            // No part active so count up every frame
            swordTimer++;

            // If the swordTimer is greater than or equal to the nextSwordSpawn time that is randomized from 10, 20 sec 
            if (swordTimer >= nextSwordSpawn)
            {
                currentSword = swordParts[swordIndex];

                // Pick a random tile on the map
                swordX = Random.Integer(1, MAP_WIDTH - 1);
                swordY = Random.Integer(1, MAP_HEIGHT - 1);


                // Reset the timer back to 0 to start the counter over again
                swordTimer = 0;
                nextSwordSpawn = Random.Integer(10 * Program.TargetFPS, 20 * Program.TargetFPS + 1);
            }
        }
        void CheckIfDead()
        {
            if (health <= 0)
            {
                gameOver = true;
            }
        }
        private void UpdateHearts() // Updates UI
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(0, MAP_HEIGHT + 3);

            // Clear the line
            Console.Write(new string(' ', 60));
            Console.SetCursorPosition(0, MAP_HEIGHT + 3);

            // Build hearts string
            string hearts = new string('♥', Math.Max(0, health)); // just in case
            string empty = new string('♡', Math.Max(0, maxHealth - health));

            Console.Write($"Health: {hearts}{empty}");

            Console.ResetColor();
        }

        private void ChangeHealth(int amount) // Can be used to damage player
        {
            int oldHealth = health;
            health = Math.Clamp(health + amount, 0, maxHealth);

            if (health != oldHealth)
            {
                lastDrawnHealth = -1; // force update
            }
        }



    }
}