using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.TerrainFeatures;
using _SyrupFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Perennials
{
    public class Sprawl : StardewValley.Object
    {
        private SoilCrop parent;
        public bool hasFruit;

        public Sprawl(Vector2 tileLocation, SoilCrop parentCrop) : base(tileLocation, -1, "sprawl", false, false, false, false)
        {
            parent = parentCrop;
        }

        public override bool isPassable()
        {
            return true;
        }

        public override bool performUseAction(GameLocation location)
        {
            if (parent.harvest(tileLocation))
            {
                hasFruit = false;
                return true;
            }
            return false;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            //These are invisible!
            return;
        }
    }
}
