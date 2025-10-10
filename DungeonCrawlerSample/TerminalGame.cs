using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.InteropServices;
using System.Text;
using DungeonCrawlerSample;
using DungeonCrawlerSample.MohawkTerminalGame.NewClasses;

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
        bool hasShownBanterOne = false;
        bool hasShownBanterTwo = false;
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
        private List<string> dialogueLines; // store wrapped lines
        private int currentLineIndex = 0;   // which line we are typing
        // Jonahs Stuff

        // HUD tracking to prevent flicker
        private int hudRowTimePos = MAP_HEIGHT + 1; // row for Time / Pos (fixed)
        private int hudRowColumnRow = MAP_HEIGHT + 2; // row for Column/Row display
        private string lastTimePosText = string.Empty;
        private string lastColRowText = string.Empty;

        string[] rightCharacterArtIdle = new string[]
{
"                   ",
"      Á            ",
"    _/-\\_         ",
"    {òʘó} *        ",
"   <[ : ]\\|       ",
"     v v  |        "
};
        string[] rightCharacterArtWave = new string[]
{
"                    ",
"             Á      ",
"     ,__   _/-\\_    ",
" __’ ) \\   {òʘó} ¡  ",
"’) \\/   \\ <[ : ]\\|",
"/  /     \\  v v  | ",
};
        string[] rightCharacterArtLightning = new string[]
{
" \\ \\               ",
"  //  Á              ",
"  V _/-\\_           ",
"    {òʘó} ¡          ",
"   <[ : ]\\|         ",
"     v v  |          "
};
        string[] rightCharacterArtSpike = new string[]
{
"        \\\\ v//      ",
"      Á  \\V/       ",
"    _/-\\_ V        ",
"    {òʘó} ¡         ",
"   <[ : ]\\|        ",
"     v v  |         "
};
        string[] rightCharacterArtHurt = new string[]
{
    "     \\ v//",
    "   Á  \\V/",
    " _/-\\_ V",
    " {òʘó} ¡",
    "<[ : ]\\| ",
    "  v v  |  "
};
        private string currentDialogue = "";
        private int dialogueCharIndex = 0;
        private int dialogueRow = 12;
        private int dialogueCol = 38;
        private bool isShowingDialogue = false;
        private ConsoleColor dialogueColor = ConsoleColor.White;
        private int dialogueMaxWidth = 32;
        private bool winScreenDrawn = false;
        private float elapsedTime = 0f; // total time in seconds
        private bool completedPhaseOne = false;
        private bool completedPhaseTwo = false;
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
        ColoredText floorTile = new(@"  ", ConsoleColor.Yellow, ConsoleColor.Black);
        ColoredText wallTile = new(@"██", ConsoleColor.White, ConsoleColor.Black);

        ColoredText floorLight = new(@"  ", ConsoleColor.Yellow, ConsoleColor.Black);
        ColoredText floorDark = new(@"  ", ConsoleColor.DarkGray, ConsoleColor.Black);

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

        int bossSpikeTimer;             // Timer for spike attack
        int bossLightningTimer;         // Timer for lightning attack
        int bossWaveTimer;              // Timer for wave attack
        int bossAttackCounter;          // Counter for attackinf parts of attacks
        int bossAttackInterval;         // How often the boss attacks (frames, not seconds)
        int bossAttackIntervalCounter;  // Counts towards the next boss attack (frames, not seconds)

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

        bool[,] attackArray;                // Memory for where the boss is currently attacking
        int bossPhase;                      // Weighting for the boss attacks based on the phase
        int bossFinalPhaseCounter;          // Counter during the final boss phase to make attacks more frequent
        bool playerHasSword;                // If the player has the sword

        public int enemyHealth = 2;
        public int enemyMaxHealth = 2;
        // ─────────────────────────────────────────────────────────────────────
        // ENGINE STUFF
        // ─────────────────────────────────────────────────────────────────────

        /// Run once before Execute begins
        public void Setup()
        {
            //BanterDialogueOne();
            damageTimerStart = DateTime.Now;
            // Run the game steady by using the timer loop (made by raph)
            Program.TerminalExecuteMode = TerminalExecuteMode.ExecuteTime;
            Program.TerminalInputMode = TerminalInputMode.EnableInputDisableReadLine;
            Program.TargetFPS = 60;

            // Making the console perty
            Terminal.SetTitle("Wizard Tower");
            Terminal.CursorVisible = false;
            Terminal.BackgroundColor = ConsoleColor.Black;
            //UpdateEnemyHearts();
            if (!gameOver)
            {
                UpdateEnemyHearts();
                // Build a new 15×15 grid (each cell makes two columns)
                map = new TerminalGridWithColor(MAP_WIDTH, MAP_HEIGHT, floorLight);

                // Fill the board with a checkered pattern
                // fill with checkered pattern
                for (int y = 0; y < MAP_HEIGHT; y++)
                {
                    for (int x = 0; x < MAP_WIDTH; x++)
                    {
                        var tile = ((x + y) % 2 == 0) ? floorLight : floorDark;
                        map.Poke(x, y, tile);
                    }
                }

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
                // render the map once
                // Put player on top 
                DrawCharacter(playerX, playerY, player);

                // Set up boss attacks
                RandomizeBossColumn();
                RandomizeBossRow();
                isSpikeVertical = true;
                bossPhase = 9;
                playerHasSword = false;
                bossAttackInterval = 160;
                bossAttackIntervalCounter = 0;

                // Set up sword items
                swordParts[0] = gem;
                swordParts[1] = shield;
                swordParts[2] = sword;

                nextSwordSpawn = Random.Integer(10 * Program.TargetFPS, 20 * Program.TargetFPS + 1);

                DrawInventory();

                // Draw static right-side ASCII character (only once)
                int mapWidth = MAP_WIDTH * CELL_W;
                int margin = 15;
                int characterX = mapWidth + margin;
                int characterY = 2;
                DrawAsciiCharacter(characterX, characterY, rightCharacterArtIdle, ConsoleColor.Red);

                if (!isShowingDialogue)
                {
                    if (!completedPhaseOne)
                    {
                        StartDialogue("I’ve sent your little sword to another dimension — now all you can do is die by my hand!", ConsoleColor.Red);
                    }
                }
            }

        }

        public void Execute()
        {
            
            elapsedTime += 1f / Program.TargetFPS;
            //BanterDialogueTwo();

            //if (testDying)
            //{
            //    damageElapsed += 1f / Program.TargetFPS;

            //    if (damageElapsed >= damageInterval && health > 0)
            //    {
            //        ChangeHealth(-1);
            //        damageElapsed = 0f;
            //    }
            //}

            if (gameOver)
            {
                if (bossPhase > 20) DrawWinScreen(elapsedTime);
                DrawGameOverScreen();
                return; // Stop all other updates
            }
            CheckIfDead();
            if (gameOver == false)
            {
                
                UpdateDialogue(); // non-blocking dialogue update

                // Does input and move player one cell at a time using WASD or the arrows
                CheckMovePlayer();

                if (inputChanged)
                {
                    // Make sure to replace what was under the old player like the floor and dots and stuff
                    ResetCell(oldPlayerX, oldPlayerY);
                    inputChanged = false;
                }

                SwordPartsTick();

                 //Basic HUD will be changed for sure
                 //Terminal.SetCursorPosition(0, MAP_HEIGHT + 1);
                 //Terminal.ResetColor();
                 //Terminal.ForegroundColor = ConsoleColor.Black;
                 //Terminal.Write(Time.DisplayText);

                // ─────────────────────────────────────────────────────────────────────
                // BOSS ATTACK CODE LOOPS
                // ATTACKS HAVE INFO INSIDE
                // ─────────────────────────────────────────────────────────────────────

                // Boss choosing what attack to do
                // Check if the boss can attack

                // Dev button for boss attack toggles
                //if (Input.IsKeyDown(ConsoleKey.H))
                if (bossAttackIntervalCounter >= bossAttackInterval)
                {
                    // Only choose a new attack if the boss is not attacking
                    if (!isBossAttacking)
                    {
                        ResetBossAttackingState();
                        // Randomize what attack the boss is going to use
                        int attackToUse = Random.Integer(0, bossPhase);

                        // Spike attack settings
                        if (attackToUse < 13)
                        {
                            
                            currentAttack = "spike";
                            // Randomize where the boss will attack
                            RandomizeBossColumn();
                            RandomizeBossRow();
                            spikePositionCounter = 0;
                            // Determine which direction to shoot the spikes
                            isSpikeVertical = Random.CoinFlip();

                            // Draw static right-side ASCII character using this attack
                            int mapWidth = MAP_WIDTH * CELL_W;
                            int margin = 15;
                            int characterX = mapWidth + margin;
                            int characterY = 2;
                            DrawAsciiCharacter(characterX, characterY, rightCharacterArtSpike, ConsoleColor.Red);

                            if (!isShowingDialogue)
                            {
                                //StartDialogue("I Cast Spike Attack!", ConsoleColor.Red);
                            }
                        }
                        // Lightning attack settings
                        else if (attackToUse < 22)
                        {
                            if (completedPhaseOne == false)
                            {
                                completedPhaseOne = true;
                                
                                
                            }
                            if (completedPhaseOne == true && completedPhaseTwo == false)
                            {
                                StartDialogue("And you forget that my spatial manipulation extends beyond creating dimensions now observe, my power!\r\n", ConsoleColor.Red);
                            }
                            currentAttack = "lightning";
                            // Randomize where the boss will attack
                            RandomizeBossColumn();
                            // Determine how far down to shoot the lightning
                            lightningSize = Random.Integer(7, 13);

                            // Draw static right-side ASCII character using this attack
                            int mapWidth = MAP_WIDTH * CELL_W;
                            int margin = 15;
                            int characterX = mapWidth + margin;
                            int characterY = 2;
                            DrawAsciiCharacter(characterX, characterY, rightCharacterArtLightning, ConsoleColor.Red);

                            //StartDialogue("I Cast Lightning Attack!", ConsoleColor.Red);
                        }
                        // Wave attack settings
                        else if (attackToUse < 30)
                        {
                            if (completedPhaseTwo == false)
                            {
                                completedPhaseTwo = true;
                            }
                            if (completedPhaseOne == true && completedPhaseTwo == true)
                            {
                                StartDialogue("Not so fast you squalid squash you forgot that I haven’t used my most powerful magic yet, my tidal mastery is absolute!\r\n", ConsoleColor.Red);
                            }
                            currentAttack = "wave";
                            // Randomize where the boss will attack
                            RandomizeBossColumn();
                            // Reset the wave row
                            waveRow = MAP_HEIGHT - 1;

                            // Draw static right-side ASCII character using this attack
                            int mapWidth = MAP_WIDTH * CELL_W;
                            int margin = 15;
                            int characterX = mapWidth + margin;
                            int characterY = 2;
                            DrawAsciiCharacter(characterX, characterY, rightCharacterArtWave, ConsoleColor.Red);

                            //StartDialogue("I Cast a Wave!", ConsoleColor.Red);
                        }
                        isBossAttacking = true;
                    }
                }
                // Increase timer to next attack when not in an attack
                else
                {
                    // ... later, when attack finishes:
                    if (!isBossAttacking && isShowingDialogue)
                    {
                        // Clear the old dialogue from the screen
                        // Clear the dialogue
                        /*
                        if (isShowingDialogue)
                        {
                            Console.SetCursorPosition(0, MAP_HEIGHT + 6);
                            Console.Write(new string(' ', Console.WindowWidth));
                            isShowingDialogue = false;
                        }
                        
                        isShowingDialogue = false;
                        */
                    }
                    bossAttackIntervalCounter++;
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
                                BossAttackEmoji(bossAttackColPos - 4, bossWarningRow, warning, false);
                                BossAttackEmoji(bossAttackColPos - 2, bossWarningRow, warning, false);
                                BossAttackEmoji(bossAttackColPos, bossWarningRow, warning, false);
                                BossAttackEmoji(bossAttackColPos + 2, bossWarningRow, warning, false);
                                BossAttackEmoji(bossAttackColPos + 4, bossWarningRow, warning, false);
                                bossWarningRow++;
                            }

                            // Boss attack
                            if (bossSpikeTimer % 3 == 0 && bossAttackCounter >= 60 && bossAttackRow < map.Height)
                            {
                                // Attack the left row, selected row, and right row
                                BossAttackEmoji(bossAttackColPos - 4, bossAttackRow, spike, true);
                                BossAttackEmoji(bossAttackColPos - 2, bossAttackRow, spike, true);
                                BossAttackEmoji(bossAttackColPos, bossAttackRow, spike, true);
                                BossAttackEmoji(bossAttackColPos + 2, bossAttackRow, spike, true);
                                BossAttackEmoji(bossAttackColPos + 4, bossAttackRow, spike, true);
                                bossAttackRow++;
                            }

                            // Reset boss attack tiles
                            if (bossSpikeTimer % 3 == 0 && bossAttackCounter >= 120 && spikePositionCounter < map.Height)
                            {
                                // Reset the left row, selected row, and right row
                                ResetBossAttackTiles(bossAttackColPos - 4, spikePositionCounter);
                                ResetBossAttackTiles(bossAttackColPos - 2, spikePositionCounter);
                                ResetBossAttackTiles(bossAttackColPos, spikePositionCounter);
                                ResetBossAttackTiles(bossAttackColPos + 2, spikePositionCounter);
                                ResetBossAttackTiles(bossAttackColPos + 4, spikePositionCounter);
                                spikePositionCounter++;

                                if (spikePositionCounter >= map.Height)
                                {
                                    bossAttackIntervalCounter = 0;
                                    isBossAttacking = false;
                                }
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
                                BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos - 2, warning, false);
                                BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos - 1, warning, false);
                                BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos, warning, false);
                                BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos + 1, warning, false);
                                BossAttackEmoji(bossWarningCol * 2, bossAttackRowPos + 2, warning, false);
                                bossWarningCol++;
                            }

                            // Boss attack
                            if (bossSpikeTimer % 3 == 0 && bossAttackCounter >= 60 && bossAttackCol < map.Width)
                            {
                                // Attack the left column, selected column, and right column
                                BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos - 2, spike, true);
                                BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos - 1, spike, true);
                                BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos, spike, true);
                                BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos + 1, spike, true);
                                BossAttackEmoji(bossAttackCol * 2, bossAttackRowPos + 2, spike, true);
                                bossAttackCol++;
                            }

                            // Reset boss attack tiles
                            if (bossSpikeTimer % 3 == 0 && bossAttackCounter >= 120 && spikePositionCounter < map.Width * 2)
                            {
                                // Reset the left column, selected column, and right columny
                                ResetBossAttackTiles(spikePositionCounter * 2, bossAttackRowPos - 2);
                                ResetBossAttackTiles(spikePositionCounter * 2, bossAttackRowPos - 1);
                                ResetBossAttackTiles(spikePositionCounter * 2, bossAttackRowPos);
                                ResetBossAttackTiles(spikePositionCounter * 2, bossAttackRowPos + 1);
                                ResetBossAttackTiles(spikePositionCounter * 2, bossAttackRowPos + 2);
                                spikePositionCounter++;

                                if (spikePositionCounter >= map.Width * 2)
                                {
                                    bossAttackIntervalCounter = 0;
                                    isBossAttacking = false;
                                }
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
                            bossAttackIntervalCounter = 0;
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

                            // When the wave has fully retracted
                            if (waveRow < 0)
                            {
                                bossAttackIntervalCounter = 0;
                                isBossAttacking = false;
                            }
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
                if (PlayerInDanger() && !playerHasSword)
                {
                    ChangeHealth(-1);
                    // Get rid of tiles all around player so they don't immediately take more damage
                    //for (int i = -1; i < 2; i++)
                    //{
                    //    for (int j = -1;  j < 2; j++)
                    //    {
                    //        ResetBossAttackTiles((playerX + i) * 2, playerY + j);
                    //    }
                    //}
                    ResetBossAttackTiles(playerX * 2, playerY);
                }
                // Player delfects an attack when they have the sword
                else if (PlayerInDanger() && playerHasSword)
                {
                    ResetBossAttackTiles(playerX * 2, playerY);
                    playerHasSword = false;
                    bossPhase += 10;
                    bossAttackInterval -= 60;

                    // Check if the boss is defeated
                    if (bossPhase > 30) gameOver = true;

                    // Isaac u can uncomment this when the deflect is implemented to clear the announmcnet and also restart the inventory for next phase
                    OnSwordDeflected();
                }

                // Increase the boss attack frequency while they are on final phase
                if (bossPhase >= 15 && bossAttackInterval > 0)
                {
                    bossFinalPhaseCounter++;
                    // Every 5 seconds, decrease time between attacks by 10 frames
                    // 4 cycles to get to max attack rate
                    if (bossFinalPhaseCounter >= 300 && bossAttackInterval > 0)
                    {
                        bossFinalPhaseCounter = 0;
                        bossAttackIntervalCounter -= 10;
                    }
                }

                // Dev key to go to next boss phase
                //if (Input.IsKeyPressed(ConsoleKey.J))
                //{
                //    //gameOver = true;
                //    bossPhase += 10;
                //    bossAttackInterval -= 60;
                //}
                
                Terminal.SetCursorPosition(0, MAP_HEIGHT + 1);
                Terminal.ResetColor();
                Terminal.ForegroundColor = ConsoleColor.White;
                Terminal.WriteLine($"Time: {Time.DisplayText}   Pos({playerX + 1},{playerY + 1})   ");
                /*
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

                //BanterDialogueOne();
                Terminal.ForegroundColor = ConsoleColor.Black;
                // Clear stray input characters
                /*
                
                Console.SetCursorPosition(0, MAP_HEIGHT + 6);
                Console.Write(new string(' ', 800));
                Console.SetCursorPosition(0, MAP_HEIGHT + 6);
                */
            }
            else
            {
                Debug.Write("Game Over");
                //return;
            }

        }
        private void DrawWinScreen(float totalTimeSeconds)
        {
            if (!winScreenDrawn)
            {
                Console.Clear();

                string[] winArt = {
            "                                        _/V\\_",
            "                                      <[)(O)(]>",
            "                                        {XXX}",
            "                                        {XXX}",
            "                                        {XXX}",
            "                                        {XXX}",
            "                                   ___<MMMMMMM>___",
            "                                 /______((I))______\\",
            "        ___        ___  __   ______   _______     _______     _______    ___    ___",
            "        \\--\\      /—-/ |--| |______| |___ ___|   |  ___  |   |-------|   \\--\\  /—-/",
            "         \\--\\    /—-/  |--| |--|       |--|  |   |--| |--|   |-------|    \\--\\/—-/",
            "          \\--\\  /—-/   |--| |--|       |--|  |   |--| |--|   |--|\\--\\      \\----/",
            "           \\--\\/—-/    |--| |--|____   |--|  |   |--| |--|   |--| \\--\\      |--|",
            "            \\____/     |__| |______|   |__|  |   |_______|   |__|  \\__\\     |__|",
            "                                        | |  |",
            "                                        | |  |",
            "                                        | |  |",
            "                                        | |  |",
            "                                        | |  |",
            "                                        | |  |",
            "                                        | |  |",
            "                                        | |  |",
            "                                        | |  |",
            "                                        | |  |",
            "                                        | |  |",
            "                                        \\ \\ /",
            "                                         \\/"
        };

                // Instructions
                string[] instructions = {
            "Press R to Restart",
            "Press Q to Quit"
        };

                // Compute score
                int maxScore = 1000;
                int score = Math.Max(0, maxScore - (int)(totalTimeSeconds * 5));
                string timeText = $"Time: {totalTimeSeconds:F2}s";
                string scoreText = $"Speed Score: {score}";

                // Console dimensions
                int winW = Console.WindowWidth;
                int winH = Console.WindowHeight;
                int artW = winArt.Max(l => l.Length);
                int artH = winArt.Length;

                // Center art
                int startX = Math.Max(0, (winW - artW) / 2);
                int startY = Math.Max(0, (winH - artH - 2) / 2); // leave space for stats

                Console.ForegroundColor = ConsoleColor.Yellow;
                for (int i = 0; i < winArt.Length; i++)
                {
                    int y = startY + i;
                    if (y < winH)
                    {
                        Console.SetCursorPosition(startX, y);
                        Console.Write(winArt[i]);
                    }
                }

                // Draw time and score just above instructions
                Console.ForegroundColor = ConsoleColor.Cyan;
                int statsY = winH - instructions.Length - 3; // 3 lines above bottom for spacing
                Console.SetCursorPosition(2, statsY);
                Console.Write(timeText);
                Console.SetCursorPosition(2, statsY + 1);
                Console.Write(scoreText);

                // Draw instructions stacked in bottom right corner
                Console.ForegroundColor = ConsoleColor.Green;
                for (int i = 0; i < instructions.Length; i++)
                {
                    int y = winH - instructions.Length + i - 1; // bottom-right alignment
                    Console.SetCursorPosition(winW - instructions[i].Length - 2, y);
                    Console.Write(instructions[i]);
                }

                Console.ResetColor();
                winScreenDrawn = true;

                // Input handling
                bool waiting = true;
                while (waiting)
                {
                    if (Input.IsKeyPressed(ConsoleKey.R))
                    {
                        RestartGame();
                        winScreenDrawn = false;
                        waiting = false;
                    }
                    else if (Input.IsKeyPressed(ConsoleKey.Q))
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }

        public void ShowIntroWithDialogue()
        {
            Terminal.Clear();
            Terminal.SetCursorPosition(24, 5);

            string[] introArt = new string[]
            {
        "_________________________________ ",
        "| _______________________________ |\\",
        "| \\______   \\_   _____/\\____    / | \\",
        "|  |       _/|    __)_   /     /  | ||",
        "|  |    |   \\|        \\ /     /_  | ||",
        "|  |____|_  /_______  //_______ \\ | ||",
        "|         \\/        \\/         \\/ | ||",
        "|_________________________________| ||",
        " \\_________________________________\\||",
        "  \\_________________________________\\|"
            };

            // Draw the intro art
            for (int i = 0; i < introArt.Length; i++)
            {
                Terminal.SetCursorPosition(36, i);
                Terminal.WriteLine(introArt[i]);
            }

            // Dialogue lines
            string[] dialogue = new string[]
            {
        "Rain pours as you stand before the dark wizard Akunin’s tower. \"The elements seem to sense the gravity of this,\" you think, reflecting on how you arrived here. You recall kneeling before King Koning, who, with worry in his eyes, tasked you, a holy knight, with slaying the vile wizard. It was a mission you knew would come. Now, standing at the door, you take a deep breath, then kick it off its hinges, ready to face the fate ahead.",
        "Prepare yourself for the Wizard Tower challenge!",
        "The boss waits for no one... are you ready?"
            };

            int dialogueStartRow = introArt.Length + 2;
            int dialogueX = 25;
            int maxLineWidth = 70; // max chars per line for terminal

            double totalTime = 14; // seconds
            double timePerLine = totalTime / dialogue.Length;

            foreach (string paragraph in dialogue)
            {
                // Add an empty line before each paragraph for spacing
                dialogueStartRow++;

                // Word-wrap the paragraph
                var words = paragraph.Split(' ');
                StringBuilder lineBuilder = new();
                foreach (var word in words)
                {
                    if (lineBuilder.Length + word.Length + 1 > maxLineWidth)
                    {
                        // Print current line
                        Terminal.SetCursorPosition(dialogueX, dialogueStartRow++);
                        Terminal.ForegroundColor = ConsoleColor.White;
                        foreach (char c in lineBuilder.ToString())
                        {
                            Terminal.Write(c);
                            System.Threading.Thread.Sleep(30); // typewriter
                        }
                        lineBuilder.Clear();
                    }
                    if (lineBuilder.Length > 0) lineBuilder.Append(' ');
                    lineBuilder.Append(word);
                }

                // Print remaining line
                if (lineBuilder.Length > 0)
                {
                    Terminal.SetCursorPosition(dialogueX, dialogueStartRow++);
                    Terminal.ForegroundColor = ConsoleColor.White;
                    foreach (char c in lineBuilder.ToString())
                    {
                        Terminal.Write(c);
                        System.Threading.Thread.Sleep(30);
                    }
                }

                // Wait before next paragraph
                System.Threading.Thread.Sleep((int)(timePerLine * 1000));
            }

            // Wait for player input
            dialogueStartRow++;
            Terminal.SetCursorPosition(dialogueX, dialogueStartRow);
            Terminal.Write("Press any key to start...");
            Console.ReadKey(true);
        }

        private void StartDialogue(string text, ConsoleColor color)
        {
            currentDialogue = text;
            dialogueCharIndex = 0;
            dialogueRow = 12;  // starting row
            dialogueCol = 38;  // starting column
            dialogueColor = color;

            // Wrap the text into word-safe lines
            dialogueLines = WrapText(currentDialogue, dialogueMaxWidth);
            currentLineIndex = 0;
            isShowingDialogue = true;
        }

        private void UpdateDialogue()
        {
            if (!isShowingDialogue || gameOver) return; // Don't show dialogue if game is over
            if (!isShowingDialogue) return;

            Terminal.ForegroundColor = dialogueColor;

            if (currentLineIndex >= dialogueLines.Count)
            {
                // Finished all lines
                isShowingDialogue = false;
                return;
            }

            string currentLine = dialogueLines[currentLineIndex];

            // Print a few characters per frame
            int charsPerFrame = 2;
            for (int i = 0; i < charsPerFrame && dialogueCharIndex < currentLine.Length; i++, dialogueCharIndex++)
            {
                Terminal.SetCursorPosition(dialogueCol, dialogueRow);
                Terminal.Write(currentLine[dialogueCharIndex]);
                dialogueCol++;
            }

            // If finished current line, move to next
            if (dialogueCharIndex >= currentLine.Length)
            {
                dialogueCharIndex = 0;
                dialogueCol = 38; // reset X
                dialogueRow++;
                currentLineIndex++;
            }
        }


        private static List<string> WrapText(string text, int maxWidth)
        {
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var lines = new List<string>();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                // If adding the next word exceeds maxWidth, wrap to a new line
                if (currentLine.Length + word.Length + (currentLine.Length > 0 ? 1 : 0) > maxWidth)
                {
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                }

                if (currentLine.Length > 0)
                    currentLine.Append(' '); // add space between words

                currentLine.Append(word);
            }

            // Add any remaining text
            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            return lines;
        }

        /*
        private void ShowBossDialogueRightSide(string text,
                                       int maxWidth = 60, // characters per line
                                       int charDelayMs = 8,
                                       ConsoleColor color = ConsoleColor.White,
                                       int linePauseMs = 250,
                                       int startRow = 12,
                                       int startCol = 38)
        {
            int cursorY = startRow;
            int cursorX = startCol;

            var lines = WrapText(text, maxWidth); // wrap text properly by words

            var originalColor = Terminal.ForegroundColor;
            Terminal.ForegroundColor = color;

            foreach (var line in lines)
            {
                cursorX = startCol; // reset X at start of each line
                Terminal.SetCursorPosition(cursorX, cursorY);

                foreach (var c in line)
                {
                    Terminal.Write(c);
                    if (charDelayMs > 0) System.Threading.Thread.Sleep(charDelayMs);
                    cursorX++;
                }

                cursorY++; // move to next line
                if (linePauseMs > 0) System.Threading.Thread.Sleep(linePauseMs);
            }

            Terminal.ForegroundColor = originalColor;
        }
        
        // now implement the three banter methods using the helper
        private void BanterDialogueOne()
        {
            ShowBossDialogueRightSide(
                "I’ve sent your little sword to another dimension — now all you can do is die by my hand!",
                maxWidth: 60,
                charDelayMs: 6,
                color: ConsoleColor.Red,
                linePauseMs: 200
            );
        }

        private void BanterDialogueTwo()
        {
            ShowBossDialogueRightSide(
                "You fool; my magic shall stop your attempts on my life before you can even make them!",
                maxWidth: 60,
                charDelayMs: 6,
                color: ConsoleColor.Yellow,
                linePauseMs: 200
            );
        }

        private void BanterDialogueThree()
        {
            ShowBossDialogueRightSide(
                "Not so fast, you squalid squash! You forgot that I haven’t used my most powerful magic yet — my tidal mastery is absolute!",
                maxWidth: 60,
                charDelayMs: 6,
                color: ConsoleColor.Cyan,
                linePauseMs: 200
            );
        }
        */
        private void DrawGameOverScreen()
        {
            if (!gameOverScreenDrawn)
            {
                Console.Clear();

                string gameOverText = "GAME OVER";
                string[] instructions = {
            "█  Press R to Retry █",
            "█  Press Q to Quit  █"
        };

                string[] skull = {
            "        ____________ ____________ _____     _____         __________",
            "       /           //   ___     //     |   /     |\\      /         /\\",
            "      /   ________//   /  /    //  /|  |  /  /|  | \\    /   ______/  \\",
            "     /   /\\      //   /__/    //  / |  | /  / |  |  \\  /   /_\\___ \\   \\",
            "    /   /  \\___ //   ____    //  /  |  |/  /  |  |   \\/         /\\ \\   \\",
            "   /   /  /_   //   /   /   //  /   |_____/   |  |   /    _____/  \\ \\   \\",
            "  /   /____/  //   /   /   //  /     \\    \\   |  |  /    /_\\__ \\   \\ \\   \\",
            " /           //   /   /   //  /       \\    \\  |  | /           /\\   \\ \\   \\",
            "/___________//___/___/___//__/      ___\\ ___\\_|__|/___________/\\ \\   \\ \\  /",
            "\\           /           /|    |\\   /   //         //  _____  \\  \\ \\   \\ \\/",
            " \\         /   ____    / |    | \\ /   //   ______//  /    /  /\\  \\ \\   \\",
            "  \\       /   /\\  /   /  |    |  /   //   /_____ /  /____/  /  \\  \\ \\  /",
            "   \\     /   /  \\/   /   |    | /   //         //   __     /    \\  \\ \\/",
            "    \\   /   /   /   /    |    |/   //   ______//   /  |   |\\     \\  \\",
            "     \\ /   /___/   /     |        //   /_____ /   /   |   | \\     \\ /",
            "      /           /      |       //         //   /    |   |  \\     \\",
            "     /___________/       |______//_________//___/     |___|   \\     \\",
            "     \\           \\        \\     \\ \\        \\ \\  \\      \\   \\   \\    /",
            "      \\           \\        \\     \\ \\        \\ \\  \\      \\   \\   \\  /",
            "       \\           \\      / \\     \\ \\        \\ \\  \\      \\   \\   \\/",
            "        \\           \\    /   \\     \\ \\        \\ \\  \\    / \\   \\  |",
            "         \\           \\  /     \\     \\ \\        \\ \\  \\  /   \\   \\ |",
            "          \\___________\\/       \\_____\\/\\________\\/\\__\\/     \\___\\|"
        };

                int winW = Console.WindowWidth;
                int winH = Console.WindowHeight;

                int skullW = skull.Max(l => l.Length);
                int instrW = instructions.Max(l => l.Length);
                int textW = gameOverText.Length;

                // Horizontal positions
                int padding = 4;
                int skullX = winW - skullW - 2;
                int textX = padding;
                int instrX = padding;

                // Vertical centering
                int skullH = skull.Length;
                int instrH = instructions.Length;
                int totalH = Math.Max(skullH, instrH + 1); // +1 for game over text
                int startY = Math.Max(0, (winH - totalH) / 2);

                Console.ForegroundColor = ConsoleColor.Red;

                // Draw GAME OVER text above instructions
                int yText = startY;
                Console.SetCursorPosition(textX, yText);
                Console.Write(gameOverText);

                // Draw instructions below GAME OVER
                for (int i = 0; i < instructions.Length; i++)
                {
                    int y = startY + 1 + i;
                    if (y < winH)
                    {
                        Console.SetCursorPosition(instrX, y);
                        Console.Write(instructions[i]);
                    }
                }

                // Draw skull
                for (int i = 0; i < skull.Length; i++)
                {
                    int y = startY + i;
                    if (y < winH)
                    {
                        Console.SetCursorPosition(skullX, y);
                        Console.Write(skull[i]);
                    }
                }

                Console.ResetColor();
                gameOverScreenDrawn = true;
            }

            // Input handling
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
            UpdateEnemyHearts();
            //string colRowText = $"Is player hit:";

            // Only update if changed (or forced)
            //if (force || timePosFull != lastTimePosText)
            //{
            //    lastTimePosText = timePosFull;
            //    // write padded text so leftover chars are cleared
            //    string padded = timePosFull.PadRight(60, ' ');

            //    Terminal.SetCursorPosition(0, hudRowTimePos);
            //    Terminal.ResetColor();
            //    Terminal.ForegroundColor = ConsoleColor.White;
            //    Terminal.Write(padded);
            //}

            //if (force || colRowText != lastColRowText)
            //{
            //    lastColRowText = colRowText;
            //    string padded2 = colRowText.PadRight(60, ' ');

            //    Terminal.SetCursorPosition(0, hudRowColumnRow);
            //    Terminal.ResetColor();
            //    Terminal.ForegroundColor = ConsoleColor.White;
            //    Terminal.Write(padded2);
            //}
        }
        // ─────────────────────────────────────────────────────────────────────
        // BOSS ATTACKS / AI METHODS
        // ─────────────────────────────────────────────────────────────────────

        // The Boss uses an attack
        void BossAttackEmoji(int x, int y, ColoredText emoji, bool isAttack)
        {
            // Check if the position is in bounds, quit if not
            if (x < 0 || x > (map.Width - 1) * 2 || y < 0 || y >= map.Height) return;

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

            // Draw static right-side ASCII character using this attack
            int mapWidth = MAP_WIDTH * CELL_W;
            int margin = 15;
            int characterX = mapWidth + margin;
            int characterY = 2;
            DrawAsciiCharacter(characterX, characterY, rightCharacterArtIdle, ConsoleColor.Red);
        }

        // Randomize boss attack column (X value)
        void RandomizeBossColumn()
        {
            int previousColumn = bossAttackColPos;

            // So the boss doesn't attack the same position twice in a row
            while (previousColumn == bossAttackColPos)
            {
                previousColumn = bossAttackColPos;
                // Multiply by 2 because emojis are 2-wide characters
                if (currentAttack != "wave")
                {
                    bossAttackColPos = Random.Integer(playerX - 1, playerX + 1) * 2;
                }
                else
                {
                    bossAttackColPos = Random.Integer(0, MAP_WIDTH) * 2;
                }
            }
        }

        // Randomize boss attack row (Y value)
        void RandomizeBossRow()
        {
            int previousRow = bossAttackRowPos;

            // So the boss doesn't attack the same position twice in a row
            while (previousRow == bossAttackRowPos)
            {
                previousRow = bossAttackRowPos;
                bossAttackRowPos = Random.Integer(playerY - 1, playerY + 1);
            }
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

            playerX = Math.Clamp(playerX, 1, MAP_WIDTH - 2);
            playerY = Math.Clamp(playerY, 1, MAP_HEIGHT - 2);

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

        int collectedCounter = 0; // How many collected
        int hudRowInventory = MAP_HEIGHT + 4; // Where to draw the inventory hud
        int hudRowAnnouncment = MAP_HEIGHT + 5;

        ColoredText[] swordParts = new ColoredText[3]; // Array for emojis (sword parts)
        int swordIndex = 0; // To know what part of the sword you are on

        bool swordAnnouncmentShown = false;
        string swordAnnouncmentText = "SWORD COLLECTED - DEFLECT THE WIZARD'S ATTACK! (Stand in the line of fire to parry)"; // Can be changed by our narrative writers (ik its corny sorry - ciaran)

        void SwordPartsTick()
        {
            // Pause all the items spawning until deflect resets it
            if (playerHasSword)
            {
                // Keep the banner visible but do not spawn anything
                return;
            }
            // If a part is on the map then make it and allow it to be picked up
            if (swordX != -1)
            {
                // Keep the item on the map visible
                map.Poke(swordX * CELL_W, swordY, currentSword);

                // Pickup when the player steps on it
                if (playerX == swordX && playerY == swordY)
                {
                    ResetCell(swordX, swordY);  // Change to original form
                    swordX = -1;
                    swordY = -1;

                    // Update as collected and update the UI
                    collectedCounter++;
                    DrawInventory();

                    if (collectedCounter >= swordParts.Length)
                    {
                        // All 3 collected means player has the sword
                        playerHasSword = true;

                        // Sgow announcment once
                        if (!swordAnnouncmentShown)
                        {
                            ShowSwordAnnouncment();
                            swordAnnouncmentShown = true;
                        }

                    }
                    else
                    {
                        // Move to next item in the order 
                        swordIndex++;
                        if (swordIndex >= swordParts.Length) swordIndex = 0;
                    }
                }

                return; // Do not advance the timer while an item is active
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

        void DrawInventory()
        {
            // Clear the line
            Terminal.SetCursorPosition(0, hudRowInventory);
            Console.ResetColor();
            Console.Write(new string(' ', 100));
            Terminal.SetCursorPosition(0, hudRowInventory);

            // Collected text
            Terminal.Write("Collected: ");

            // Show filled slots first, then empties as a box
            for (int i = 0; i < swordParts.Length; i++)
            {
                if (i < collectedCounter)
                    Terminal.Write(swordParts[i].text + " ");
                else
                    Terminal.Write("⬜ ");
            }
        }

        void ShowSwordAnnouncment()
        {
            // Center the announcment below the HUD
            int screenCols = MAP_WIDTH * CELL_W; // Console columns used by the map
            int x = Math.Max(0, (screenCols - swordAnnouncmentText.Length) / 2);
            int y = hudRowAnnouncment;

            Terminal.SetCursorPosition(0, y);
            Console.Write(new string(' ', 120)); // Clear line

            Terminal.SetCursorPosition(x, y);
            Terminal.Write(swordAnnouncmentText, ConsoleColor.Yellow); // You guys can change the color this is also just temporary
        }

        // Call this when the sword gets used/deflected to resume the randomized spawns of the sword parts
        void OnSwordDeflected()
        {
            enemyHealth= enemyHealth-1;
            // Clear the accouncement line
            Terminal.SetCursorPosition(0, hudRowAnnouncment);
            Console.Write(new string(' ', 120));
            swordAnnouncmentShown = false;
            //enemyHealth=enemyHealth-1;
            // Reset inventory to empty for the next phaze
            collectedCounter = 0;
            swordIndex = 0;
            DrawInventory();

            // Resume item spawn timer
            swordTimer = 0;
            nextSwordSpawn = Random.Integer(10 * Program.TargetFPS, 20 * Program.TargetFPS + 1);
        }

        void CheckIfDead()
        {
            if (health <= 0)
            {
                gameOver = true;
            }
        }
        void UpdateEnemyHearts()
        {
            // Save old color
            var oldColor = Console.ForegroundColor;

            // Choose a good display line below the map
            int uiY = MAP_HEIGHT - 8; // +2 lines below the map
            int uiX = 46;              // left side

            // Move cursor & clear the UI area
            Console.SetCursorPosition(uiX, uiY);
            //Console.Write(new string(' ', 80));
            Console.SetCursorPosition(uiX, uiY);

            // Build hearts string
            string enemyHearts = new string('♥', Math.Max(0, enemyHealth));
            string enemyEmpty = new string('♡', Math.Max(0, enemyMaxHealth - enemyHealth));

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"Enemy Health: {enemyHearts}{enemyEmpty}");
            //Console.Write("Enemy Health: " + enemyHealth);
            // Restore color
            Console.ForegroundColor = oldColor;
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