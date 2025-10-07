using DungeonCrawlerSample;
using DungeonCrawlerSample.MohawkTerminalGame.NewClasses;
using System;
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
        int lightningSize;                  // How many rows should the lightning affect
        String currentAttack = "";          // Which attack the boss is currently using

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
            map = new TerminalGridWithColor(MAP_WIDTH, MAP_HEIGHT, floorTile);

            //for (int i = 0; i < MAP_WIDTH; i += 3)
            //{
            map.SetCol(wallTile, 0);
            map.SetCol(wallTile, MAP_WIDTH - 1);
            map.SetRow(wallTile, 0);
            map.SetRow(wallTile, MAP_HEIGHT - 1);
            //}

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
                isBossAttacking = true;
                // Randomize what attack the boss is going to use
                // !!! CHANGE THE 3 TO CHECK WHAT STAGE OF THE FIGHT THE BOSS IS IN !!!
                //int attackToUse = Random.Integer(0, 3); 
                int attackToUse = 1;

                if (attackToUse == 0)
                {
                    currentAttack = "spike";
                    // Determine which direction to shoot the spikes
                    isSpikeVertical = Random.CoinFlip();
                }

                if (attackToUse == 1)
                {
                    currentAttack = "lightning";
                    // Determine how far down to shoot the lightning
                    lightningSize = Random.Integer(5, 10);
                }

                if (attackToUse == 2) currentAttack = "wave";
            }

            // Many nested ifs for attacks because I don't want to work with classes or state machine for this - Isaac
            // Start of Spike attack
            if (currentAttack == "spike")
            {
                /**
                 * Attack Steps:
                 * 1. Coin flip to choose vertical or horizontal spike line
                 *    Spike line is 3 columns wide
                 * 2. Warn where the spikes will show up
                 * 3. Spikes show up in warning spots
                 * 4. Randomize where the next attack will happen
                 * 5. Reset the tiles affected and boss attacking state
                 */

                // Boss SPIKE ATTACK - VERTICAL
                if (isSpikeVertical)
                {
                    // Boss warning
                    if (bossSpikeTimer % 3 == 0 && bossAttackCounter > 0 && bossWarningRow < map.Height)
                    {
                        // Warn the left row, selected row, and right row
                        if (bossAttackColPos >= 2) BossAttackEmoji(bossAttackColPos - 2, bossWarningRow, warning);
                        BossAttackEmoji(bossAttackColPos, bossWarningRow, warning);
                        if (bossAttackColPos <= (map.Width - 2) * 2 - 2) BossAttackEmoji(bossAttackColPos + 2, bossWarningRow, warning);
                        bossWarningRow++;
                    }

                    // Boss attack
                    if (bossSpikeTimer % 3 == 0 && bossAttackCounter >= 60 && bossAttackRow < map.Height)
                    {
                        // Attack the left row, selected row, and right row
                        if (bossAttackColPos >= 2) BossAttackEmoji(bossAttackColPos - 2, bossAttackRow, spike);
                        BossAttackEmoji(bossAttackColPos, bossAttackRow, spike);
                        if (bossAttackColPos <= (map.Width - 2) * 2 - 2) BossAttackEmoji(bossAttackColPos + 2, bossAttackRow, spike);
                        bossAttackRow++;
                    }

                    // Reset boss attack tiles
                    if (bossAttackCounter >= 120)
                    {
                        for (int y = 0; y < map.Height; y++)
                        {
                            // Reset the left row, selected row, and right row
                            if (bossAttackColPos >= 2) ResetBossAttackTiles(bossAttackColPos - 2, y);
                            ResetBossAttackTiles(bossAttackColPos, y);
                            if (bossAttackColPos <= (map.Width - 2) * 2 - 2) ResetBossAttackTiles(bossAttackColPos + 2, y);
                        }
                        RandomizeBossColumn();
                        ResetBossAttackingState();
                    }

                    // Increase time counter while the boss is using an attack
                    if (isBossAttacking)
                    {
                        bossSpikeTimer++;
                        bossAttackCounter++;
                    }
                }
                // Boss SPIKE ATTACK - HORIZONTAL
                else
                {
                    // Boss warning
                    if (bossSpikeTimer % 3 == 0 && bossAttackCounter > 0 && bossWarningCol < map.Width)
                    {
                        // Warn the left column, selected column, and right column
                        if (bossAttackRowPos >= 1) BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos - 1, warning);
                        BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos, warning);
                        if (bossAttackRowPos <= map.Height - 1) BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos + 1, warning);
                        bossWarningCol++;
                    }

                    // Boss attack
                    if (bossSpikeTimer % 3 == 0 && bossAttackCounter >= 60 && bossAttackCol < map.Width)
                    {
                        // Attack the left column, selected column, and right column
                        if (bossAttackRowPos >= 1) BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos - 1, spike);
                        BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos, spike);
                        if (bossAttackRowPos <= map.Height - 1) BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos + 1, spike);
                        bossAttackCol++;
                    }

                    // Reset boss attack tiles
                    if (bossAttackCounter >= 120)
                    {
                        for (int x = 0; x < map.Width; x++)
                        {
                            // Reset the left column, selected column, and right columny
                            if (bossAttackRowPos >= 1) ResetBossAttackTiles(x * 2, bossAttackRowPos - 1);
                            ResetBossAttackTiles(x * 2, bossAttackRowPos);
                            if (bossAttackRowPos <= map.Height - 1) ResetBossAttackTiles(x * 2, bossAttackRowPos + 1);
                        }
                        RandomizeBossRow();
                        ResetBossAttackingState();
                    }

                    // Increase time counter while the boss is using an attack
                    if (isBossAttacking)
                    {
                        bossSpikeTimer++;
                        bossAttackCounter++;
                    }
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
                 * 4. Randomize next attack position
                 * 5. Reset tiles affected by attack
                 */

                // Boss warning
                if (bossLightningTimer % 2 == 0 && bossAttackCounter > 0 && bossWarningRow < lightningSize)
                {
                    // Warn the far left column, left column, selected column, right column, and far right column
                    if (bossAttackColPos >= 8) BossAttackEmoji(bossAttackColPos - 8, bossWarningRow, warning);
                    if (bossAttackColPos >= 4) BossAttackEmoji(bossAttackColPos - 4, bossWarningRow, warning);
                    BossAttackEmoji(bossAttackColPos, bossWarningRow, warning);
                    if (bossAttackColPos <= (map.Width - 4) * 2 - 2) BossAttackEmoji(bossAttackColPos + 4, bossWarningRow, warning);
                    if (bossAttackColPos <= (map.Width - 8) * 2 - 2) BossAttackEmoji(bossAttackColPos + 8, bossWarningRow, warning);
                    bossWarningRow++;
                }

                // Boss attack
                if (bossLightningTimer % 2 == 0 && bossAttackCounter >= 45 && bossAttackRow < lightningSize)
                {
                    // Attack the far left column, left column, selected column, right column, and far right column
                    if (bossAttackColPos >= 8) BossAttackEmoji(bossAttackColPos - 8, bossAttackRow, lightning);
                    if (bossAttackColPos >= 4) BossAttackEmoji(bossAttackColPos - 4, bossAttackRow, lightning);
                    BossAttackEmoji(bossAttackColPos, bossAttackRow, lightning);
                    if (bossAttackColPos <= (map.Width - 4) * 2 - 2) BossAttackEmoji(bossAttackColPos + 4, bossAttackRow, lightning);
                    if (bossAttackColPos <= (map.Width - 8) * 2 - 2) BossAttackEmoji(bossAttackColPos + 8, bossAttackRow, lightning);
                    bossAttackRow++;
                }

                // Reset boss attack tiles
                if (bossAttackCounter >= 70)
                {
                    for (int y = 0; y < lightningSize; y++)
                    {
                        // Reset the far left column, left column, selected column, right column, and far right column
                        if (bossAttackColPos >= 8) ResetBossAttackTiles(bossAttackColPos - 8, y);
                        if (bossAttackColPos >= 4) ResetBossAttackTiles(bossAttackColPos - 4, y);
                        ResetBossAttackTiles(bossAttackColPos, y);
                        if (bossAttackColPos <= (map.Width - 4) * 2 - 2) ResetBossAttackTiles(bossAttackColPos + 4, y);
                        if (bossAttackColPos <= (map.Width - 8) * 2 - 2) ResetBossAttackTiles(bossAttackColPos + 8, y);
                    }
                    RandomizeBossColumn();
                    ResetBossAttackingState();
                }

                // Increase time counter while the boss is using an attack
                if (isBossAttacking)
                {
                    bossLightningTimer++;
                    bossAttackCounter++;
                }

            } // End of Lightning attack

            // Start of Wave attack
            if (currentAttack == "wave")
            {
                /**
                 * Attack Steps:
                 * 1. Choose vertical or horizontal (?)
                 * 2. Warning on the rows or columns that will be affected
                 * 3. Waves on the warnings
                 * 4. Randomize next attack position
                 * 5. Reset tiles affected by attack
                 */

            } // End of Wave attack

            // ─────────────────────────────────────────────────────────────────────
            // DISPLAY
            // ─────────────────────────────────────────────────────────────────────

            Terminal.SetCursorPosition(0, MAP_HEIGHT + 1);
            Terminal.ResetColor();
            Terminal.ForegroundColor = ConsoleColor.White;
            Terminal.WriteLine($"Time: {Time.DisplayText}   Pos({playerX + 1},{playerY + 1})   ");
            Terminal.ClearLine();
            Terminal.Write($"Column:{bossAttackColPos} Row:{bossAttackRowPos}");
            Terminal.ForegroundColor = ConsoleColor.Black;
            Terminal.SetCursorPosition(0, MAP_HEIGHT + 5);
        }

        // ─────────────────────────────────────────────────────────────────────
        // BOSS ATTACKS / AI METHODS
        // ─────────────────────────────────────────────────────────────────────

        // The Boss uses an attack
        void BossAttackEmoji(int x, int y, ColoredText emoji)
        {
            map.Poke(x, y, emoji);
        }

        // Set the tiles the boss just attacked back to the normal tileset
        void ResetBossAttackTiles(int x, int y)
        {
            map.Poke(x, y, map.Get(x / 2, y));
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
            bossAttackCounter = 0;

            bossWarningRow = 0;
            bossAttackRow = 0;

            bossWarningCol = 0;
            bossAttackCol = 0;

            bossSpikeTimer = 0;
            bossLightningTimer = 0;
            bossWaveTimer = 0;
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

                // EAch cell is two collumns wide
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
    }
}