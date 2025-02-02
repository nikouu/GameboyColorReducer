﻿using GameboyColorReducer.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameboyColorReducer.Core.ImageConverters
{
    public class ImageSharpImageConverter : IImageConverter
    {
        public WorkingImage ToWorkingImage(byte[] inputImage)
        {
            using var image = Image.Load<Rgba32>(inputImage);
            ValidateImage(image);
            var tiles = new Tile[image.Width / 8, image.Height / 8];

            image.ProcessPixelRows(accessor =>
            {
                for (int i = 0; i < image.Width; i += 8)
                {
                    for (int j = 0; j < image.Height; j += 8)
                    {
                        var gbcPixels = new Colour[8, 8];

                        for (int y = 0; y < 8; y++)
                        {
                            var pixelRow = accessor.GetRowSpan(j + y);
                            for (int x = 0; x < 8; x++)
                            {
                                var pixel = pixelRow[i + x];
                                gbcPixels[x, y] = Colour.FromRgb(pixel.A, pixel.R, pixel.G, pixel.B);
                            }
                        }

                        var x1 = i / 8;
                        var y1 = j / 8;

                        var id = (x1 * (image.Height / 8)) + y1;

                        tiles[x1, y1] = new Tile(id, x1, y1, gbcPixels);
                    }
                }
            });

            return new WorkingImage(image.Width, image.Height, tiles);
        }

        public byte[] ToByteArray(WorkingImage workingImage)
        {
            using var image = new Image<Rgba32>(workingImage.Width, workingImage.Height);
            image.ProcessPixelRows(accessor =>
            {
                for (int i = 0; i < workingImage.Width; i += 8)
                {
                    for (int j = 0; j < workingImage.Height; j += 8)
                    {
                        var tile = workingImage.Tiles[i / 8, j / 8];

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

                    }
                }
            });
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }

        private static void ValidateImage(Image image)
        {
            if (image.Height % 8 != 0)
            {
                throw new ReducerException($"Image height of {image.Height}px is not divisible by 8.");
            }

            if (image.Width % 8 != 0)
            {
                throw new ReducerException($"Image width of {image.Width}px is not divisible by 8.");
            }
        }
    }
}
