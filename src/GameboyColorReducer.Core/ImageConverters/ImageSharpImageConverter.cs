using GameboyColorReducer.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyColorReducer.Core.ImageConverters
{
    public class ImageSharpImageConverter : IImageConverter
    {
        public WorkingImage ToWorkingImage(byte[] inputImage)
        {
            using var image = Image.Load<Rgba32>(inputImage);
            ValidateImage(image);

            var tiles = new Tile[image.Width / 8, image.Height / 8];

            // todo: see if width then height, or height then width for performance
            for (int i = 0; i < image.Width; i += 8)
            {
                for (int j = 0; j < image.Height; j += 8)
                {
                    var gbcPixels = new Colour[8, 8];

                    // todo: can this be a static lambda?
                    image.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            var pixelRow = accessor.GetRowSpan(j + y);
                            for (int x = 0; x < 8; x++)
                            {
                                var pixel = pixelRow[i + x];
                                gbcPixels[x, y] = Colour.FromRgb(pixel.A, pixel.R, pixel.G, pixel.B);
                            }
                        }
                    });

                    tiles[i / 8, j / 8] = new Tile(i / 8, j / 8, gbcPixels);
                }
            }

            return new WorkingImage(image.Width, image.Height, tiles);
        }

        public byte[] ToByteArray(WorkingImage workingImage)
        {
            using var image = new Image<Rgba32>(workingImage.Width, workingImage.Height);

            for (int i = 0; i < workingImage.Width; i += 8)
            {
                for (int j = 0; j < workingImage.Height; j += 8)
                {
                    var tile = workingImage.Tiles[i / 8, j / 8];
                    image.ProcessPixelRows(accessor =>
                    {
                        // todo: validate colour count per tile
                        for (int y = 0; y < 8; y++)
                        {
                            var pixelRow = accessor.GetRowSpan(j + y);
                            for (int x = 0; x < 8; x++)
                            {
                                var color = tile.GbPixels[x, y];
                                pixelRow[i + x] = new Rgba32(color.R, color.G, color.B, color.A);
                            }
                        }
                    });
                }
            }

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }

        private static void ValidateImage(Image image)
        {
            if (image.Height % 8 != 0)
            {
                throw new ArgumentException($"Image height of {image.Height}px is not divisible by 8.");
            }

            if (image.Width % 8 != 0)
            {
                throw new ArgumentException($"Image width of {image.Width}px is not divisible by 8.");
            }
        }
    }
}
