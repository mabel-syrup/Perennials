using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using _SyrupFramework;

namespace Perennials
{
    class CropScythe : SoilCrop
    {
        public CropScythe() { }

        public CropScythe(string cropName, int heightOffset = 0) : base(cropName, heightOffset)
        {
            Logger.Log("CropScythe was created!");
            shakable = true;
            impassable = false;
        }


    }
}
