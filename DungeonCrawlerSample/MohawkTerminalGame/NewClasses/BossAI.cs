using MohawkTerminalGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawlerSample
{
    public class BossAI
    {
        // Emojis for boss attacks
        public ColoredText warning = new(@"⚠️", ConsoleColor.Yellow, ConsoleColor.DarkGreen);
        public ColoredText spike = new(@"💥", ConsoleColor.Red, ConsoleColor.DarkGreen);
        public ColoredText lightning = new(@"⚡", ConsoleColor.Yellow, ConsoleColor.DarkGreen);
        public ColoredText wave = new(@"🌊", ConsoleColor.Blue, ConsoleColor.DarkGreen);

        public BossAI()
        {

        }
        public void TestAttack()
        {
            
        }
    }
}
