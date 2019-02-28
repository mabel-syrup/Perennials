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
            foreach(GameLocation location in Game1.locations)
            {
                foreach(Vector2 tileLocation in location.terrainFeatures.Keys)
                {
                    if (!(location.terrainFeatures[tileLocation] is CropSoil) || (location.terrainFeatures[tileLocation] as CropSoil).crop == null || !((location.terrainFeatures[tileLocation] as CropSoil).crop is CropSprawler))
                        return;
                    CropSprawler sprawler = (CropSprawler)(location.terrainFeatures[tileLocation] as CropSoil).crop;
                    foreach(Sprawl sprawlTile in sprawler.sprawlTiles)
                    {
                        location.objects[sprawlTile.tileLocation] = sprawlTile;
                    }
                }
            }
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
                    location.objects.Remove(tile);
                }
            }
            return;
        }
    }
}
