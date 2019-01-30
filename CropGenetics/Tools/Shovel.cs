using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Tools;
using _SyrupFramework;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using xTile.Dimensions;
using Netcode;

namespace Perennials
{
    public class Shovel : Tool, IModdedItem
    {
        public Shovel() : base("Shovel", 0, 21, 47, false, 0)
        {
            numAttachmentSlots.Value = 1;
            this.attachments.SetCount((int)((NetFieldBase<int, NetInt>)this.numAttachmentSlots));
            UpgradeLevel = 0;
        }

        public override int attachmentSlots()
        {
            return 1;
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            Logger.Log("Used shovel!");
            //if (!(location is Farm))
            //{
            //    return;
            //}
            //Global.Log("Shovel used on farm...");
            base.DoFunction(location, x, y, power, who);
            if (location.Name.Equals("UndergroundMine"))
                power = 1;
            who.Stamina = who.Stamina - ((float)(2 * power) - (float)who.FarmingLevel * 0.1f);
            power = who.toolPower;
            who.stopJittering();
            Game1.playSound("woodyHit");
            Vector2 vector2 = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            List<Vector2> vector2List = this.tilesAffected(vector2, power, who);
            foreach (Vector2 index in vector2List)
            {
                index.Equals(vector2);
                if (location.terrainFeatures.ContainsKey(index))
                {
                    if (location.terrainFeatures[index].performToolAction((Tool)this, 0, index, location))
                        location.terrainFeatures.Remove(index);
                }
                else
                {
                    if (location.doesTileHaveProperty((int)index.X, (int)index.Y, "Diggable", "Back") != null)
                    {
                        if (location.Name.Equals("UndergroundMine") && !location.isTileOccupied(index, ""))
                        {
                            if (Game1.mine.mineLevel < 40 || Game1.mine.mineLevel >= 80)
                            {
                                location.terrainFeatures.Add(index, (TerrainFeature)new HoeDirt());
                                Game1.playSound("hoeHit");
                            }
                            else if (Game1.mine.mineLevel < 80)
                            {
                                location.terrainFeatures.Add(index, (TerrainFeature)new HoeDirt());
                                Game1.playSound("hoeHit");
                            }
                            Game1.removeSquareDebrisFromTile((int)index.X, (int)index.Y);
                            location.checkForBuriedItem((int)index.X, (int)index.Y, false, false);
                            location.temporarySprites.Add(new TemporaryAnimatedSprite(12, new Vector2(vector2.X * (float)Game1.tileSize, vector2.Y * (float)Game1.tileSize), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, -1, 0));
                            if (vector2List.Count > 2)
                                location.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(index.X * (float)Game1.tileSize, index.Y * (float)Game1.tileSize), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance(vector2, index) * 30f, 0, -1, -1f, -1, 0));
                        }
                        else if (!location.isTileOccupied(index, "") && location.isTilePassable(new Location((int)index.X, (int)index.Y), Game1.viewport))
                        {
                            //location.makeHoeDirt(index);
                            CropSoil cropSoil = new CropSoil();
                            location.terrainFeatures[index] = cropSoil;
                            Game1.playSound("hoeHit");
                            Game1.removeSquareDebrisFromTile((int)index.X, (int)index.Y);
                            location.temporarySprites.Add(new TemporaryAnimatedSprite(12, new Vector2(index.X * (float)Game1.tileSize, index.Y * (float)Game1.tileSize), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, -1, 0));
                            if (vector2List.Count > 2)
                                location.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(index.X * (float)Game1.tileSize, index.Y * (float)Game1.tileSize), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance(vector2, index) * 30f, 0, -1, -1f, -1, 0));
                            location.checkForBuriedItem((int)index.X, (int)index.Y, false, false);
                            if (Game1.isRaining)
                            {
                                if (cropSoil.height == CropSoil.Lowered && !cropSoil.flooded)
                                    cropSoil.flooded = true;
                                if (!cropSoil.hydrated)
                                    cropSoil.hydrated = true;
                                cropSoil.holdOver = true;
                            }
                        }
                        ++Game1.stats.DirtHoed;
                    }
                }
            }
        }

        protected override string loadDisplayName()
        {
            return "Shovel";
        }

        protected override string loadDescription()
        {
            return "Can loosen soil into suitable crop fields, or shape terrain.";
        }

        public override Item getOne()
        {
            return new Shovel();
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
