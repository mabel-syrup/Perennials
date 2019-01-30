using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using StardewValley;
using StardewValley.Tools;
using StardewValley.Locations;
using _SyrupFramework;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using Netcode;

namespace CropGenetics
{
    public class UtilityWand : Tool, IModdedItem
    {
        private int day = 0;

        public UtilityWand() : base("UtilityWand", 0, 7, 7, false, 0)
        {
            this.upgradeLevel.Value = 0;
        }

        protected override string loadDisplayName()
        {
            return "Utility Wand";
        }

        protected override string loadDescription()
        {
            return "Does what I need it to.";
        }

        public override int attachmentSlots()
        {
            return 1;
        }

        public override bool canBeTrashed()
        {
            return true;
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            if (who.currentLocation is FarmHouse)
                who.warpFarmer(new Warp(x, y, "SeedShop", 5, 20, false));
            else if (who.currentLocation == Game1.getLocationFromName("SeedShop"))
                who.warpFarmer(new Warp(x, y, "Farm", 64, 17, false));
            else if (who.currentLocation is Farm)
            {
                day %= 112;
                day++;
                PerennialsGlobal.equalizeDitches(location);
                string season;
                int seasonInt = (int)Math.Floor((double)(day / 28));
                switch (seasonInt)
                {
                    case 0:
                        season = "spring";
                        break;
                    case 1:
                        season = "summer";
                        break;
                    case 2:
                        season = "fall";
                        break;
                    case 3:
                        season = "winter";
                        break;
                    default:
                        season = Game1.currentSeason;
                        break;
                }
                Logger.Log("Simulating a day of " + season + " growth.");
                PerennialsGlobal.simulateFarmDay(season);
            }
        }

        public override Item getOne()
        {
            return new UtilityWand();
        }

        public Dictionary<string, string> Save()
        {
            return new Dictionary<string, string>();
        }

        public void Load(Dictionary<string, string> data)
        {
            return;
        }
    }
}