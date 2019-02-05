using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using _SyrupFramework;
using Microsoft.Xna.Framework;

namespace Perennials
{
    public class CropTrellis : SoilCrop
    {
        public CropTrellis() { }

        public CropTrellis(string cropName, int heightOffset = 0) : base(cropName, heightOffset)
        {
            Logger.Log("CropTrellis was created!");
            shakable = false;
            impassable = true;
        }

        private int getSeasonOffset(string spoofSeason = null)
        {
            string season = Game1.currentSeason;
            if (spoofSeason != null)
                season = spoofSeason;
            switch (season)
            {
                case "spring":
                    return 0;
                case "summer":
                    return 1;
                case "fall":
                    return 2;
                default:
                    return 3;
            }
        }

        public override void updateSpriteIndex(string spoofSeason = null)
        {
            if (!isGrowingSeason(spoofSeason))
            {
                currentSprite = 8 + getSeasonOffset();
                return;
            }
            base.updateSpriteIndex(spoofSeason);
        }

        public override Rectangle getSprite(int number = 0)
        {
            if (currentSprite >= 8)
                return new Rectangle((columnInSpriteSheet * 128) + ((currentSprite + number % 2) * 16), rowInSpriteSheet * 32, 16, 32);
            else
                return base.getSprite(number);
        }
    }
}
