using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using _SyrupFramework;

namespace Perennials
{
    public class MagnifyingGlass : StardewValley.Tool, IModdedItem
    {
        public MagnifyingGlass () : base("Magnifying Glass", 0, -1, -1, false)
        {

        }

        public override Item getOne()
        {
            return new MagnifyingGlass();
        }

        public void Load(Dictionary<string, string> data)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> Save()
        {
            throw new NotImplementedException();
        }

        protected override string loadDescription()
        {
            throw new NotImplementedException();
        }

        protected override string loadDisplayName()
        {
            throw new NotImplementedException();
        }
    }
}
