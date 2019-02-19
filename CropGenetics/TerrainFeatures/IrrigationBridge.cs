using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _SyrupFramework;

namespace Perennials
{
    public class IrrigationBridge : TerrainFeature, IModdedItem, ILiquidContainer
    {
        public static Texture2D floorSheet;
        public static Texture2D fenceSheet;
        public static Dictionary<int, int> drawGuide;

        //For floor texture drawing
        private int selfAdjacency;
        //For rails
        private int levelAdjacency;

        public int waterLevel;

        public IrrigationBridge() : base(false)
        {
            waterLevel = 0;
        }

        public IrrigationBridge(int which) : this()
        {

        }

        public static void populateDrawGuide()
        {
            IrrigationBridge.drawGuide = new Dictionary<int, int>();
            IrrigationBridge.drawGuide.Add(0, 0);
            IrrigationBridge.drawGuide.Add(10, 15);
            IrrigationBridge.drawGuide.Add(100, 13);
            IrrigationBridge.drawGuide.Add(1000, 12);
            IrrigationBridge.drawGuide.Add(500, 4);
            IrrigationBridge.drawGuide.Add(1010, 11);
            IrrigationBridge.drawGuide.Add(1100, 9);
            IrrigationBridge.drawGuide.Add(1500, 8);
            IrrigationBridge.drawGuide.Add(600, 1);
            IrrigationBridge.drawGuide.Add(510, 3);
            IrrigationBridge.drawGuide.Add(110, 14);
            IrrigationBridge.drawGuide.Add(1600, 5);
            IrrigationBridge.drawGuide.Add(1610, 6);
            IrrigationBridge.drawGuide.Add(1510, 7);
            IrrigationBridge.drawGuide.Add(1110, 10);
            IrrigationBridge.drawGuide.Add(610, 2);
        }

        private void calculateAdjacency(Vector2 tileLocation, GameLocation location = null)
        {
            if (location is null)
                location = Game1.currentLocation;
            selfAdjacency = 0;
            levelAdjacency = 1610;
            Vector2 key = tileLocation;
            ++key.X;
            if (location.terrainFeatures.ContainsKey(key))
            {
                if(location.terrainFeatures[key] is IrrigationBridge)
                {
                    selfAdjacency += 100;
                }
                if(location.terrainFeatures[key] is CropSoil && (location.terrainFeatures[key] as CropSoil).height == CropSoil.Lowered)
                {
                    levelAdjacency -= 100;
                }
            }
            key.X -= 2f;
            if (location.terrainFeatures.ContainsKey(key))
            {
                if (location.terrainFeatures[key] is IrrigationBridge)
                {
                    selfAdjacency += 10;
                }
                if (location.terrainFeatures[key] is CropSoil && (location.terrainFeatures[key] as CropSoil).height == CropSoil.Lowered)
                {
                    levelAdjacency -= 10;
                }
            }
            ++key.X;
            ++key.Y;
            if (location.terrainFeatures.ContainsKey(key))
            {
                if (location.terrainFeatures[key] is IrrigationBridge)
                {
                    selfAdjacency += 500;
                }
                if (location.terrainFeatures[key] is CropSoil && (location.terrainFeatures[key] as CropSoil).height == CropSoil.Lowered)
                {
                    levelAdjacency -= 500;
                }
            }
            key.Y -= 2f;
            if (location.terrainFeatures.ContainsKey(key))
            {
                if (location.terrainFeatures[key] is IrrigationBridge)
                {
                    selfAdjacency += 1000;
                }
                if (location.terrainFeatures[key] is CropSoil && (location.terrainFeatures[key] as CropSoil).height == CropSoil.Lowered)
                {
                    levelAdjacency -= 1000;
                }
            }
        }

        public override Rectangle getBoundingBox(Vector2 tileLocation)
        {
            return new Rectangle((int)((double)tileLocation.X * 64.0), (int)((double)tileLocation.Y * 64.0), 64, 64);
        }

        public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who, GameLocation location)
        {
            base.doCollisionAction(positionOfCollider, speedOfCollision, tileLocation, who, location);
            if (who == null || !(who is Farmer) || !(location is Farm))
                return;
            (who as Farmer).temporarySpeedBuff = 0.1f;
            if (Game1.soundBank != null && (who == null || who.GetType() != typeof(FarmAnimal)) && !StepSoundHelper.woodSound.IsPlaying)
            {
                Logger.Log(who.yJumpOffset.ToString());
                StepSoundHelper.woodSound = Game1.soundBank.GetCue(getFootstepSound());
                StepSoundHelper.woodSound.Play();
            }
        }

        public string getFootstepSound()
        {
            return "woodyStep";
        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
        {
            if ((t != null && t.isHeavyHitter() && !(t is MeleeWeapon)) || t is Pickaxe)
            {
                location.playSound("axchop");
                CropSoil replacementSoil = new CropSoil();
                location.terrainFeatures[tileLocation] = replacementSoil;
                replacementSoil.height = CropSoil.Lowered;
                PerennialsGlobal.equalizeDitches(location);
                return false;
            }
            return false;
        }

        public override bool isPassable(Character c = null)
        {
            if (c is null)
                c = Game1.player;
            Vector2 characterTileLocation = c.getTileLocation();
            if (c.currentLocation.terrainFeatures.ContainsKey(characterTileLocation) && c.currentLocation.terrainFeatures[characterTileLocation] is CropSoil && (c.currentLocation.terrainFeatures[characterTileLocation] as CropSoil).height == CropSoil.Lowered)
                return false;
            return true;
        }

        public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            calculateAdjacency(tileLocation);
            int num1 = CropSoil.drawGuide[selfAdjacency];
            spriteBatch.Draw(floorSheet,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)),
                new Rectangle?(new Rectangle(num1 % 4 * 16, num1 / 4 * 16, 16, 16)),
                Color.White,
                0.0f,
                Vector2.Zero,
                (float)Game1.pixelZoom,
                SpriteEffects.None,
                1E-08f
            );
            drawFence(spriteBatch, tileLocation);
        }

        public void drawFence(SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            float depth = ((tileLocation.Y) * 64) / 10000f;
            float sideDepth = ((tileLocation.Y + 0.5f) * 64) / 10000f;
            float frontDepth = ((tileLocation.Y + 1) * 64) / 10000f;

            bool backEdge = levelAdjacency < 1000;
            bool leftEdge = levelAdjacency % 100 != 10;
            bool rightEdge = levelAdjacency % 500 < 100;
            bool frontEdge = levelAdjacency % 1000 < 500;
            //Back layer
            drawBackFence(spriteBatch, tileLocation, depth);
            //Sides
            if (leftEdge && rightEdge)
                drawSideFence(spriteBatch, tileLocation, 2, depth + 0.00001f, sideDepth, frontDepth - 0.00001f);
            else if (leftEdge)
                drawSideFence(spriteBatch, tileLocation, 0, depth + 0.00001f, sideDepth, frontDepth - 0.00001f);
            else if (rightEdge)
                drawSideFence(spriteBatch, tileLocation, 1, depth + 0.00001f, sideDepth, frontDepth - 0.00001f);
            //Front
            drawFrontFence(spriteBatch, tileLocation, frontDepth);
        }

        private void drawSideFence(SpriteBatch spriteBatch, Vector2 tileLocation, int which, float backDepth, float midDepth, float frontDepth)
        {
            if (levelAdjacency % 500 >= 100 && levelAdjacency % 100 >= 10)
                return;
            bool backEdge = selfAdjacency < 1000;
            bool frontEdge = selfAdjacency % 1000 < 500;
            int xIndex = which + 4;

            //Draw the back post
            spriteBatch.Draw(fenceSheet,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, (tileLocation.Y) * (float)Game1.tileSize)),
                new Rectangle(xIndex * 16, 0, 16, 32),
                Color.White,
                0.0f,
                new Vector2(0, 24),
                (float)Game1.pixelZoom,
                SpriteEffects.None,
                backDepth
            );

            //Draw the rail
            spriteBatch.Draw(fenceSheet,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, (tileLocation.Y) * (float)Game1.tileSize)),
                new Rectangle(xIndex * 16, 64, 16, 32),
                Color.White,
                0.0f,
                new Vector2(0, 16),
                (float)Game1.pixelZoom,
                SpriteEffects.None,
                midDepth
            );

            //Draw the front post
            spriteBatch.Draw(fenceSheet,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, (tileLocation.Y + 1) * (float)Game1.tileSize)),
                new Rectangle(xIndex * 16, 32, 16, 32),
                Color.White,
                0.0f,
                new Vector2(0, 24),
                (float)Game1.pixelZoom,
                SpriteEffects.None,
                frontDepth
            );
            return;


            //If the side-fence ends in the back, render the back post
            if (backEdge)
            {
                spriteBatch.Draw(fenceSheet,
                    Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, (tileLocation.Y) * (float)Game1.tileSize)),
                    new Rectangle(xIndex * 16, 0, 16, 32),
                    Color.White,
                    0.0f,
                    new Vector2(0, 24),
                    (float)Game1.pixelZoom,
                    SpriteEffects.None,
                    midDepth
                );
            }
            //If it ends in the front, render the front post
            if (frontEdge)
            {
                spriteBatch.Draw(fenceSheet,
                    Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, (tileLocation.Y + 1) * (float)Game1.tileSize)),
                    new Rectangle(xIndex * 16, 32, 16, 32),
                    Color.White,
                    0.0f,
                    new Vector2(0, 24),
                    (float)Game1.pixelZoom,
                    SpriteEffects.None,
                    midDepth
                );
            }
            //If it does not end in the back or the front, draw the mid-section.
            else if (!backEdge)
            {
                spriteBatch.Draw(fenceSheet,
                    Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, (tileLocation.Y + 1) * (float)Game1.tileSize)),
                    new Rectangle(xIndex * 16, 16, 16, 16),
                    Color.White,
                    0.0f,
                    new Vector2(0, 24),
                    (float)Game1.pixelZoom,
                    SpriteEffects.None,
                    midDepth
                );
            }
        }

        private void drawBackFence(SpriteBatch spriteBatch, Vector2 tileLocation, float depth)
        {
            //If the back edge of the tile is level (i.e. is against a ditch)
            if (levelAdjacency >= 1000)
                return;
            //If the right edge is not connected to more bridge
            bool rightEdge = selfAdjacency % 500 < 100;
            bool leftEdge = selfAdjacency % 100 < 10;
            spriteBatch.Draw(fenceSheet,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)),
                getFenceRectFrontBack(0, rightEdge, leftEdge),
                Color.White,
                0.0f,
                new Vector2(0, 24),
                (float)Game1.pixelZoom,
                SpriteEffects.None,
                depth
            );
        }

        private void drawFrontFence(SpriteBatch spriteBatch, Vector2 tileLocation, float depth)
        {
            //If the front edge of the tile is level (i.e. is against a ditch)
            if (levelAdjacency % 1000 >= 500)
                return;
            //If the right edge is not connected to more bridge
            bool rightEdge = selfAdjacency % 500 < 100;
            bool leftEdge = selfAdjacency % 100 < 10;
            spriteBatch.Draw(fenceSheet,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, (tileLocation.Y + 1) * (float)Game1.tileSize)),
                getFenceRectFrontBack(1, rightEdge, leftEdge),
                Color.White,
                0.0f,
                new Vector2(0, 24),
                (float)Game1.pixelZoom,
                SpriteEffects.None,
                depth
            );
        }

        private Rectangle getFenceRectFrontBack(int layer, bool rightEdge, bool leftEdge)
        {
            int xIndex = (rightEdge && leftEdge ? 0 : (rightEdge ? 3 : (leftEdge ? 1 : 2)));
            return new Rectangle(xIndex * 16, layer * 32, 16, 32);
        }

        private Rectangle getFenceForLayerOld(int layer, bool backEdge, bool rightEdge, bool leftEdge, bool frontEdge)
        {
            
            int xIndex = 0;
            int yIndex = 0;
            //Set the y index to the layer we're drawing for
            yIndex += layer;
            //Move y index down one if this is not also touching the back
            if (layer == 1 && !backEdge)
                yIndex += 1;
            //Move the x index to 0 for both sides as fences, 3 for right edge, 1 for left edge, and 2 for middle.
            xIndex += (rightEdge && leftEdge ? 0 : (rightEdge ? 3 : (leftEdge ? 1 : 2)));

            return new Rectangle(xIndex * 16, yIndex * 32, 16, 32);
        }

        public void Load(Dictionary<string, string> data)
        {
            return;
        }

        public Dictionary<string, string> Save()
        {
            return new Dictionary<string, string>();
        }

        public void addLiquid(int amount)
        {
            waterLevel += amount;
            waterLevel = Math.Min(waterLevel, 16);
            waterLevel = Math.Max(0, waterLevel);
        }

        public void setLiquid(int amount)
        {
            addLiquid(amount - waterLevel);
        }

        public int getLiquidAmount()
        {
            return waterLevel;
        }
    }
}
