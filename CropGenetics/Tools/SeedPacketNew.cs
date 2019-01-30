using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using Netcode;
using _SyrupFramework;

namespace Perennials
{
    public class SeedPacketNew : Stackable, IModdedItem
    {

        string crop;

        public static Dictionary<string, string> seeds;
        public static Texture2D parentSheet;
        private int numberInStack;

        public new int NumberInStack
        {
            get
            {
                return this.numberInStack;
            }
            set
            {
                this.numberInStack = value;
            }
        }

        private string categoryName;
        private int price;

        public SeedPacketNew() { }

        public SeedPacketNew(string which, int count=1) : base("Seed Packet", 0, 0, 0, true)
        {
            init(which, count);
        }

        private void init(string which, int count = 1)
        {
            crop = which;
            if (seeds.ContainsKey(which))
            {
                Dictionary<string, string> seedData = getPacketFromXML(seeds[which]);
                if (seedData is null)
                {
                    Logger.Log("Could not create a seed packet for the crop '" + which + "'!");
                    throw new KeyNotFoundException("The Seeds.xml file did not contain a valid definition for the crop name.  Please contact the mod author if this issue persists.");
                }
                Name = seedData["name"];
                ParentSheetIndex = Convert.ToInt32(seedData["parentSheetIndex"]);
                price = Convert.ToInt32(seedData["price"]);
                Category = Convert.ToInt32(seedData["categoryID"]);
                categoryName = seedData["categoryName"];
                displayName = seedData["displayName"];
                description = seedData["description"];
                numberInStack = count;
                Logger.Log("Initialized " + Name + " x" + numberInStack);
            }
            else
            {
                throw new KeyNotFoundException("The Seeds.xml file did not contain a valid definition for the crop name.  Please contact the mod author if this issue persists.");
            }
        }

        public Dictionary<string, string> getPacketFromXML(string data)
        {
            Dictionary<string, string> seedPacketData = new Dictionary<string, string>();
            string[] substrings = data.Split('/');
            try
            {
                seedPacketData["name"] = substrings[0];
                seedPacketData["parentSheetIndex"] = substrings[1];
                seedPacketData["price"] = substrings[2];
                string[] categoryData = substrings[3].Split(' ');
                seedPacketData["categoryName"] = categoryData[0];
                seedPacketData["categoryID"] = categoryData[1];
                seedPacketData["displayName"] = substrings[4];
                seedPacketData["description"] = substrings[5];
                Logger.Log("Parsed " + seedPacketData["name"] + " successfully.");
                return seedPacketData;
            }
            catch (IndexOutOfRangeException)
            {
                Logger.Log("Seed packet data is not in correct format!");
                return null;
            }
        }

        public override int addToStack(int amount)
        {
            Logger.Log("Calling addToStack in seedpacket...");
            return base.addToStack(amount);
        }

        public override int getStack()
        {
            Logger.Log("Getting seedpacket stack...");
            return base.getStack();
        }

        public override Item getOne()
        {
            Logger.Log("Calling SeedPacketNew.getOne()");
            if(crop is null)
                return new SeedPacketNew();
            return new SeedPacketNew(crop, 1);
        }

        protected override string loadDisplayName()
        {
            return "...";
        }

        protected override string loadDescription()
        {
            SpriteFont smallFont = Game1.smallFont;
            int width = Math.Max(Game1.tileSize * 4 + Game1.tileSize / 4, (int)Game1.dialogueFont.MeasureString(this.DisplayName).X);
            return Game1.parseText(description, smallFont, width);
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            Vector2 tile = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            List<Vector2> vector2List = this.tilesAffected(tile, power, who);
            foreach (Vector2 index in vector2List)
            {
                if (location.terrainFeatures.ContainsKey(index) && location.terrainFeatures[index] is CropSoil)
                {
                    Logger.Log("Seeds used on cropsoil...");
                    CropSoil soil = (CropSoil)location.terrainFeatures[index];
                    bool success = soil.plant(crop, who);
                    Logger.Log(crop + " was " + (success ? "" : "un") + "successfully planted @ " + tile.ToString());
                    if (success)
                    {
                        who.Stamina = who.Stamina - (float)(2.0 - (double)who.FarmingLevel * 0.100000001490116);
                        this.numberInStack = this.numberInStack - 1;
                        location.playSound("seeds");
                    }
                }
                else
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13021"));
                }
            }
        }

        //public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        //{
        //    spriteBatch.Draw(parentSheet, location + new Vector2((float)(int)((double)(Game1.tileSize / 2) * (double)scaleSize), (float)(int)((double)(Game1.tileSize / 2) * (double)scaleSize)), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(parentSheet, this.parentSheetIndex, 16, 16)), Color.White * transparency, 0.0f, new Vector2(8f, 8f) * scaleSize, (float)Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);
        //    if (drawStackNumber && this.maximumStackSize() > 1 && ((double)scaleSize > 0.3 && this.numberInStack != int.MaxValue) && this.numberInStack > 1)
        //        Utility.drawTinyDigits(this.numberInStack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.numberInStack, 3f * scaleSize)) + 3f * scaleSize, (float)((double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
        //}

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(parentSheet, location + new Vector2(32f, 32f), new Rectangle?(Game1.getSourceRectForStandardTileSheet(parentSheet, this.parentSheetIndex, 16, 16)), color * transparency, 0.0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
            Game1.drawWithBorder(string.Concat((object)((StardewValley.Tools.Stackable)this).NumberInStack), Color.Black, Color.White, location + new Vector2(64f - Game1.dialogueFont.MeasureString(string.Concat((object)((StardewValley.Tools.Stackable)this).NumberInStack)).X, (float)(64.0 - (double)Game1.dialogueFont.MeasureString(string.Concat((object)((StardewValley.Tools.Stackable)this).NumberInStack)).Y * 3.0 / 4.0)), 0.0f, 0.5f, 1f);
        }

        public void Load(Dictionary<string, string> data)
        {
            string which = data["which"];
            int count = Convert.ToInt32(data["stack"]);
            init(which, count);
        }

        public Dictionary<string, string> Save()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["which"] = crop;
            data["stack"] = numberInStack.ToString();
            return data;
        }

        public override int salePrice()
        {
            return price;
        }
    }
}
