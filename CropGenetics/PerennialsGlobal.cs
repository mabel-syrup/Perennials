using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using _SyrupFramework;
using Netcode;

namespace Perennials
{
    public static class PerennialsGlobal
    {
        private static Dictionary<Farmer, Boolean[]> farmerStates;

        private static Dictionary<Farmer, int> farmerHeight;
        private static Dictionary<Farmer, Boolean> farmerSubmerged;

        private const int raise = 0;
        private const int low = 0;
        private const int swim = 0;

        public static void initDictionaries()
        {
            if (farmerHeight is null)
                farmerHeight = new Dictionary<Farmer, int>();
            if (farmerSubmerged is null)
                farmerSubmerged = new Dictionary<Farmer, bool>();
        }

        public static void raiseFarmerTo(Farmer who, int newHeight)
        {
            initFarmerValues(who);
            raiseFarmerBy(who, heightDifference(who, newHeight));
        }

        public static int heightDifference(Farmer who, int newHeight)
        {
            initFarmerValues(who);
            return newHeight - farmerHeight[who];
        }

        public static void raiseFarmerBy(Farmer who, int heightToRaise)
        {
            initFarmerValues(who);
            who.position.Value = new Vector2(who.position.X, who.position.Y - heightToRaise);
            farmerHeight[who] += heightToRaise;
        }

        public static void setFarmerHeight(Farmer who, int height)
        {
            initFarmerValues(who);
            farmerHeight[who] = height;
        }

        public static void setFarmerSubmerged(Farmer who, bool submerged = true)
        {
            initFarmerState(who);
            farmerSubmerged[who] = submerged;
        }

        public static int getFarmerHeight(Farmer who)
        {
            initFarmerValues(who);
            return farmerHeight[who];
        }

        public static bool getFarmerSubmerged(Farmer who)
        {
            initFarmerValues(who);
            return farmerSubmerged[who];
        }

        private static void initFarmerValues(Farmer who)
        {
            if (!farmerHeight.ContainsKey(who))
                farmerHeight[who] = 0;
            if (!farmerSubmerged.ContainsKey(who))
                farmerSubmerged[who] = false;
        }

        public static void setFarmerRaised(Farmer who, bool raised = true)
        {
            if (!farmerStates.ContainsKey(who))
                initFarmerState(who);
            farmerStates[who][raise] = raised;
        }

        public static void setFarmerLowered(Farmer who, bool lowered = true)
        {
            if (!farmerStates.ContainsKey(who))
                initFarmerState(who);
            farmerStates[who][low] = lowered;
        }

        //public static void setFarmerSubmerged(Farmer who, bool submerged = true)
        //{
        //    if (!farmerStates.ContainsKey(who))
        //        initFarmerState(who);
        //    farmerStates[who][swim] = submerged;
        //}

        public static bool farmerRaised(Farmer who)
        {
            if (!farmerStates.ContainsKey(who))
                initFarmerState(who);
            return farmerStates[who][raise];
        }

        public static bool farmerLowered(Farmer who)
        {
            if (!farmerStates.ContainsKey(who))
                initFarmerState(who);
            return farmerStates[who][low];
        }

        //public static bool farmerSubmerged(Farmer who)
        //{
        //    if (!farmerStates.ContainsKey(who))
        //        initFarmerState(who);
        //    return farmerStates[who][swim];
        //}

        private static void initFarmerState(Farmer who)
        {
            if (!farmerStates.ContainsKey(who))
                farmerStates[who] = new Boolean[] { false, false, false };
        }

        public static void processWeeds(string season = null)
        {
            if (season == null)
                season = Game1.currentSeason;
            Farm farm = Game1.getFarm();
            List<Vector2> destroyedTiles = new List<Vector2>();
            Dictionary<Vector2, int> forageTiles = new Dictionary<Vector2, int>();
            foreach(Vector2 tileLocation in farm.terrainFeatures.Keys)
            {
                if(farm.terrainFeatures[tileLocation] is CropSoil)
                {
                    CropSoil soil = (CropSoil)farm.terrainFeatures[tileLocation];
                    if(soil.weeds && soil.crop == null)
                    {
                        double rand = Game1.random.NextDouble();
                        if (rand <= 0.08)
                        {
                            destroyedTiles.Add(tileLocation);
                        }
                        if (rand >= 0.98)
                        {
                            int indexOfForageCrop;
                            if (season == "spring")
                                indexOfForageCrop = 16 + Game1.random.Next(4) * 2;
                            else if (!(season == "summer"))
                            {
                                if (season == "fall")
                                    indexOfForageCrop = 404 + Game1.random.Next(4) * 2;
                                if (season == "winter")
                                    indexOfForageCrop = 412 + Game1.random.Next(4) * 2;
                                indexOfForageCrop = 22;
                            }
                            else if (Game1.random.NextDouble() < 0.33)
                                indexOfForageCrop = 396;
                            else
                                indexOfForageCrop = Game1.random.NextDouble() >= 0.5 ? 402 : 398;
                            forageTiles[tileLocation] = indexOfForageCrop;
                            soil.weeds = false;
                        }
                    }
                }
            }
            foreach(Vector2 tileLocation in destroyedTiles)
            {
                if (farm.terrainFeatures.ContainsKey(tileLocation))
                    farm.terrainFeatures.Remove(tileLocation);
            }
            foreach(Vector2 tileLocation in forageTiles.Keys)
            {
                if (farm.terrainFeatures.ContainsKey(tileLocation))
                    farm.terrainFeatures.Remove(tileLocation);
                if (farm.objects.ContainsKey(tileLocation))
                    continue;
                farm.objects.Add(tileLocation, new StardewValley.Object(tileLocation, forageTiles[tileLocation], "weeds", true, true, false, true));
            }
        }

        public static void simulateFarmDay(string spoofSeason = null)
        {
            processWeeds(spoofSeason);
            Farm farm = Game1.getFarm();
            foreach(Vector2 position in farm.terrainFeatures.Keys)
            {
                if(farm.terrainFeatures[position] is CropSoil)
                {
                    CropSoil soil = (CropSoil)farm.terrainFeatures[position];
                    soil.dayUpdate(farm, position, spoofSeason);
                }
            }
        }

        public static void equalizeDitches(GameLocation location)
        {
            if (location == null)
                location = Game1.getFarm();
            List<Ditch> ditches = new List<Ditch>();
            foreach(Vector2 position in location.terrainFeatures.Keys)
            {
                if(location.terrainFeatures[position] is CropSoil && ((CropSoil)location.terrainFeatures[position]).height == CropSoil.Lowered)
                {
                    CropSoil thisDitch = (CropSoil)location.terrainFeatures[position];
                    int waterLevel = 0;
                    if (thisDitch.flooded)
                        waterLevel += 3;
                    else if (thisDitch.hydrated)
                        waterLevel += 1;
                    if (thisDitch.holdOver)
                        waterLevel += 1;
                    bool newDitch = true;
                    foreach (Ditch ditch in ditches)
                    {
                        if (ditch.isAdjacent(position))
                        {
                            ditch.tiles.Add(position);
                            ditch.updateHighest(waterLevel);
                            newDitch = false;
                            break;
                        }
                    }
                    if (newDitch)
                    {
                        ditches.Add(new Ditch(position));
                        ditches[ditches.Count - 1].updateHighest(waterLevel);
                    }
                }
            }
            foreach(Ditch ditch in ditches)
            {
                foreach(Vector2 tile in ditch.tiles)
                {
                    CropSoil thisTile = (CropSoil)location.terrainFeatures[tile];
                    int waterLevel = ditch.getWaterLevel();
                    thisTile.flooded = waterLevel >= 3;
                    thisTile.holdOver = waterLevel == 2 || waterLevel == 4;
                    thisTile.hydrated = waterLevel >= 1;
                    if (thisTile.flooded)
                    {
                        thisTile.hydrateAdjacent(location, tile);
                    }
                }
            }
        }
    }
}
