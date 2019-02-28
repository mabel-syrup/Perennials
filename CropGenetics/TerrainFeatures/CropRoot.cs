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

        public override void loadFromXNBData(Dictionary<string, string> cropData)
        {
            Logger.Log("Parsing as a root crop...");
            string[] growStages = cropData["growthTimes"].Split(' ');
            foreach (string stage in growStages)
            {
                growthStages.Add(Convert.ToInt32(stage));
            }
            parseSecondaryGrowth(cropData["regrowthTimes"]);
            if (Convert.ToBoolean(cropData["spring"]))
                seasonsToGrowIn.Add("spring");
            if (Convert.ToBoolean(cropData["summer"]))
                seasonsToGrowIn.Add("summer");
            if (Convert.ToBoolean(cropData["fall"]))
                seasonsToGrowIn.Add("fall");
            if (Convert.ToBoolean(cropData["winter"]))
                seasonsToGrowIn.Add("winter");
            perennial = Convert.ToBoolean(cropData["perennial"]);
            tropical = Convert.ToBoolean(cropData["tropical"]);
            multiHarvest = false;
            daysBetweenHarvest = 0;
            rowInSpriteSheet = Convert.ToInt32(cropData["parentSheetIndex"]);
            columnInSpriteSheet = 0;
            parseYears(cropData["growthYears"]);
            string[] npk = cropData["npk"].Split(' ');
            nReq = Convert.ToInt32(npk[0]);
            pReq = Convert.ToInt32(npk[1]);
            kReq = Convert.ToInt32(npk[2]);
            //hydrationRequirement = (Convert.ToInt32(cropData["hydration"]) / 100);
        }

        public override Dictionary<string, string> getCropFromXNB(string data)
        {
            Dictionary<string, string> cropData = new Dictionary<string, string>();
            string[] substrings = data.Split('/');
            try
            {
                cropData["parentSheetIndex"] = substrings[0];
                cropData["growthTimes"] = substrings[1];
                cropData["regrowthTimes"] = substrings[2];
                cropData["spring"] = substrings[3];
                cropData["summer"] = substrings[4];
                cropData["fall"] = substrings[5];
                cropData["winter"] = substrings[6];
                cropData["perennial"] = substrings[7];
                cropData["tropical"] = substrings[8];
                cropData["growthYears"] = substrings[9];
                cropData["npk"] = substrings[10];
                Logger.Log("Parsed successfully as root crop.");
                return cropData;
            }
            catch (IndexOutOfRangeException)
            {
                Logger.Log("Root crop data in Roots.xml is not in correct format!  Given\n" + data);
                return null;
            }
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
