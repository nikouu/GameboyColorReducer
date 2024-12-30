using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyColorReducer.Core.Models
{
    public class Tile : IEquatable<Tile>
    {
        public readonly int Id;
        public readonly int X;
        public readonly int Y;

        public readonly Colour[,] GbcPixels;

        private Colour[,] _gbPixels = new Colour[8, 8];
        public Colour[,] GbPixels => _gbPixels;

        public readonly Colour[] GbcColours;

        public bool IsProcessed { get; set; } = false;

        private readonly int _gbcColoursHash;

        public Tile(int id, int x, int y, Colour[,] gbcPixels)
        {
            Id = id;
            X = x;
            Y = y;
            GbcPixels = gbcPixels;

            GbcColours = GetDistinctOrderedColours(gbcPixels);

            var hash = 0;
            foreach (var colour in GbcColours)
            {
                hash += colour.GetHashCode();
            }

            _gbcColoursHash = hash;
        }

        public static bool operator ==(Tile left, Tile right)
        {
            return left.GetHashCode() == right.GetHashCode();
        }

        public static bool operator !=(Tile left, Tile right)
        {
            return left.GetHashCode() != right.GetHashCode();
        }

        public bool Equals(Tile? other)
        {
            if (other is null)
                return false;

            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is Tile other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _gbcColoursHash;
        }

        private static Colour[] GetDistinctOrderedColours(Colour[,] gbcPixels)
        {
            var distinctColours = new HashSet<Colour>();
            foreach (var colour in gbcPixels)
            {
                distinctColours.Add(colour);
            }
            return [.. distinctColours.OrderBy(x => x.GetHashCode())];
        }
    }
}
