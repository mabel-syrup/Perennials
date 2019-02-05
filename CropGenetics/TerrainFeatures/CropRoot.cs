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
    public class CropRoot : SoilCrop
    {
        public CropRoot() { }

        public CropRoot(string cropName, int heightOffset = 0) : base(cropName, heightOffset)
        {
            Logger.Log("CropRoot was created!");
            shakable = true;
        }

        public override bool grow(bool hydrated, bool flooded, int xTile, int yTile, GameLocation environment, string spoofSeason = null)
        {
            if(mature && !isGrowingSeason(spoofSeason, environment))
            {
                //Custom code for roots improving in quality being left in the ground?
                dormant = true;
                regrowMaturity = 0;
                return false;
            }
            if(isGrowingSeason(spoofSeason, environment) && dormant)
            {
                //The crop is reborn if it made it to the next growing season.  It is no longer harvestable.
                hasFruit = false;
                mature = false;
                dormant = false;
                seedMaturity = 0;
                years++;
                return true;
            }
            return base.grow(hydrated, flooded, xTile, yTile, environment, spoofSeason);
        }

        public override void updateSpriteIndex(string spoofSeason = null)
        {
            if(!isGrowingSeason(spoofSeason) && mature)
            {
                currentSprite = 8;
                return;
            }
            base.updateSpriteIndex(spoofSeason);
        }
    }
}
