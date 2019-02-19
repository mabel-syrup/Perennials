using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using _SyrupFramework;
using Netcode;

namespace Perennials
{
    public class CompostBin : StardewValley.Object, IModdedItem
    {


        public CompostBin() { }

        public CompostBin(Vector2 tileLocation) : base (tileLocation, -1)
        {
            Name = "Compost Bin";
            price.Value = 120;
            category.Value = BigCraftableCategory;
            this.Type = "Crafting";
        }

        public override string getDescription()
        {
            return "A crate in which organic matter will break down into nutrient-rich soil.";
        }

        public override bool isPassable()
        {
            return false;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            if (t == null || !t.isHeavyHitter() || t is MeleeWeapon)
                return base.performToolAction(t, location);
            if (this.heldObject.Value != null)
                Game1.createItemDebris((Item)this.heldObject.Value, this.tileLocation.Value * 64f, -1, (GameLocation)null, -1);
            location.playSound("woodWhack");
            if (this.heldObject.Value == null)
                return true;
            this.heldObject.Value = (StardewValley.Object)null;
            this.readyForHarvest.Value = false;
            this.minutesUntilReady.Value = -1;
            return false;
        }

        public override bool performObjectDropInAction(Item dropIn, bool probe, Farmer who)
        {
            if (!(dropIn is StardewValley.Object))
                return false;
            if (dropIn != null && dropIn is StardewValley.Object && (bool)((NetFieldBase<bool, NetBool>)(dropIn as StardewValley.Object).bigCraftable) || this.heldObject.Value != null)
                return false;
            if(!probe)
                Logger.Log("Checking category of drop-in...");
            int dropInCat = dropIn.Category % -200;
            if(!probe)
                Logger.Log("Category of drop-in is " + dropInCat + ", fruits is" + FruitsCategory + ", vegetables is " + VegetableCategory);
            if (dropInCat == FruitsCategory || dropInCat == VegetableCategory)
            {
                if(!probe)
                    Logger.Log("Drop-in was organic...");
                heldObject.Value = new Fertilizer("Compost");
                if (!probe)
                {
                    Logger.Log("Processing " + dropIn.Name + " into " + heldObject.Value.Name);
                    minutesUntilReady.Value = 20;
                    who.currentLocation.playSound("Ship");
                    heldObject.Value.Stack = 5;
                    PerennialsGlobal.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[1]
                    {
                        new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, this.tileLocation.Value * 64f + new Vector2(0.0f, (float) sbyte.MinValue), false, false, (float) (((double) this.tileLocation.Y + 1.0) * 64.0 / 10000.0 + 9.99999974737875E-05), 0.0f, Color.LightBlue * 0.75f, 1f, 0.0f, 0.0f, 0.0f, false)
                        {
                            alphaFade = 0.005f
                        }
                    });
                }
                return true;
            }
            return false;
        }



        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            Vector2 vector2 = this.getScale() * 4f;
            Vector2 local = Game1.GlobalToLocal(
                Game1.viewport,
                new Vector2((float)(x * 64), (float)(y * 64 - 64))
            );
            Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle(
                (int)((double)local.X - (double)vector2.X / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0),
                (int)((double)local.Y - (double)vector2.Y / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0),
                (int)(64.0 + (double)vector2.X), (int)(128.0 + (double)vector2.Y / 2.0)
            );
            spriteBatch.Draw(
                PerennialsGlobal.objectSpriteSheet,
                destinationRectangle,
                new Rectangle(16, 0, 16, 32),
                Color.White * alpha,
                0.0f,
                Vector2.Zero,
                SpriteEffects.None,
                (float)((double)Math.Max(0.0f, (float)((y + 1) * 64 - 24) / 10000f) + ((int)((NetFieldBase<int, NetInt>)this.parentSheetIndex) == 105 ? 0.00350000010803342 : 0.0) + (double)x * 9.99999974737875E-06)
            );
            if (!(bool)((NetFieldBase<bool, NetBool>)this.readyForHarvest))
                return;
            float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2));
            spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 - 8), (float)(y * 64 - 96 - 16) + num)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((double)((y + 1) * 64) / 10000.0 + 9.99999997475243E-07 + (double)this.tileLocation.X / 10000.0 + ((int)((NetFieldBase<int, NetInt>)this.parentSheetIndex) == 105 ? 0.00150000001303852 : 0.0)));
            if (this.heldObject.Value == null)
                return;
            if (heldObject.Value is IModdedItem)
            {
                heldObject.Value.drawInMenu(
                    spriteBatch,
                    Game1.GlobalToLocal(
                        Game1.viewport, new Vector2((float)(x * 64),
                        (float)(y * 64 - 64 - 40) + num)
                    ),
                    1f,
                    0.75f,
                    (float)((double)((y + 1) * 64) / 10000.0 + 9.99999974737875E-06 + (double)this.tileLocation.X / 10000.0)
                );
            }
            else
            {
                spriteBatch.Draw(
                    Game1.objectSpriteSheet,
                    Game1.GlobalToLocal(
                        Game1.viewport,
                        new Vector2((float)(x * 64 + 32), (float)(y * 64 - 64 - 8) + num)
                    ),
                    new Microsoft.Xna.Framework.Rectangle?(
                        Game1.getSourceRectForStandardTileSheet(
                            Game1.objectSpriteSheet,
                            (int)((NetFieldBase<int, NetInt>)this.heldObject.Value.parentSheetIndex),
                            16,
                            16
                        )
                    ),
                    Color.White * 0.75f,
                    0.0f,
                    new Vector2(8f, 8f),
                    4f,
                    SpriteEffects.None,
                    (float)((double)((y + 1) * 64) / 10000.0 + 9.99999974737875E-06 + (double)this.tileLocation.X / 10000.0 + ((int)((NetFieldBase<int, NetInt>)this.parentSheetIndex) == 105 ? 0.00150000001303852 : 0.0))
                );
            }



            //spriteBatch.Draw(
            //    PerennialsGlobal.objectSpriteSheet,
            //    Game1.GlobalToLocal(Game1.viewport, new Vector2((x * 64f), (y * 64f - 64f))),
            //    new Rectangle(16, 0, 16, 32),
            //    Color.White,
            //    0.0f,
            //    new Vector2(0, 0),
            //    (float)Game1.pixelZoom,
            //    SpriteEffects.None,
            //    ((y + 0.5f) * 64) / 10000f
            //);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(
                PerennialsGlobal.objectSpriteSheet,
                location + new Vector2(32f, 32f),
                new Rectangle(16, 0, 16, 32),
                color * transparency,
                0.0f,
                new Vector2(8f, 16f),
                (float)(4.0 * ((double)scaleSize < 0.2 ? (double)scaleSize : (double)scaleSize / 2.0)),
                SpriteEffects.None,
                layerDepth
            );
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            spriteBatch.Draw(
                PerennialsGlobal.objectSpriteSheet,
                objectPosition,
                new Rectangle(16, 0, 16, 32),
                Color.White,
                0.0f,
                Vector2.Zero,
                4f,
                SpriteEffects.None,
                Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f)
            );
        }

        public override Item getOne()
        {
            return new CompostBin(Vector2.Zero);
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
