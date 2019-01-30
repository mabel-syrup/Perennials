using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
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
            }
            return;
        }
    }
}
