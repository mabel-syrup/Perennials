using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using Microsoft.Xna.Framework;

namespace Perennials
{
    public class MultiTileDitch
    {
        private List<Vector2> tiles;

        public MultiTileDitch(Vector2 tile)
        {
            tiles = new List<Vector2>();
            tiles.Add(tile);
        }

        public int getSize()
        {
            return tiles.Count;
        }

        public void setWaterLevels(GameLocation location, int level)
        {
            level = Math.Min(16, level);
            foreach (Vector2 tile in tiles)
            {
                //if (location.terrainFeatures.Keys.Contains(tile) && location.terrainFeatures[tile] is CropSoil)
                //{
                //    CropSoil ditch = (location.terrainFeatures[tile] as CropSoil);
                //    ditch.floodLevel = level;
                    
                //}

                if(location.terrainFeatures.ContainsKey(tile) && location.terrainFeatures[tile] is ILiquidContainer)
                {
                    ILiquidContainer container = (location.terrainFeatures[tile] as ILiquidContainer);
                    container.setLiquid(level);
                    if(location.terrainFeatures[tile] is CropSoil)
                    {
                        (location.terrainFeatures[tile] as CropSoil).hydrateAdjacent(location, tile);
                    }
                }
            }
        }

        public int getWaterContent(GameLocation location)
        {
            int totalWater = 0;
            int fromSources = 0;
            int drainage = 0;
            bool dumpAll = false;
            foreach(Vector2 tile in tiles)
            {
                //if(location.terrainFeatures.Keys.Contains(tile) && location.terrainFeatures[tile] is CropSoil)
                //{
                //    CropSoil ditch = (location.terrainFeatures[tile] as CropSoil);
                //    if (ditch.flooded)
                //    {
                //        totalWater += ditch.floodLevel;
                //    }
                //    else
                //    {
                //        totalWater += ditch.hydrated ? 1 : 0;
                //        totalWater += ditch.holdOver ? 1 : 0;
                //    }
                //}

                if(location.terrainFeatures.ContainsKey(tile) && location.terrainFeatures[tile] is ILiquidContainer)
                {
                    ILiquidContainer container = (location.terrainFeatures[tile] as ILiquidContainer);
                    totalWater += container.getLiquidAmount();
                }
                if(location.objects.Keys.Contains(tile) && location.objects[tile] is Irrigator)
                {
                    int amount = (location.objects[tile] as Irrigator).waterAmount();
                    if (amount >= 0)
                        fromSources += amount;
                    else if (amount > -1000)
                        drainage += amount;
                    else
                        dumpAll = true;
                }
            }

            //Drains away as much water as the drainage capacity can handle, capping at 0.
            totalWater = Math.Max(0, totalWater - drainage);
            //Adds up to the amount added by spigots to the water level.
            totalWater += Math.Max(0, fromSources - totalWater);
            //Drains the irrigation completely when using a drain with a "limitless" drainage capacity, regardless of fill amount
            if (dumpAll)
                totalWater -= 1000;
            return totalWater;
        }

        public void addTile(Vector2 tile)
        {
            if (tiles.Contains(tile))
                return;
            tiles.Add(tile);
        }

        public bool isConnected(Vector2 tileLocation)
        {
            foreach(Vector2 tile in tiles)
            {
                if ((int)(Math.Abs(tileLocation.X - tile.X) + Math.Abs(tileLocation.Y - tile.Y)) == 1)
                {
                    return true;
                }
            }
            return false;
        }

        public bool shouldMerge(MultiTileDitch candidate)
        {
            foreach(Vector2 candidateTile in candidate.tiles)
            {
                if (isConnected(candidateTile))
                {
                    return true;
                }
            }
            return false;
        }

        public void merge(MultiTileDitch secondDitch)
        {
            foreach(Vector2 tile in secondDitch.tiles)
            {
                addTile(tile);
            }
        }
    }
}
