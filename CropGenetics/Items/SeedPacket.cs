using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using _SyrupFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace Perennials
{
    class SeedPacket : StardewValley.Object, IModdedItem
    {

        string crop;

        public static Dictionary<string, string> seeds;
        public static Texture2D parentSheet;

        private string description;
        private string categoryName;

        public SeedPacket() : base(Vector2.Zero, 322, "Seed Packet", true, true, false, false)
        {

        }

        public SeedPacket(string which, int count = 1) : base(Vector2.Zero, 322, "Seed Packet", true, true, false, false)
        {
            init(which, count);
        }

        public SeedPacket(Vector2 tileLocation, string which, int count=1) : base(tileLocation, 322, "Seed Packet", true, true, false, false)
        {
            init(which, count);
        }

        public void init(string which, int count = 1)
        {
            this.Stack = count;
            this.stack.Value = count;
            crop = which;
            if (seeds.ContainsKey(which))
            {
                Dictionary<string, string> seedData = getPacketFromXNB(seeds[which]);
                if (seedData is null)
                {
                    Logger.Log("Could not create a seed packet for the crop '" + which + "'!");
                    throw new KeyNotFoundException("The Seeds.xml file did not contain a valid definition for the crop name.  Please contact the mod author if this issue persists.");
                }
                name = seedData["name"];
                ParentSheetIndex = Convert.ToInt32(seedData["parentSheetIndex"]);
                Price = Convert.ToInt32(seedData["price"]);
                //Category = Convert.ToInt32(seedData["categoryID"]);
                Category = CraftingCategory;
                categoryName = seedData["categoryName"];
                displayName = seedData["displayName"];
                description = seedData["description"];
                Logger.Log("Created " + name + " successfully.  Stack size is " + this.Stack + ", " + this.stack.Value + ", " + count);
            }
            else
            {
                throw new KeyNotFoundException("The Seeds.xml file did not contain a valid definition for the crop name.  Please contact the mod author if this issue persists.");
            }
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            if(l is Farm || (l.Name != null && l.name.Equals("Greenhouse"))){
                if (l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is CropSoil)
                {
                    //Logger.Log("Seeds can be sown here.");
                    return true;
                }
            }
            return false;
        }

        public override bool performUseAction(GameLocation location)
        {
            //Logger.Log("Performing seedpacket use action...");
            return base.performUseAction(location);
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Logger.Log("Attempting to perform placement action for seedpacket...");
            Vector2 tile = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            if (who != null)
                this.owner.Value = who.UniqueMultiplayerID;
            else
                this.owner.Value = Game1.player.UniqueMultiplayerID;
            if(location.terrainFeatures.ContainsKey(tile) && location.terrainFeatures[tile] is CropSoil && !location.objects.ContainsKey(tile))
            {
                CropSoil soil = (CropSoil)location.terrainFeatures[tile];
                bool success = soil.plant(crop, who);
                Logger.Log(crop + " was " + (success ? "" : "un") + "successfully planted @ " + tile.ToString());
                return success;
            }
            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13021"));
            return false;
        }

        public Dictionary<string,string> getPacketFromXNB(string data)
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

        public override string getDescription()
        {
            SpriteFont smallFont = Game1.smallFont;
            int width = Math.Max(Game1.tileSize * 4 + Game1.tileSize / 4, (int)Game1.dialogueFont.MeasureString(this.DisplayName).X);
            return Game1.parseText(description, smallFont, width);
        }

        public override string getCategoryName()
        {
            return categoryName;
        }

        public Rectangle getSprite()
        {
            int spriteX = parentSheetIndex % 8;
            int spriteY = ((int)parentSheetIndex / 8);
            return new Rectangle(spriteX * 16, spriteY * 16, 16, 16);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(
                parentSheet,
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

        //public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        //{
        //    spriteBatch.Draw(parentSheet, objectPosition, new Microsoft.Xna.Framework.Rectangle?(GameLocation.getSourceRectForObject(f.ActiveObject.ParentSheetIndex)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
        //}

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            spriteBatch.Draw(
                parentSheet,
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

        public override void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
        {
            int x = Game1.getOldMouseX() + Game1.viewport.X;
            int y = Game1.getOldMouseY() + Game1.viewport.Y;
            if ((double)Game1.mouseCursorTransparency == 0.0)
            {
                x = (int)Game1.player.GetGrabTile().X * 64;
                y = (int)Game1.player.GetGrabTile().Y * 64;
            }
            if (Game1.player.GetGrabTile().Equals(Game1.player.getTileLocation()) && (double)Game1.mouseCursorTransparency == 0.0)
            {
                Vector2 translatedVector2 = Utility.getTranslatedVector2(Game1.player.GetGrabTile(), Game1.player.FacingDirection, 1f);
                x = (int)translatedVector2.X * 64;
                y = (int)translatedVector2.Y * 64;
            }
            bool flag = Utility.playerCanPlaceItemHere(location, (Item)this, x, y, Game1.player);
            spriteBatch.Draw(Game1.mouseCursors, new Vector2((float)(x / 64 * 64 - Game1.viewport.X), (float)(y / 64 * 64 - Game1.viewport.Y)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(flag ? 194 : 210, 388, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
        }

        public override Item getOne()
        {
            if (crop is null)
                return new SeedPacket();
            return new SeedPacket(crop);
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
            data["stack"] = stack.ToString();
            return data;
        }
    }
}
