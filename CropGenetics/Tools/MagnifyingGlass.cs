using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netcode;
using Microsoft.Xna.Framework;
using StardewValley;
using _SyrupFramework;
using Microsoft.Xna.Framework.Graphics;

namespace Perennials
{
    public class MagnifyingGlass : StardewValley.Tool, IModdedItem
    {
        public static int lens;
        public static List<string> lensLabels;

        public static Texture2D lensOverlay;

        public static Color dry = Color.SandyBrown;
        public static Color flooded = Color.DeepSkyBlue;
        public static Color weeds = Color.ForestGreen;
        public static Color nitrogen = Color.Cyan;
        public static Color phosphorous = Color.Yellow;
        public static Color potassium = Color.Magenta;
        public static Color neighbors = Color.Coral;

        public MagnifyingGlass () : base("Magnifying Glass", 0, 5, 5, false)
        {
            InstantUse = true;
        }

        public static void loadLensLabels()
        {
            lensLabels = new List<string>();
            lensLabels.Add("Hydration");
            lensLabels.Add("Weeds");
            lensLabels.Add("Nutrients");
            lensLabels.Add("Neighbors");
        }

        public override Item getOne()
        {
            return new MagnifyingGlass();
        }

        protected override string loadDescription()
        {
            return "Used to examine crops.";
        }

        protected override string loadDisplayName()
        {
            return "Magnifying Glass";
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            base.DoFunction(location, x, y, power, who);
            lens ++;
            lens %= 4;
            Game1.showGlobalMessage("Lens set to " + lensLabels[lens]);
            Game1.playSound("dwoop");
            who.CanMove = true;
            who.UsingTool = false;
        }

        public void Load(Dictionary<string, string> data)
        {
            return;
        }

        public Dictionary<string, string> Save()
        {
            return new Dictionary<string, string>();
        }
    }
}
