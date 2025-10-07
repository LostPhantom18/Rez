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
        public ColoredText warning = new(@"⚠️", ConsoleColor.Yellow, ConsoleColor.Black);
        public ColoredText spike = new(@"💥", ConsoleColor.Red, ConsoleColor.Black);
        public ColoredText lightning = new(@"⚡", ConsoleColor.Yellow, ConsoleColor.Black);
        public ColoredText wave = new(@"🌊", ConsoleColor.Blue, ConsoleColor.Black);

        public BossAI()
        {

        }
        public void TestAttack()
        {
            
        }
    }
}
