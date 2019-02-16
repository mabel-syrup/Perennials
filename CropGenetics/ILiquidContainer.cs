using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perennials
{
    public interface ILiquidContainer
    {
        void addLiquid(int amount);
        void setLiquid(int amount);
        int getLiquidAmount();

    }
}
