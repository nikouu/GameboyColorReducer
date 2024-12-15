using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyColorReducer.Core.Models
{
    public class Tile : IEquatable<Tile>
    {
        public readonly int X;
        public readonly int Y;

        public readonly Colour[,] GbcPixels;

        private Colour[,] _gbPixels = new Colour[8, 8];
        public Colour[,] GbPixels => _gbPixels;

        public readonly Colour[] GbcColours;
        public readonly string GbcColoursKey;

        public bool IsProcessed { get; set; } = false;

        private readonly int _gbcColoursHash;

        public Tile(int x, int y, Colour[,] gbcPixels)
        {
            X = x;
            Y = y;
            GbcPixels = gbcPixels;

            GbcColours = [.. gbcPixels.Cast<Colour>().Distinct().OrderBy(x => x.GetHashCode())];
            GbcColoursKey = GenerateColourKeyString();

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

        public string GenerateColourKeyString()
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in GbcColours.OrderBy(x => x.GetBrightness()))
            {
                stringBuilder.Append(item);
            }

            return stringBuilder.ToString();
        }
    }
}
