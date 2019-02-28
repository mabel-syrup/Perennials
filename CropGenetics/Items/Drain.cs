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

namespace Perennials
{
    public class Drain : StardewValley.Object, IModdedItem, Irrigator
    {
        public Drain() { }

        public Drain(Vector2 tileLocation, int stack = 1) : base(tileLocation, 322, "Drain", true, true, false, false)
        {
            name = "Drain";
            this.stack.Value = stack;
            if (tileLocation != Vector2.Zero)
            {
                Logger.Log("Placed a drain at (" + tileLocation.X + ", " + tileLocation.Y + ")");
            }
            category.Value = BigCraftableCategory;
        }

        public int waterAmount()
        {
            return -1000;
        }

        public override string getDescription()
        {
            return "A drain which prevents a ditch from flooding.  Place in an empty ditch.";
        }

        public override bool canBePlacedHere(GameLocation location, Vector2 index1)
        {
            //Logger.Log("Checking placement rules for drain...");
            if (location.objects.ContainsKey(index1))
            {
                //Logger.Log("Drain cannot be placed on an object.");
                return false;
            }
            if (location.terrainFeatures.ContainsKey(index1) && location.terrainFeatures[index1] is CropSoil)
            {
                CropSoil soil = location.terrainFeatures[index1] as CropSoil;
                if (soil.height != CropSoil.Lowered || soil.crop != null)
                {
                    //Logger.Log("Attempted to place drain on a cropsoil that cannot accept it.");
                    return false;
                }
                return true;
            }
            return false;
        }

        public override bool canBePlacedInWater()
        {
            return true;
        }

        public override bool isPassable()
        {
            return true;
        }

        public override bool isPlaceable()
        {
            return true;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            if ((t != null && t.isHeavyHitter() && !(t is MeleeWeapon)) || t is Pickaxe)
            {
                Farmer who = t.getLastFarmerToUse();
                dropItem(location, who.GetToolLocation(false), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y));
                location.playSound("hammer");
                location.objects.Remove(tileLocation);
                PerennialsGlobal.equalizeDitches(location);
                return false;
            }
            return false;
        }

        public override void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
        {
            Logger.Log("Dropping drain...");
            location.debris.Add(new Debris(new Drain(Vector2.Zero), origin, destination));
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Logger.Log("Attempting to place drain...");
            Vector2 index1 = new Vector2((float)(x / 64), (float)(y / 64));
            this.health = 10;
            if (who != null)
                this.owner.Value = who.UniqueMultiplayerID;
            else
                this.owner.Value = Game1.player.UniqueMultiplayerID;
            if (location.objects.ContainsKey(index1))
            {
                Logger.Log("Drain cannot be placed on an object.");
                return false;
            }
            if (location.terrainFeatures.ContainsKey(index1) && location.terrainFeatures[index1] is CropSoil)
            {
                CropSoil soil = location.terrainFeatures[index1] as CropSoil;
                if (soil.height != CropSoil.Lowered || soil.crop != null)
                {
                    Logger.Log("Attempted to place drain on a cropsoil that cannot accept it.");
                    return false;
                }
                Logger.Log("Placing drain at (" + index1.X + ", " + index1.Y + ")");
                location.objects.Add(index1, new Drain(index1));
                location.playSound("hammer");
                PerennialsGlobal.equalizeDitches(Game1.currentLocation);
                return true;
            }
            Logger.Log(location.name + " did not have a cropsoil here.");
            return false;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            spriteBatch.Draw(
                PerennialsGlobal.objectSpriteSheet,
                Game1.GlobalToLocal(Game1.viewport, new Vector2((x * 64f), (y * 64f - 64f))),
                new Rectangle(32, 0, 16, 32),
                Color.White,
                0.0f,
                new Vector2(0, 0),
                (float)Game1.pixelZoom,
                SpriteEffects.None,
                ((y + 0.5f) * 64) / 10000f
            );
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(
                PerennialsGlobal.objectSpriteSheet,
                location + new Vector2(32f, 32f),
                new Rectangle(48, 0, 16, 32),
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
                new Rectangle(48, 0, 16, 32),
                Color.White,
                0.0f,
                new Vector2(0, 32),
                4f,
                SpriteEffects.None,
                Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f)
            );
        }
        
        public override Item getOne()
        {
            return new Drain(Vector2.Zero);
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
