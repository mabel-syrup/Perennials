using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CropGenetics
{
    public class Ditch
    {
        public List<Vector2> tiles = new List<Vector2>();
        int highestWater = 0;

        public Ditch()
        {
        }

        public Ditch(Vector2 tile)
        {
            tiles.Add(tile);
        }

        public Boolean isAdjacent(Vector2 position)
        {
            foreach(Vector2 tile in tiles)
            {
                if (position.X == tile.X && Math.Abs(position.Y - tile.Y) == 1)
                    return true;
                if (position.Y == tile.Y && Math.Abs(position.X - tile.X) == 1)
                    return true;
            }
            return false;
        }

        public void updateHighest(int level)
        {
            if (level > highestWater)
                highestWater = level;
        }

        public int getWaterLevel()
        {
            return highestWater;
        }
    }
}
