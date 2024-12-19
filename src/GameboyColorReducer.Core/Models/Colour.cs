﻿using System.Collections.Frozen;

namespace GameboyColorReducer.Core.Models
{
    // A trimmed version of the System.Drawing Color object
    public readonly struct Colour : IEquatable<Colour>, IComparable<Colour>
    {
        internal static readonly FrozenSet<Colour> _gbColourList = new List<Colour> { GBWhite, GBLight, GBDark, GBBlack }.ToFrozenSet();

        public static Colour GBWhite => FromRgb(224, 248, 207);
        public static Colour GBLight => FromRgb(134, 192, 108);
        public static Colour GBDark => FromRgb(48, 104, 80);
        public static Colour GBBlack => FromRgb(7, 24, 33);

        private readonly bool? _isDefault;

        // standard 32bit sRGB (ARGB)
        private readonly long value;

        private const int ARGBAlphaShift = 24;
        private const int ARGBRedShift = 16;
        private const int ARGBGreenShift = 8;
        private const int ARGBBlueShift = 0;

        public byte R => unchecked((byte)(value >> ARGBRedShift));
        public byte G => unchecked((byte)(value >> ARGBGreenShift));
        public byte B => unchecked((byte)(value >> ARGBBlueShift));
        public byte A => unchecked((byte)(value >> ARGBAlphaShift));

        public static List<Colour> GbColourList => new(_gbColourList);

        internal Colour(long value, bool isDefault)
        {
            _isDefault = isDefault;
            this.value = value;
        }

        public static Colour FromRgb(int red, int green, int blue)
        {
            // if alpha is 0, it means completely transparent and other colours won't matter
            return FromArgb(255, red, green, blue);
        }

        public static Colour FromRgb(int alpha, int red, int green, int blue)
        {
            return FromArgb(alpha, red, green, blue);
        }

        public static Colour FromArgb(int alpha, int red, int green, int blue)
        {
            CheckByte(alpha, nameof(alpha));
            CheckByte(red, nameof(red));
            CheckByte(green, nameof(green));
            CheckByte(blue, nameof(blue));

            return new Colour(
                (uint)alpha << ARGBAlphaShift |
                (uint)red << ARGBRedShift |
                (uint)green << ARGBGreenShift |
                (uint)blue << ARGBBlueShift,
                false
            );
        }

        // https://stackoverflow.com/a/25168506
        public static bool IsCloseColour(Colour a, Colour z, int threshold = 50)
        {
            int r = (int)a.R - z.R,
                g = (int)a.G - z.G,
                b = (int)a.B - z.B;
            return (r * r + g * g + b * b) <= threshold * threshold;
        }

        // https://www.nbdtech.com/Blog/archive/2008/04/27/calculating-the-perceived-brightness-of-a-color.aspx
        public int GetBrightness()
        {
            return (int)Math.Sqrt(
               R * R * .241 +
               G * G * .691 +
               B * B * .068);
        }

        public bool IsBlank
            => A == 0 && R == 0 && G == 0 && B == 0;

        public bool IsDefault => _isDefault ?? true;

        // https://stackoverflow.com/a/37821008
        public string ToHexString => $"#{R:X2}{G:X2}{B:X2}";

        // RGB(R, G, B)
        public string RgbString => $"RGB({R}, {G}, {B})";

        // #RRGGBBAA
        public string ToHexaString => $"#{R:X2}{G:X2}{B:X2}{A:X2}";

        public double ToProportion(byte b) => b / (double)Byte.MaxValue;

        // RGBA(R, G, B, A)
        public string ToRgbaString => $"RGBA({R}, {G}, {B},{ToProportion(A):N2})";

        private static void CheckByte(int value, string name)
        {
            if (unchecked((uint)value) > byte.MaxValue)
            {
                throw new ArgumentException();
            }
        }

        public static bool operator ==(Colour left, Colour right) =>
            left.value == right.value;

        public static bool operator !=(Colour left, Colour right) => !(left == right);

        public bool Equals(Colour other) => this == other;

        public override bool Equals(object? obj)
        {
            return obj is Colour other && Equals(other);
        }

        public override string ToString() => ToRgbaString;

        public override int GetHashCode() => value.GetHashCode();

        public int CompareTo(Colour other)
        {
            throw new NotImplementedException();
        }
    }

}
