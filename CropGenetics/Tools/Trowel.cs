using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Dimensions;
using StardewValley;
using StardewValley.Tools;
using _SyrupFramework;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace Perennials
{
    public class Trowel : Tool, IModdedItem
    {
        public Trowel() : base("Trowel", 0, 7, 7, false, 0)
        {
            this.upgradeLevel.Value = 0;
            InstantUse = true;
        }

        protected override string loadDisplayName()
        {
            return "Trowel";
        }

        protected override string loadDescription()
        {
            return "Use this to remove weeds from tilled soil.";
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            power = who.toolPower;
            who.stopJittering();
            Vector2 vector2 = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            List<Vector2> vector2List = this.tilesAffected(vector2, power, who);
            foreach (Vector2 index in vector2List)
            {
                index.Equals(vector2);
                if (location.terrainFeatures.ContainsKey(index))
                {
                    TerrainFeature feature = location.terrainFeatures[index];
                    if (feature is CropSoil && (feature as CropSoil).weeds)
                    {
                        Logger.Log("Trowel used on cropsoil @" + index.ToString() + "!\nRemoving weeds @(" + (int)index.X + ", " + (int)index.Y + ")...");
                        ((CropSoil)feature).weeds = false;
                        Game1.playSound("hoeHit");
                    }
                    if(feature is CropSoil && (feature as CropSoil).height == CropSoil.Lowered)
                    {
                        IrrigationBridge bridge = new IrrigationBridge();
                        location.terrainFeatures[index] = bridge;
                    }
                }
                //else
                //{
                //    Tree tree = new WhiteOak();
                //    location.terrainFeatures[index] = tree;
                //}
            }
            Logger.Log("Used trowel");
            who.CanMove = true;
            who.UsingTool = false;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(
                PerennialsGlobal.toolSpriteSheet,
                location + new Vector2(32f, 32f),
                new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16),
                color * transparency,
                0.0f,
                new Vector2(8f, 8f),
                4f * scaleSize,
                SpriteEffects.None,
                layerDepth
            );
        }

        public override Item getOne()
        {
            return new Trowel();
        }

        public Dictionary<string, string> Save()
        {
            return new Dictionary<string, string>();
        }

        public void Load(Dictionary<string, string> data)
        {
            return;
        }
    }
}
