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

        public override void loadFromXNBData(Dictionary<string, string> cropData)
        {
            Logger.Log("Parsing as a trellis crop...");
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
            parseMultiHarvest(cropData["daysBetweenHarvest"]);
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
                cropData["daysBetweenHarvest"] = substrings[3];
                cropData["spring"] = substrings[4];
                cropData["summer"] = substrings[5];
                cropData["fall"] = substrings[6];
                cropData["winter"] = substrings[7];
                cropData["perennial"] = substrings[8];
                cropData["tropical"] = substrings[9];
                cropData["growthYears"] = substrings[10];
                cropData["npk"] = substrings[11];
                Logger.Log("Parsed successfully as trellis crop.");
                return cropData;
            }
            catch (IndexOutOfRangeException)
            {
                Logger.Log("Trellis crop data in Trellis.xml is not in correct format!  Given\n" + data);
                return null;
            }
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
