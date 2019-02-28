using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using _SyrupFramework;

namespace Perennials
{
    class CropScythe : SoilCrop
    {
        public CropScythe() { }

        public CropScythe(string cropName, int heightOffset = 0) : base(cropName, heightOffset)
        {
            Logger.Log("CropScythe was created!");
            shakable = true;
            impassable = false;
        }

        public override void loadFromXNBData(Dictionary<string, string> cropData)
        {
            Logger.Log("Parsing as a scythe crop...");
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
                Logger.Log("Parsed successfully as scythe crop.");
                return cropData;
            }
            catch (IndexOutOfRangeException)
            {
                Logger.Log("Scythe crop data in Scythe.xml is not in correct format!  Given\n" + data);
                return null;
            }
        }
    }
}
