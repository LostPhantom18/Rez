namespace MohawkTerminalGame;

public class TerminalGame
{
    // Place your variables here


    /// Run once before Execute begins
    public void Setup()
    {
        Program.TerminalExecuteMode = TerminalExecuteMode.ExecuteOnce;
        Program.TerminalInputMode = TerminalInputMode.KeyboardReadAndReadLine;

        Terminal.SetTitle("Title");
    }

    // Execute() runs based on Program.TerminalExecuteMode (assign to it in Setup).
    //  ExecuteOnce: runs only once. Once Execute() is done, program closes.
    //  ExecuteLoop: runs in infinite loop. Next iteration starts at the top of Execute().
    //  ExecuteTime: runs at timed intervals (eg. "FPS"). Code tries to run at Program.TargetFPS.
    //               Code must finish within the alloted time frame for this to work well.
    public void Execute()
    {

    }

}
