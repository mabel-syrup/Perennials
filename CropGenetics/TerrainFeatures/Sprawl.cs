using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;
using _SyrupFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Perennials
{
    public class Sprawl : StardewValley.Object
    {
        private CropSprawler parent;
        public bool hasFruit;

        public Sprawl(Vector2 tileLocation, CropSprawler parentCrop) : base(tileLocation, -1, "sprawl", false, false, false, false)
        {
            parent = parentCrop;
        }

        public void setInteractive(bool interactive)
        {
            this.Type = interactive ? "interactive" : "";
        }

        public override bool isPassable()
        {
            return !(hasFruit && parent.mature);
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            if (t != null)
            {
                if (t.isHeavyHitter() && !(t is MeleeWeapon))
                    parent.destroySprawl(this);
                else if (t is MeleeWeapon && (t as MeleeWeapon).BaseName.Equals("Scythe"))
                {
                    hasFruit = false;
                    setInteractive(false);
                    Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X, tileLocation.Y) * (float)Game1.tileSize, Color.White, 8, false, 100f, 0, -1, -1f, -1, 0));
                }
            }
            return base.performToolAction(t, location);
        }

        public override bool performUseAction(GameLocation location)
        {
            Logger.Log("Performing use action on sprawl...");
            return parent.harvestFruit(this);
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
                return true;
            if (!hasFruit)
            {
                Logger.Log("Sprawl was checked and did not have fruit.");
                return true;
            }
            Logger.Log("Performing action check on sprawl...");
            bool harvested = parent.harvestFruit(this);
            Logger.Log("Sprawl's fruit was " + (harvested ? "" : "not ") + "harvested.");
            return harvested;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            //These are invisible!
            return;
        }
    }
}
