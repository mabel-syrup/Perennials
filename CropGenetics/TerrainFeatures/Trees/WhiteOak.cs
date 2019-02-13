using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using _SyrupFramework;

namespace Perennials
{
    public class WhiteOak : Tree
    {

        public WhiteOak() : base()
        {
            treeID = "whiteoak";
            treeName = "White Oak";
            trunkSheet = trunkSheets[treeID];
            trunkBase = new Rectangle(0, 0, 24, 16);
            trunk = new Rectangle(24, 0, 24, 16);
            maxHealth = 10;
            health = 10f;
            treeStructure = newStump();
            //int height = Game1.random.Next(15);
            int height = 15;
            Logger.Log("Selected height " + height + " for new " + treeName);
            for (int i = 0; i < height; i++)
            {
                Logger.Log("Adding trunk to tree of height " + (i + 1) + ", progressing to max height of " + height + "...");
                extendTrunk();
            }
        }

        public WhiteOak(int days, int years) : this()
        {
            this.ageDays = days;
            this.ageYears = years;
        }

        public WhiteOak(int rawAge) : this()
        {
            this.ageDays = rawAge % 122;
            this.ageYears = (int)(rawAge / 122);
        }
    }
}
