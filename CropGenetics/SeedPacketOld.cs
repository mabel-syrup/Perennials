using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Tools;
using StardewValley;
using _SyrupFramework;

namespace CropGenetics
{
    public class SeedPacketOld : Stackable, IModdedItem
    {
        private string seedType;
        private int numberInStack;

        public new int NumberInStack
        {
            get
            {
                return this.numberInStack;
            }
            set
            {
                this.numberInStack = value;
            }
        }

        public string SeedType
        {
            get
            {
                return this.seedType;
            }
            set
            {
                this.seedType = value;
            }
        }

        public SeedPacketOld()
        {
        }

        public SeedPacketOld(string seedType, int numberInStack) : base("Seeds", 0, 0, 0, true)
        {
            Logger.Log("Created seed packet of type '" + seedType + "', and a count of " + numberInStack);
            this.seedType = seedType;
            this.numberInStack = numberInStack;
            this.setCurrentTileIndexToSeedType();
            this.indexOfMenuItemView.Value = this.CurrentParentTileIndex;
            Logger.Log("Seed packet uses index " + CurrentParentTileIndex + " and is a stack of " + this.numberInStack);
        }

        public override Item getOne()
        {
            return new SeedPacketOld(seedType, 1);
        }

        protected override string loadDisplayName()
        {
            return Game1.content.LoadString("Strings\\StringsFromCSFiles:Seeds.cs.14209");
        }

        protected override string loadDescription()
        {
            return Game1.content.LoadString("Strings\\StringsFromCSFiles:Seeds.cs.14210");
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            who.Stamina = who.Stamina - (float)(2.0 - (double)who.FarmingLevel * 0.100000001490116);
            this.numberInStack = this.numberInStack - 1;
            this.setCurrentTileIndexToSeedType();
            Game1.playSound("seeds");
        }

        private void setCurrentTileIndexToSeedType()
        {
            //Beetroot?  You want beetroot?
            CurrentParentTileIndex = 62;
        }

        public Dictionary<string, string> Save()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["seedType"] = seedType;
            data["numberInStack"] = numberInStack.ToString();
            return data;
        }

        public void Load(Dictionary<string, string> data)
        {
            seedType = data["seedType"];
            Stack = Convert.ToInt32(data["numberInStack"]);
        }
    }
}
