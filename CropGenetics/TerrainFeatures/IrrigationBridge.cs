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
        public static Texture2D spriteSheet;
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
            spriteBatch.Draw(spriteSheet,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)),
                new Rectangle?(new Rectangle(num1 % 4 * 16, num1 / 4 * 16, 16, 16)),
                Color.White,
                0.0f,
                Vector2.Zero,
                (float)Game1.pixelZoom,
                SpriteEffects.None,
                1E-08f
            );
        }

        public Dictionary<string, string> Save()
        {
            throw new NotImplementedException();
        }

        public void Load(Dictionary<string, string> data)
        {
            throw new NotImplementedException();
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
