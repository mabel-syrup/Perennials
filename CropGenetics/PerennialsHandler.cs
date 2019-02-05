using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using Microsoft.Xna.Framework;
using _SyrupFramework;

namespace Perennials
{
    public class PerennialsHandler : IModHandler
    {
        public void AfterModdedLoad(object sender, EventArgs e)
        {
            PerennialsGlobal.processWeeds();
            return;
        }

        public void BeforeModdedSave(object sender, EventArgs e)
        {
            foreach(GameLocation location in Game1.locations)
            {
                PerennialsGlobal.equalizeDitches(location);
                List<Vector2> sprawlTiles = new List<Vector2>();
                foreach(Vector2 tile in location.objects.Keys)
                {
                    if(location.objects[tile] is Sprawl)
                    {
                        sprawlTiles.Add(tile);
                    }
                }
                foreach(Vector2 tile in sprawlTiles)
                {
                    location.objects[tile] = null;
                }
            }
            return;
        }
    }
}
