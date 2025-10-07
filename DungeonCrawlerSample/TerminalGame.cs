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
        readonly ConsoleColor MACRO_DOT_COLOR = ConsoleColor.Gray; // Big tile lines
        readonly ConsoleColor MICRO_DOT_COLOR = ConsoleColor.DarkGray; // Inner 3×3 lines
        readonly ConsoleColor FLOOR_FG = ConsoleColor.Black;
        readonly ConsoleColor FLOOR_BG = ConsoleColor.Black; // Dark background

        // ─────────────────────────────────────────────────────────────────────
        // GAME RUNTIME STATE
        // ─────────────────────────────────────────────────────────────────────

        TerminalGridWithColor map = null!; // The 2D array we are drawing into, then rendering

        // Floor tile which is two columns wide to make the board look nice and even
        ColoredText floorTile = null!;

        // Player
        ColoredText player = new("💀", ConsoleColor.White, ConsoleColor.Black);

        // Input recording so we only need to redraw when neccessary
        bool inputChanged;
        int oldPlayerX, oldPlayerY;
        int playerX = MAP_WIDTH / 2; // Start in center (no real center cause its 15x15)
        int playerY = MAP_HEIGHT / 2;

        // ─────────────────────────────────────────────────────────────────────
        // ENGINE STUFF
        // ─────────────────────────────────────────────────────────────────────

        public void Setup()
        {
            // Run the game steady by using the timer loop (made by raph)
            Program.TerminalExecuteMode = TerminalExecuteMode.ExecuteTime;
            Program.TerminalInputMode = TerminalInputMode.EnableInputDisableReadLine;
            Program.TargetFPS = 60;

            // Making the console perty
            Terminal.SetTitle("Wizard Tower");
            Terminal.CursorVisible = false;
            Terminal.BackgroundColor = FLOOR_BG;

            // Build a new 15×15 grid (each cell makes two columns)
            floorTile = new ColoredText("  ", FLOOR_FG, FLOOR_BG);
            map = new TerminalGridWithColor(MAP_WIDTH, MAP_HEIGHT, floorTile);

            // Make floor
            BuildFloor();

            // Drawing the grid lines onto screen
            DrawGridLines();

            // Rendering
            map.ClearWrite();

            // Put player on top 
            DrawCharacter(playerX, playerY, player);
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
            Terminal.SetCursorPosition(0, MAP_HEIGHT + 1);
            Terminal.ResetColor();
            Terminal.Write($"Time: {Time.DisplayText}   Pos({playerX + 1},{playerY + 1})   ");
        }

        // ─────────────────────────────────────────────────────────────────────
        // BUILDING THE STATIC BACKGROUND
        // ─────────────────────────────────────────────────────────────────────

        // Just makes the floor
        void BuildFloor()
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
                for (int x = 0; x < MAP_WIDTH; x++)
                    map.SetRectangle(floorTile, x, y, 1, 1);
        }

        void DrawGridLines()
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                for (int x = 0; x < MAP_WIDTH; x++)
                {
                    // Outer lines between small tiles
                    bool isOuterLine = (x % INNER_GRID == 0 && x != 0 && x != MAP_WIDTH) || (y % INNER_GRID == 0 && y != 0 && y != MAP_HEIGHT);

                    // Inner lines inside the outer lines 3x3
                    int xInOuter = x % INNER_GRID;   
                    int yInOuter = y % INNER_GRID;
                    bool isInnerLine = (xInOuter != 0) || (yInOuter != 0);

                    // Edge highlight (what you asked for): brighten TOP and LEFT edges
                    bool isEdgeHighlight = (x == 0) || (y == 0);

                    // Decide if we draw here
                    bool drawDot = isEdgeHighlight || isOuterLine || isInnerLine;

                    if (!drawDot)
                    { 
                        continue; // If draw dot not applicable then skip the rest of the code
                    }

                    var under = map.Get(x, y);

                    // If its highlighted or is the outerline 
                    if (isEdgeHighlight || isOuterLine)
                    {
                        map.SetRectangle(new ColoredText("• ", MACRO_DOT_COLOR, under.bgColor), x, y, 1, 1);
                    }
                    // Must be inner line then
                    else
                    {
                        map.SetRectangle(new ColoredText("· ", MICRO_DOT_COLOR, under.bgColor), x, y, 1, 1);
                    }     
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // INPUT / MOVEMENT
        // ─────────────────────────────────────────────────────────────────────

        // Player movement stuff made by Isaac
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