using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _SyrupFramework;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Perennials
{
    public class Fruit : StardewValley.Object, IModdedItem
    {
        //Yes, I do know that not all the produce is fruits.
        //It's just easier to simplify it.  Please be nice to me.

        public static Dictionary<string, string> fruits;
        public static Texture2D fruitSheet;

        public string description;
        public int row;
        public int healthValue;
        public int energyValue;
        public string qualityPrefix;


        public string fruit;

        public Fruit() { }

        public Fruit(string which, int count = 1, int in_quality = 0) : base(-1, count)
        {
            initFruit(which, count, in_quality);
        }

        public void initFruit(string which, int count = 1, int quality = 0)
        {
            fruit = which;
            Quality = quality;
            //Logger.Log("Loaded a " + fruit + ", of quality " + quality);
            if (fruits.ContainsKey(which))
            {
                Dictionary<string, string> fruitData = getFruitFromXML(fruits[which]);
                if (fruitData is null)
                {
                    Logger.Log("Could not create a fruit item for the crop '" + which + "'!");
                    throw new KeyNotFoundException("The Fruits.xml file did not contain a valid definition for the crop name.  Please contact the mod author if this issue persists.");
                }
                name = which;
                DisplayName = fruitData["displayName"];
                row = Convert.ToInt32(fruitData["row"]);
                ParentSheetIndex = Convert.ToInt32(fruitData["parentSheetIndex"]);
                //edibility = Convert.ToInt32(Game1.objectInformation[parentSheetIndex][objectInfoEdibilityIndex]);
                //Global.Log("Edibility of " + name + " is " + edibility);
                string[] prices = fruitData["price"].Split(' ');
                if (prices.Length < 4)
                {
                    throw new IndexOutOfRangeException("'" + name + "' within Fruits.xml has its prices field improperly formatted! Please contact the mod author if this issue persists.");
                }
                int correctedQuality = quality < 4 ? quality : 3;
                float qualityFactor = 1f;
                switch (correctedQuality)
                {
                    case 1:
                        qualityFactor = 1.25f;
                        qualityPrefix = "Artisinal ";
                        break;
                    case 2:
                        qualityFactor = 1.5f;
                        qualityPrefix = "High-end ";
                        break;
                    case 3:
                        qualityFactor = 2f;
                        qualityPrefix = "Masterful ";
                        break;
                    default:
                        qualityFactor = 1f;
                        qualityPrefix = "";
                        break;
                }
                Price = (int)Math.Ceiling(Convert.ToInt32(prices[correctedQuality]) / qualityFactor);
                //Category = -75;
                //Category = FruitsCategory;
                Category = FruitsCategory;
                description = fruitData["description"];
                string[] healthValues = fruitData["health"].Split(' ');
                if (healthValues.Length < 4 && healthValues.Length > 1)
                {
                    throw new IndexOutOfRangeException("'" + name + "' within Fruits.xml has its health field improperly formatted! Please contact the mod author if this issue persists.");
                }
                if (healthValues.Length < 2)
                    healthValue = 0;
                else
                    healthValue = Convert.ToInt32(healthValues[correctedQuality]);
                bool isEdible = Convert.ToBoolean(fruitData["edible"]);
                if (isEdible)
                    Edibility = (int)(healthValue * 0.9);
                else
                    Edibility = -300;
                string[] energyValues = fruitData["energy"].Split(' ');
                if (energyValues.Length < 4 && energyValues.Length > 1)
                {
                    throw new IndexOutOfRangeException("'" + name + "' within Fruits.xml has its energy field improperly formatted! Please contact the mod author if this issue persists.");
                }
                if (energyValues.Length < 2)
                    energyValue = 0;
                else
                    energyValue = Convert.ToInt32(energyValues[correctedQuality]);
            }
            else
            {
                throw new KeyNotFoundException("The Fruits.xml file did not contain a valid definition for the crop name.  Please contact the mod author if this issue persists.");
            }
            //Logger.Log("Sprite index for this " + name + " is " + ((row * 4) + (quality < 4 ? quality : 3)));
            //Logger.Log("Fruit sprite sheet is " + fruitSheet.Width / 16 + " tiles wide by " + fruitSheet.Height / 16 + " tiles tall.");
        }

        public override void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
        {
            return;
        }

        public override string getCategoryName()
        {
            return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12854");
            //return base.getCategoryName();
        }

        public override Color getCategoryColor()
        {
            return Color.DeepPink;
        }

        public Dictionary<string, string> getFruitFromXML(string data)
        {
            Dictionary<string, string> fruitData = new Dictionary<string, string>();
            string[] substrings = data.Split('/');
            try
            {
                fruitData["row"] = substrings[0];
                fruitData["parentSheetIndex"] = substrings[1];
                fruitData["price"] = substrings[2];
                fruitData["displayName"] = substrings[3];
                fruitData["description"] = substrings[4];
                fruitData["edible"] = substrings[5];
                fruitData["energy"] = substrings[6];
                fruitData["health"] = substrings[7];
                //Logger.Log("Parsed " + fruitData["displayName"] + " successfully.");
                return fruitData;
            }
            catch (IndexOutOfRangeException)
            {
                Logger.Log("Seed packet data is not in correct format!");
                return null;
            }
        }

        public override bool isPlaceable()
        {
            return false;
            //Logger.Log("Checking if fruit is placeable...");
            //return (Game1.currentLocation.objects.ContainsKey(Game1.player.GetGrabTile()));
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            return false;
            //Logger.Log("Checking if fruit can be placed here...");
            //return (l.objects.ContainsKey(tile));
        }

        public void overrideMachineResult(StardewValley.Object machine)
        {
            if (!machine.name.Equals("Keg") && !machine.name.Equals("PreservesJar"))
                return;

        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Logger.Log("Attempting to place fruit into machine...");
            if (who == null)
                who = Game1.player;
            Vector2 tileLocation = new Vector2((float)(x / 64), (float)(y / 64));
            if (location.objects.ContainsKey(tileLocation))
            {
                StardewValley.Object machine = location.objects[tileLocation];
                StardewValley.Object tempFruit = (StardewValley.Object)this.getOne();
                tempFruit.Category = FruitsCategory;
                if (!machine.name.Equals("Keg") && !machine.name.Equals("PreservesJar"))
                    return false;
                if(machine.performObjectDropInAction(tempFruit, false, who))
                {
                    Logger.Log("Placed " + fruit + " into the machine, expecting " + qualityPrefix + fruit + " Wine out.");
                    machine.heldObject.Value.name = qualityPrefix + fruit + " Wine";
                    machine.heldObject.Value.preserve.Value = new PreserveType?(PreserveType.Wine);
                    return true;
                }
                else
                {
                    Logger.Log("Machine could not accept this fruit!");
                }
            }
            else
            {
                Logger.Log("Fruit did not find a machine to be placed in!");
            }
            return false;
        }

        public override string getDescription()
        {
            SpriteFont smallFont = Game1.smallFont;
            int width = Math.Max(Game1.tileSize * 4 + Game1.tileSize / 4, (int)Game1.dialogueFont.MeasureString(this.DisplayName).X);
            return Game1.parseText(description, smallFont, width);
        }

        public Rectangle getSprite()
        {
            return Game1.getSourceRectForStandardTileSheet(fruitSheet, (row * 4) + (quality < 4 ? quality : 3), 16, 16);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            //draw the shadow thingy
            spriteBatch.Draw(Game1.shadowTexture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize * 3 / 4)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * 0.5f, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            //draw the fruit (overridden to have unique quality sprites!)
            spriteBatch.Draw(fruitSheet, location + new Vector2((float)(int)((double)(Game1.tileSize / 2) * (double)scaleSize), (float)(int)((double)(Game1.tileSize / 2) * (double)scaleSize)), getSprite(), Color.White * transparency, 0.0f, new Vector2(8f, 8f) * scaleSize, (float)Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);
            //stack number
            if (drawStackNumber && this.maximumStackSize() > 1 && ((double)scaleSize > 0.3 && this.Stack != int.MaxValue) && this.Stack > 1)
                Utility.drawTinyDigits(this.stack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.stack, 3f * scaleSize)) + 3f * scaleSize, (float)((double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
            //quality star
            if (drawStackNumber && this.quality > 0)
            {
                float num = this.quality < 4 ? 0.0f : (float)((Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
                spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, (float)(Game1.tileSize - 12) + num), new Microsoft.Xna.Framework.Rectangle?(this.quality < 4 ? new Microsoft.Xna.Framework.Rectangle(338 + (this.quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8)), Color.White * transparency, 0.0f, new Vector2(4f, 4f), (float)(3.0 * (double)scaleSize * (1.0 + (double)num)), SpriteEffects.None, layerDepth);
            }
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            spriteBatch.Draw(
                fruitSheet,
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

        public override Item getOne()
        {
            if(fruit == null || fruit == "")
                return new Fruit();
            return new Fruit(fruit, 1, quality);
        }

        public void Load(Dictionary<string, string> data)
        {
            //string which = data["which"];
            //int quality = Convert.ToInt32(data["quality"]);
            //int count = Convert.ToInt32(data["count"]);
            initFruit(data["which"], stack, Convert.ToInt32(data["quality"]));
        }

        public Dictionary<string, string> Save()
        {
            CustomData saveData = new CustomData();
            saveData.add("which", fruit);
            saveData.add("quality", quality);
            return saveData.build();
        }
    }
}
