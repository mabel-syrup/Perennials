using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using xTile;
using StardewValley;
using _SyrupFramework;


namespace Perennials
{
    public class SoilCrop
    {
        
        public static Dictionary<string, string> cropDictionary;
        public static Texture2D cropSpriteSheet;
        public static List<int> ageQualities = new List<int> {0, 50, 90, 120, 145, 165, 180, 189, 195, 198, 200 };

        //Which species this is.
        public string crop;

        //Species-specific, not individual-specific.
        public List<int> growthStages;
        public List<int> regrowthStages;
        public List<string> seasonsToGrowIn;
        public bool perennial;
        private bool tropical;
        private bool multiHarvest;
        private int daysBetweenHarvest;
        public int rowInSpriteSheet;
        public int columnInSpriteSheet;
        private double hydrationRequirement;
        public bool impassable = false;
        public bool shakable = true;
        public string specialType;
        private int yearsToFruit;
        public int nReq;
        public int pReq;
        public int kReq;

        //Individual-specific
        public bool dead;
        public bool regrowing;
        public int dayOfCurrentPhase;
        public int phaseToShow = -1;
        public int currentPhase;
        public int currentSprite;
        public bool hasFruit = false;
        private bool hasMatured = false;

        public bool mature = false;
        public bool dormant = false;
        public int seedMaturity = 0;
        public int regrowMaturity = 0;
        public int seedDaysToMature = 0;
        public int regrowDaysToMature = 0;

        private int daysUntilHarvest = 0;
        public double hydration;
        public int neighbors;
        public double weedless;
        public double fertilized;
        public int age;
        public int years = 0;
        public int heightOffset;

        public bool weeds = false;
        public int n;
        public int p;
        public int k;
        public bool flip = false;
        private Color tintColor = Color.ForestGreen;
        private int seasonOffset;


        public SoilCrop()
        {
            
        }

        //Growing days/Fruiting days/Spring/Summer/Fall/Winter/Perennial/Hardy/Multiple Harvests/Regrow Time/Sprite Index

        public static string getCropType(string cropName)
        {
            if (cropDictionary.ContainsKey(cropName))
            {
                Dictionary<string, string> cropData = getCropFromXNB(cropDictionary[cropName]);
                return cropData["specialType"];
            }
            else
            {
                Logger.Log("No crop found by the name " + cropName);
                return "";
            }
        }

        public SoilCrop(string cropName, int heightOffset=0)
        {
            if (cropDictionary.ContainsKey(cropName))
            {
                crop = cropName;
                dead = false;
                age = 0;
                currentPhase = 0;
                regrowing = false;
                dayOfCurrentPhase = 0;
                hydration = 0f;
                neighbors = 0;
                this.heightOffset = heightOffset;
                growthStages = new List<int>();
                regrowthStages = new List<int>();
                seasonsToGrowIn = new List<string>();
                seasonOffset = offsetForSeason();
                nReq = 1;
                pReq = p;
                kReq = k;
                hydrationRequirement = 1f;

                Dictionary<string, string> cropData = getCropFromXNB(cropDictionary[cropName]);

                string[] growStages = cropData["growthTimes"].Split(' ');
                foreach (string stage in growStages)
                {
                    //Logger.Log("Adding growthstage of " + stage + " to " + cropName + "'s growth stages...");
                    growthStages.Add(Convert.ToInt32(stage));
                }
                string[] regrowStages = cropData["fruitTimes"].Split(' ');
                foreach (string stage in regrowStages)
                    regrowthStages.Add(Convert.ToInt32(stage));
                if (Convert.ToBoolean(cropData["spring"]))
                    seasonsToGrowIn.Add("spring");
                if (Convert.ToBoolean(cropData["summer"]))
                    seasonsToGrowIn.Add("summer");
                if (Convert.ToBoolean(cropData["fall"]))
                    seasonsToGrowIn.Add("fall");
                if (Convert.ToBoolean(cropData["winter"]))
                    seasonsToGrowIn.Add("winter");
                perennial = Convert.ToBoolean(cropData["perennial"]);
                tropical = Convert.ToBoolean(cropData["hardy"]);
                multiHarvest = Convert.ToBoolean(cropData["multipleHarvests"]);
                daysBetweenHarvest = Convert.ToInt32(cropData["nextHarvest"]);
                rowInSpriteSheet = Convert.ToInt32(cropData["parentSheetIndex"]);
                columnInSpriteSheet = 0;
                specialType = cropData["specialType"];
                yearsToFruit = Convert.ToInt32(cropData["growthYears"]);
                hydrationRequirement = (Convert.ToInt32(cropData["hydration"]) / 100);
                //int parentSheetIndex = Convert.ToInt32(cropData["parentSheetIndex"]);
                //rowInSpriteSheet = (int)Math.Floor((Double)(parentSheetIndex / 2));
                //columnInSpriteSheet = parentSheetIndex % 2;
                flip = Game1.random.NextDouble() < 0.5;
                Logger.Log("Successfully created a " + crop + " plant. Sprite: row " + rowInSpriteSheet + ", col " + columnInSpriteSheet);
                calculateGrowthPatterns();
                updateSpriteIndex();
            }
            else
            {
                throw new KeyNotFoundException("The Crops.xml file did not contain a valid definition for the crop name.  Please contact the mod author if this issue persists.");
            }
        }

        public static Dictionary<string, string> getCropFromXNB(string data)
        {
            Dictionary<string, string> cropData = new Dictionary<string, string>();
            string[] substrings = data.Split('/');
            try
            {
                cropData["parentSheetIndex"] = substrings[0];
                cropData["growthTimes"] = substrings[1];
                cropData["fruitTimes"] = substrings[2];
                cropData["spring"] = substrings[3];
                cropData["summer"] = substrings[4];
                cropData["fall"] = substrings[5];
                cropData["winter"] = substrings[6];
                cropData["perennial"] = substrings[7];
                cropData["hardy"] = substrings[8];
                cropData["multipleHarvests"] = substrings[9];
                cropData["nextHarvest"] = substrings[10];
                cropData["specialType"] = substrings[11];
                cropData["growthYears"] = substrings[12];
                cropData["hydration"] = substrings[13];
                Logger.Log("Parsed successfully.");
                Logger.Log("Special type: " + cropData["specialType"] + " Growth Years: " + cropData["growthYears"]);
                return cropData;
            }
            catch (IndexOutOfRangeException)
            {
                Logger.Log("Crop data is not in correct format!  Given\n" + data);
                return null;
            }
        }

        public virtual void flood()
        {
            dead = true;
        }

        public bool produceWeeds()
        {
            if (specialType.Equals("Scythe") || specialType.Equals("Bush"))
                return false;
            weeds = true;
            return true;
        }

        public bool isScytheHarvest()
        {
            return specialType.Equals("Schythe");
        }

        public virtual bool isSeed()
        {
            return getCurrentPhase() == 1 && !mature;
        }

        public int getCurrentPhase()
        {
            int phase = 1;
            int growthSum = 0;
            if (dormant)
            {
                foreach(int growthStage in regrowthStages)
                {
                    growthSum += growthStage;
                    if (regrowMaturity > growthSum)
                        phase++;
                    else
                        break;
                }
            }
            else
            {
                foreach(int growthStage in growthStages)
                {
                    growthSum += growthStage;
                    if (seedMaturity > growthSum)
                        phase++;
                    else
                        break;
                }
            }
            return phase;
        }

        public virtual void updateSpriteIndex(string spoofSeason = null)
        {
            if (spoofSeason is null)
                spoofSeason = Game1.currentSeason;
            int flipped = flip ? 1 : 0;
            if (specialType == "Trellis")
            {
                if (!isGrowingSeason(spoofSeason))
                {
                    int seasonSprite;
                    switch (spoofSeason)
                    {
                        case "spring":
                            seasonSprite = 0;
                            break;
                        case "summer":
                            seasonSprite = 2;
                            break;
                        case "fall":
                            seasonSprite = 4;
                            break;
                        case "winter":
                            seasonSprite = 6;
                            break;
                        default:
                            seasonSprite = 0;
                            break;
                    }
                    currentSprite = 8 + seasonSprite + flipped;
                    return;
                }
            }
            if (!mature)
                currentSprite = getCurrentPhase();
            else
                currentSprite = 6 + (hasFruit ? 0 : 1);
            //Logger.Log("Updated sprite index for " + crop + ", sprite index is now " + currentSprite);
        }

        public bool isGrowingSeason(string season, GameLocation environment=null)
        {
            if (!(environment is null) && environment.Name.Equals("Greenhouse"))
                return true;
            return seasonsToGrowIn.Contains(season);
        }

        private void calculateGrowthPatterns()
        {
            //Logger.Log("Calculating growth patterns for " + crop + "...");
            seedDaysToMature = 0;
            regrowDaysToMature = 0;
            foreach(int stage in growthStages)
            {
                seedDaysToMature += stage;
                //Logger.Log("Adding " + stage + "days to seed maturity. Seed maturity total is now " + seedDaysToMature);
            }
            foreach(int stage in regrowthStages)
            {
                regrowDaysToMature += stage;
                //Logger.Log("Adding " + stage + "days to regrowth maturity. Regrowth maturity total is now " + regrowDaysToMature);
            }
            //Logger.Log("Calculated for " + crop + ": " + seedDaysToMature + " seedDaysToMature, " + regrowDaysToMature + " regrowDaysToMature.");
        }

        public virtual void growFruit(string season)
        {
            //Even crops in greenhouses only fruit in their season.  Some crops can ONLY mature in the greenhouse, but still only fruit in their season.
            if (!isGrowingSeason(season))
                return;
            if (years < yearsToFruit)
            {
                Logger.Log(crop + " produced no fruit, must be " + yearsToFruit + " years old, currently " + years + ".");
                return;
            }
            if (daysBetweenHarvest > 0 && daysUntilHarvest > 0)
                daysUntilHarvest--;
            if(daysBetweenHarvest == 0 || daysUntilHarvest <= 0)
            {
                hasFruit = true;
            }
        }

        public virtual bool grow(bool hydrated, bool flooded, int xTile, int yTile, GameLocation environment, string spoofSeason = null)
        {
            string season;
            if (spoofSeason != null)
                season = spoofSeason;
            else
                season = Game1.currentSeason;

            seasonOffset = offsetForSeason(season);

            bool growingSeason = isGrowingSeason(season, environment);

            bool grew = false;

            string report = crop + " growth report: ";
            //The crop is able to grow now, if it needs to.
            if (growingSeason)
            {
                report += season + " is within growing season, ";
                //The crop is growing from seeds.  This is a separate lifestage from regrowth in perennials.
                if (!mature && !dormant)
                {
                    report += "was not mature, and was not dormant, ";
                    //Reset regrowMaturity, just in case.
                    regrowMaturity = 0;
                    //Progress the seed maturity by one, flagging this crop as mature if it's done.
                    seedMaturity++;
                    report += "is " + seedMaturity + " days into seed growth out of " + seedDaysToMature + " needed to mature";
                    if(seedMaturity >= seedDaysToMature)
                    {
                        report += ", making it mature. ";
                        mature = true;
                    }
                    else
                    {
                        report += ".";
                    }
                }
                //The crop is regrowing.  This is typically much faster and with different sprites.
                else if (!mature && dormant)
                {
                    report += "was not mature, and was dormant, ";
                    regrowMaturity++;
                    report += "is " + regrowMaturity + " days into regrowth out of " + regrowDaysToMature + " needed to mature";
                    if (regrowMaturity >= regrowDaysToMature)
                    {
                        report += ", making it mature. ";
                        mature = true;
                        dormant = false;
                    }
                    else
                    {
                        report += ".";
                    }
                }
                //New if statement.  If the crop has finished growing, we want to immediately progress its fruiting.
                if (mature && !dormant)
                {
                    report += "was mature, and was not dormant, so it will progress the fruiting cycle.";
                    //This crop is growing fruit at this point.
                    growFruit(season);
                }
                grew = true;
            }
            //The crop is outside its growing season.  This does not necessarily kill it.
            else
            {
                if (isSeed())
                {
                    report += "currently a seed outside of its growing season, lying in wait.";
                    //Do nothing, the seed is dormant.
                }
                //Bushes retain their progress in the off-season, so long as they aren't tropical.
                else if (specialType.Equals("Bush") && !tropical)
                {
                    if(age > 0 && mature)
                    {
                        report += "bush reached maturity before the end of the growing season, beginning its regrowth cycle.";
                        age = 0;
                        years++;
                        dormant = true;
                        mature = false;
                        regrowMaturity = 0;
                        seedMaturity = 0;
                    }
                    report += "bush is out of its growing season, but isn't tropical.  Simply waiting.";
                    //Do nothing, it's a non-tropical bush and should retain its progress.
                }
                else if (age > 0 && perennial)
                {
                    report += "a perennial with growth that is out of season, ";
                    //The crop reached maturity before the off-season, so it will be on its regrow cycle next year.
                    if (mature)
                    {
                        report += "mature and therefore beginning its regrowth cycle.";
                        dormant = true;
                        mature = false;
                        regrowMaturity = 0;
                        seedMaturity = 0;
                    }
                    //The crop was not mature before the off-season, and will need to regrow from seed.
                    else
                    {
                        report += "not mature, and going to regrow from seed.";
                        dormant = false;
                        mature = false;
                        regrowMaturity = 0;
                        seedMaturity = 0;
                    }
                    years++;
                    age = 0;
                    hydration = 0f;
                    weedless = 1f;
                    report += " It is now " + years + " years old.";
                }
                else if (specialType.Equals("Root"))
                {
                    report += "an out of season root, ";
                    if (!mature)
                    {
                        report += "that has died due to not reaching maturity.";
                        dead = true;
                    }
                    else
                    {
                        report += "that is preserved for later harvest.";
                        //Do nothing.  Todo: make roots improve in quality after freezing?
                    }
                }
                else
                {
                    report += "out of season, and now dead.";
                    //The crop is dead.
                    dead = true;
                }
            }
            //Logger.Log(report);
            return grew;
        }

        public void newDay(bool hydrated, bool flooded, int xTile, int yTile, GameLocation environment, string spoofSeason = null)
        {
            //If the crop grew at all.
            if (grow(hydrated, flooded, xTile, yTile, environment, spoofSeason))
            {
                //Logger.Log("Grew " + crop + " by one day.");
                age++;
                hydration = (((age - 1) * hydration) + (hydrated ? 1 : 0)) / age;
                weedless = (((age - 1) * weedless) + (weeds ? 0 : 1)) / age;
                fertilized = (((age - 1) * fertilized) + (fertilizationCorrectness())) / age;
            }
            else
            {
                //Logger.Log(crop + " did not grow today.");
                age = 0;
                hydration = 0f;
                weedless = 1f;
            }
            //string report = crop;
            //report += " is " + (dead ? "dead." : (mature ? " " : "not ") + "mature, is " + (isSeed() ? "" : "not ") + "a seed" + (
            //    mature ? "" : ", is " + (dormant ? " regrowing, and " + regrowMaturity + "days regrown." : " growing from seed, and " + seedMaturity + "days grown.")));
            updateSpriteIndex(spoofSeason);
            //Logger.Log(reportCondition(spoofSeason));
            //Logger.Log(report);
        }

        private float fertilizationCorrectness()
        {
            return 1f - (Math.Min(n - nReq, 0) + Math.Min(p - pReq, 0) + Math.Min(k - kReq, 0) / -6f);
        }

        public int calculateQuality(StardewValley.Farmer who = null)
        {
            if(who == null)
                who = Game1.player;
            //quality calculations
            double quality = 0;
            //Hydration is affected double for underwatering than for overwatering.
            double waterQuality = (1 - ((hydration < hydrationRequirement) ? ((hydrationRequirement - hydration) * 2) : ((hydration / hydrationRequirement >= 2) ? (hydration - hydrationRequirement) - hydration : 0))) * 100;
            //Quality categories are never negative.
            if (waterQuality < 0)
                waterQuality = 0;
            quality += waterQuality;
            quality += weedless * 50;
            //todo: fertilizer bonus
            quality += fertilized * 150;
            quality += Math.Min(neighbors * 25, 75);
            int ageQuality = ageQualities[Math.Min(years, 10)];
            //quality += Math.Min(years * 20, 200);
            quality += ageQuality;
            quality += (who.FarmingLevel * 10);
            Logger.Log("Quality breakdown:\n" +
                "Water: " + waterQuality + "\n" +
                "Weedless: " + weedless * 50 + "\nFertilizer:" + fertilized * 150 + "\n" +
                "Adjacency: " + Math.Min(neighbors * 25, 75) + "\n" +
                "Age: " + ageQuality + "\n" +
                "Farmer: " + (who.FarmingLevel * 10));
            int qualityLevel;
            if (quality >= 500)
                qualityLevel = Fruit.bestQuality;
            else if (quality >= 250)
                qualityLevel = Fruit.highQuality;
            else if (quality >= 100)
                qualityLevel = Fruit.medQuality;
            else
                qualityLevel = Fruit.lowQuality;
            Logger.Log("Final quality decision: " + qualityLevel + ", from " + (int)quality + "/675 points.");
            return qualityLevel;
        }

        public virtual bool harvest(Vector2 tileLocation)
        {
            if (dead)
                return true;
            if (hasFruit)
            {
                StardewValley.Farmer who = Game1.player;
                int qualityLevel = calculateQuality(who);

                hasFruit = false;
                StardewValley.Object fruitItem = new Fruit(crop, 1, qualityLevel);


                if (isScytheHarvest())
                {
                    DelayedAction.playSoundAfterDelay("daggerswipe", 150);
                    createFruitDebris(fruitItem, tileLocation);
                }
                else if (!multiHarvest)
                {
                    if (!who.addItemToInventoryBool(fruitItem))
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
                        return false;
                    }
                    Game1.playSound("harvest");
                    Game1.player.animateOnce(279 + Game1.player.facingDirection);
                    Game1.player.canMove = false;
                    Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(17, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize), Color.White, 7, Game1.random.NextDouble() < 0.5, 125f, 0, -1, -1f, -1, 0));
                    Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize), Color.White, 7, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, -1, 0));
                }
                else
                {
                    Game1.playSound("dwoop");
                    createFruitDebris(fruitItem, tileLocation);
                }
                float experience = (float)(16.0 * Math.Log(0.018 * fruitItem.price + 1.0, Math.E));
                who.gainExperience(0, (int)Math.Round((double)0.5));
                if (!multiHarvest)
                    return true;
                //dayOfCurrentPhase = 0;
                daysUntilHarvest = daysBetweenHarvest;
                updateSpriteIndex();
            }
            return false;
        }

        public virtual void beforeDestroy()
        {

        }

        public void createFruitDebris(StardewValley.Object fruitItem, Vector2 tileLocation, float velocityMultiplier=1f)
        {
            Debris debris = new Debris(fruitItem, new Vector2((float)(tileLocation.X * Game1.tileSize + Game1.tileSize / 2), (float)(tileLocation.Y * Game1.tileSize + Game1.tileSize / 2)))
            {
                itemQuality = fruitItem.quality
            };
            foreach (Chunk chunk in debris.Chunks)
            {
                double num1 = (double)chunk.xVelocity * velocityMultiplier;
                chunk.xVelocity.Value = (float)num1;
                double num2 = (double)chunk.yVelocity * velocityMultiplier;
                chunk.yVelocity.Value = (float)num2;
            }
            Game1.currentLocation.debris.Add(debris);
        }

        public int daysForCurrentPhase()
        {
            if (mature)
                return -1;
            return dormant ? regrowthStages[getCurrentPhase()] : growthStages[getCurrentPhase()];
        }

        private int offsetForSeason(string season = null)
        {
            if (season == null)
                season = Game1.currentSeason;
            switch (season)
            {
                case "winter":
                    return 3;
                case "fall":
                    return 2;
                case "summer":
                    return 1;
                default:
                    return 0;
            }
        }

        public virtual Rectangle getSprite(int number = 0)
        {
            //Vanilla dead sprite selection
            if (this.dead)
                return new Rectangle(64 + number % 4 * 16, 1024, 16, 32);
            else if (isSeed() && (!specialType.Equals("Bush") || !dormant))
                return new Rectangle((columnInSpriteSheet * 128) + ((currentSprite - (number % 2)) * 16), rowInSpriteSheet * 32, 16, 32);
            return new Rectangle((columnInSpriteSheet * 128) + (currentSprite * 16), rowInSpriteSheet * 32, 16, 32);
        }

        public virtual void draw(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
        {
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((double)tileLocation.X * (double)Game1.tileSize) + (float)(Game1.tileSize / 2), (float)((double)tileLocation.Y * (double)Game1.tileSize) + (Game1.tileSize / 2) - (Game1.tileSize * heightOffset / 4)));
            float depth = (float)((tileLocation.Y * 64 + 4) + (tileLocation.Y * 11.0 + tileLocation.X * 7.0) % 10.0 - 5.0) / 10000f;
            b.Draw(cropSpriteSheet,
                local,
                getSprite((int)tileLocation.X * 7 + (int)tileLocation.Y * 11),
                toTint,
                rotation,
                new Vector2(8f, 24f),
                (float)Game1.pixelZoom,
                this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                depth
                //(float)((double)tileLocation.Y * (double)Game1.tileSize + (double)(Game1.tileSize / 2))
            );
        }

        public virtual void drawInMenu(SpriteBatch b, Vector2 screenPosition, Color toTint, float rotation, float scale, float layerDepth)
        {
            b.Draw(cropSpriteSheet, screenPosition, new Rectangle?(this.getSprite(0)), toTint, rotation, new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize + Game1.tileSize / 2)), scale, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }


        public string reportCondition(string season = null)
        {
            if (season is null)
                season = Game1.currentSeason;
            return "Currently " + (dead ? "dead." : (dormant ? "regrowing, " + regrowMaturity : "growing, " + seedMaturity) + " days into growth, and in phase " + getCurrentPhase() + ". Hydration level is " + hydration + ". Crop is " + age + " days old. ") + "It is " + (isGrowingSeason(season) ? "" : "not ") + "this crop's growing season.";
            //return "Currently " + (dead ? "dead." : (regrowing ? "regrowing, " : " ") + dayOfCurrentPhase + " days into current phase, and in phase " + currentPhase + ". Hydration level is " + hydration + ". Crop is " + age + " days old.") + "It is " + (seasonsToGrowIn.Contains(Game1.currentSeason) ? "" : "not ") + "this crop's growing season.";
        }

        public Dictionary<string, string> Save()
        {
            /*
            public int dayOfCurrentPhase;
            public int phaseToShow = -1;
            public int currentPhase;
            public int currentSprite;
            private double hydration;
            private double sunlight;
            private int age;
            public bool hasFruit = false;
            private int daysUntilHarvest = 0;
            */
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["dead"] = dead.ToString();
            data["dormant"] = regrowing.ToString();
            data["mature"] = mature.ToString();
            data["seedMaturity"] = seedMaturity.ToString();
            data["regrowMaturity"] = regrowMaturity.ToString();
            data["hydration"] = hydration.ToString();
            data["weedless"] = weedless.ToString();
            data["neighbors"] = neighbors.ToString();
            data["age"] = age.ToString();
            data["years"] = years.ToString();
            data["hasFruit"] = hasFruit.ToString();
            data["daysUntilHarvest"] = daysUntilHarvest.ToString();
            return data;
        }

        public virtual void Load(Dictionary<string,string> data)
        {
            try
            {
                dead = Convert.ToBoolean(data["dead"]);
                dormant = Convert.ToBoolean(data["dormant"]);
                mature = Convert.ToBoolean(data["mature"]);
                seedMaturity = Convert.ToInt32(data["seedMaturity"]);
                regrowMaturity = Convert.ToInt32(data["regrowMaturity"]);
                hydration = Convert.ToDouble(data["hydration"]);
                weedless = Convert.ToDouble(data["weedless"]);
                neighbors = Convert.ToInt32(data["neighbors"]);
                age = Convert.ToInt32(data["age"]);
                years = Convert.ToInt32(data["years"]);
                hasFruit = Convert.ToBoolean(data["hasFruit"]);
                daysUntilHarvest = Convert.ToInt32(data["daysUntilHarvest"]);
            }
            catch (KeyNotFoundException)
            {
                dead = false;
                dormant = false;
                mature = false;
                seedMaturity = 0;
                regrowMaturity = 0;
                hydration = 0f;
                weedless = 1f;
                neighbors = 0;
                age = 0;
                years = 0;
                hasFruit = false;
                daysUntilHarvest = daysBetweenHarvest;
            }
            calculateGrowthPatterns();
            updateSpriteIndex();
        }
    }
}
