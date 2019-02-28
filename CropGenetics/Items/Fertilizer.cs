using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using _SyrupFramework;
using Netcode;

namespace Perennials
{
    public class Fertilizer : StardewValley.Object, IModdedItem
    {
        public int n;
        public int p;
        public int k;
        public static Dictionary<string, string> fertilizers;
        public static Texture2D fertilizerSheet;
        private string which;
        public string description;

        public Fertilizer() { }

        public Fertilizer(string which, int stack=1) : base(-1, stack)
        {
            init(which, stack);
        }

        private void init(string which, int count)
        {
            //wood ash, bone meal, compost
            //potash, manure, coffee grounds
            this.Stack = count;
            this.stack.Value = count;
            this.which = which;
            if (fertilizers.ContainsKey(which))
            {
                Dictionary<string, string> fertilizerData = readFromXNB(fertilizers[which]);
                if (fertilizerData is null)
                {
                    Logger.Log("Could not create a fertilizer of the type '" + which + "'!");
                    throw new KeyNotFoundException("The Fertilizers.xml file did not contain a valid definition for the crop name.");
                }
                name = which;
                ParentSheetIndex = Convert.ToInt32(fertilizerData["parentSheetIndex"]);
                n = Convert.ToInt32(fertilizerData["nitrogen"]);
                p = Convert.ToInt32(fertilizerData["phosphorous"]);
                k = Convert.ToInt32(fertilizerData["potassium"]);
                Price = 160;
                //Category = Convert.ToInt32(seedData["categoryID"]);
                Category = fertilizerCategory;
                displayName = which;
                description = fertilizerData["description"];
                //Logger.Log("Created " + name + " successfully.  Stack size is " + this.Stack + ", " + this.stack.Value + ", " + count);
            }
            else
            {
                throw new KeyNotFoundException("The Fertilizers.xml file did not contain a valid definition for the name '" + which + "'.");
            }
        }

        public override string getDescription()
        {
            return description;
        }

        public Dictionary<string, string> readFromXNB(string data)
        {
            Dictionary<string, string> fertilizerData = new Dictionary<string, string>();
            string[] substrings = data.Split('/');
            try
            {
                fertilizerData["parentSheetIndex"] = substrings[0];
                fertilizerData["nitrogen"] = substrings[1];
                fertilizerData["phosphorous"] = substrings[2];
                fertilizerData["potassium"] = substrings[3];
                fertilizerData["description"] = substrings[4];
                //Logger.Log("Parsed " + which + " successfully.");
                return fertilizerData;
            }
            catch (IndexOutOfRangeException)
            {
                Logger.Log("Fertilizer data is not in correct format!  Was given " + data);
                return null;
            }
        }

        public Rectangle getSprite()
        {
            return new Rectangle(ParentSheetIndex * 16, 0, 16, 16);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(
                fertilizerSheet,
                location + new Vector2((float)(int)((double)(Game1.tileSize / 2) * (double)scaleSize),
                (float)(int)((double)(Game1.tileSize / 2) * (double)scaleSize)),
                getSprite(),  //new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(parentSheet, this.parentSheetIndex, 16, 16)),
                Color.White * transparency,
                0.0f,
                new Vector2(8f, 8f) * scaleSize,
                (float)Game1.pixelZoom * scaleSize,
                SpriteEffects.None,
                layerDepth
            );
            //if (drawStackNumber && this.maximumStackSize() > 1 && ((double)scaleSize > 0.3 && this.Stack != int.MaxValue) && this.Stack > 1)
            //    Utility.drawTinyDigits(this.stack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.stack, 3f * scaleSize)) + 3f * scaleSize, (float)((double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
            if (drawStackNumber && this.maximumStackSize() > 1 && ((double)scaleSize > 0.3 && this.Stack != int.MaxValue) && this.Stack > 1)
                Utility.drawTinyDigits((int)((NetFieldBase<int, NetInt>)this.stack), spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString((int)((NetFieldBase<int, NetInt>)this.stack), 3f * scaleSize)) + 3f * scaleSize, (float)(64.0 - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, color);

        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            spriteBatch.Draw(
                fertilizerSheet,
                objectPosition,
                getSprite(),
                Color.White,
                0.0f,
                new Vector2(0, 0),
                Game1.pixelZoom,
                SpriteEffects.None,
                Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f)
            );
        }

        public override bool isPlaceable()
        {
            return (Game1.currentLocation is Farm || (Game1.currentLocation.name != null && Game1.currentLocation.name.Equals("Greenhouse")));
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            if(l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is CropSoil)
            {
                CropSoil soil = l.terrainFeatures[tile] as CropSoil;
                return soil.n + n < 3 && soil.p + p < 3 && soil.k + k < 3;
            }
            return false;
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Logger.Log("Attempting to place fertilizer onto soil...");
            if (who == null)
                who = Game1.player;
            Vector2 tileLocation = new Vector2((float)(x / 64), (float)(y / 64));
            if(location.terrainFeatures.ContainsKey(tileLocation) && location.terrainFeatures[tileLocation] is CropSoil)
            {
                CropSoil soil = location.terrainFeatures[tileLocation] as CropSoil;
                soil.n = Math.Min(2, soil.n + n);
                soil.p = Math.Min(2, soil.p + p);
                soil.k = Math.Min(2, soil.k + k);
                who.reduceActiveItemByOne();
                Logger.Log("Soil's npk value now: " + soil.n + " " + soil.p + " " + soil.k);
            }
            return false;
        }

        public override Item getOne()
        {
            if (which != null && which != "")
                return new Fertilizer(which);
            return new Fertilizer();
        }

        public void Load(Dictionary<string, string> data)
        {
            string which = data["which"];
            init(which, Stack);
        }

        public Dictionary<string, string> Save()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            if (which != null)
                data["which"] = which;
            else
                data["which"] = "";
            return data;
        }
    }
}
