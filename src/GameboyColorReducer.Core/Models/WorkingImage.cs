using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyColorReducer.Core.Models
{
    public class WorkingImage
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Tile[,] Tiles;

        public WorkingImage(int width, int height, Tile[,] tiles)
        {
            Width = width;
            Height = height;
            Tiles = tiles;
        }
    }
}
