using GameboyColorReducer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyColorReducer.Core
{
    public class ColourReducer
    {
        private Dictionary<Colour[], Dictionary<Colour, Colour>> _colourMappingCache = new(new ColourArrayEqualityComparer());

        // todo: issue where white obviously needs to be used, but a brighter colour takes over. see the cerulean city windows
 
        public void QuantizePerTile(WorkingImage workingImage)
        {
            ProcessEasyTiles(workingImage);
            ProcessTransparentTiles(workingImage);
            ProcessBasedOnExistingTileColours(workingImage);
            ProcessBasedOnBestNearestEstimate(workingImage);

            _colourMappingCache.Clear();
        }

        public void QuantizeToGameBoyPalette(WorkingImage workingImage)
        {
            foreach (var tile in workingImage.Tiles)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        var originalColor = tile.GbcPixels[x, y];
                        var closestColor = FindClosestColor(originalColor, Colour.GbColourList);
                        tile.GbPixels[x, y] = closestColor;
                    }
                }
            }
        }

        private Colour FindClosestColor(Colour originalColor, IReadOnlySet<Colour> palette)
        {
            Colour closestColor = palette.ElementAt(0);
            double closestDistance = double.MaxValue;

            foreach (var color in palette)
            {
                var distance = GetColorDistance(originalColor, color);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestColor = color;
                }
            }

            return closestColor;
        }

        private double GetColorDistance(Colour c1, Colour c2)
        {
            int rDiff = Math.Abs(c1.R - c2.R);
            int gDiff = Math.Abs(c1.G - c2.G);
            int bDiff = Math.Abs(c1.B - c2.B);
            return rDiff + gDiff + bDiff;
        }

        private void ProcessEasyTiles(WorkingImage workingImage)
        {
            // Creates a group of tiles by number of colours in gbColour count descending order. (I.e. 4 gbColour tile group, 3 gbColour, 2 then 1).
            var tileGroups = workingImage.Tiles.ToIEnumerable().GroupBy(x => x.GbcColours.Length).OrderByDescending(x => x.Key).ToList();

            foreach (var tileGroup in tileGroups)
            {
                foreach (var tile in tileGroup)
                {
                    if (_colourMappingCache.ContainsKey(tile.GbcColours))
                    {
                        ProcessFromCache(tile);
                    }
                    else if (tile.GbcColours.Length == 4)
                    {
                        ProcessFourColours(tile);
                    }
                    else
                    {
                        // look at the groups above which are possibly completed
                        if (tileGroups.Any(x => x.Key == tileGroup.Key + 1))
                        {
                            var compareToTiles = tileGroups.Where(x => x.Key == tileGroup.Key + 1).First().Where(x => x.IsProcessed).OrderBy(x => x.GbcColours.Length);

                            //ProcessFromSupersetTiles(tile, compareToTiles);
                        }
                    }
                }
            }
        }

        private void ProcessTransparentTiles(WorkingImage workingImage)
        {
            foreach (var tile in workingImage.Tiles.ToIEnumerable().Where(x => !x.IsProcessed && x.GbcColours.Length == 1))
            {
                if (tile.GbcColours[0].IsBlank)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            tile.GbPixels[x, y] = Colour.GBBlack;
                        }
                    }
                    tile.IsProcessed = true;
                }
            }
        }
        private void ProcessBasedOnExistingTileColours(WorkingImage workingImage)
        {
            var step1 = _colourMappingCache.SelectMany(x => x.Value);
            var step2 = step1.OrderByDescending(x => x.Key.GetBrightness());
            var step3 = step2.GroupBy(x => x.Key);

            var step4 = step3.ToDictionary(k => k.Key, v => v.GroupBy(x => x.Value).OrderByDescending(x => x.Count()).First().First().Value);



            foreach (var tileGroup in workingImage.Tiles.ToIEnumerable().Where(x => !x.IsProcessed).GroupBy(x => x.GbcColours.Length))
            {
                foreach (var tile in tileGroup)
                {
                    int pixelCounter = 0;
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            var gbcColour = tile.GbcPixels[x, y];

                            if (step4.TryGetValue(gbcColour, out Colour value))
                            {
                                tile.GbPixels[x, y] = value;
                                pixelCounter++;
                            }
                        }
                    }

                    if (pixelCounter == 64)
                    {
                        tile.IsProcessed = true;
                    }
                }
            }
        }

        private void ProcessBasedOnBestNearestEstimate(WorkingImage workingImage)
        {
            var allColoursAndMappings = _colourMappingCache.SelectMany(kvp => kvp.Value)
                        .GroupBy(kvp => kvp.Key)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(kvp => kvp.Value).ToList());

            foreach (var tile in workingImage.Tiles.ToIEnumerable().Where(x => !x.IsProcessed))
            {
                if (_colourMappingCache.TryGetValue(tile.GbcColours, out Dictionary<Colour, Colour>? cachedMapping))
                {
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            tile.GbPixels[x, y] = cachedMapping[tile.GbcPixels[x, y]];
                        }
                    }

                    tile.IsProcessed = true;
                }
                else
                {
                    // todo: rent this object
                    var colourMapping = new Dictionary<Colour, Colour>();
                    // search through pixel by pixel then update the cache with the whole tile
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            var gbColour = tile.GbPixels[x, y];
                            var gbcColour = tile.GbcPixels[x, y];

                            if (colourMapping.TryGetValue(gbcColour, out Colour value))
                            {
                                tile.GbPixels[x, y] = value;
                            }
                            else if (gbColour.IsDefault)
                            {
                                var existingGbColoursForTile = tile.GbPixels.ToIEnumerable().Where(x => !x.IsDefault);

                                var gbcColourBrightness = gbcColour.GetBrightness();

                                var remainingColourOptions = Colour.GbColourList.Except(existingGbColoursForTile);



                                Colour bestMatch;

                                try
                                {
                                    bestMatch = remainingColourOptions.OrderBy(x => Math.Abs(x.GetBrightness() - gbcColourBrightness)).First();
                                }
                                catch
                                {
                                    bestMatch = Colour.FromRgb(255, 87, 51);
                                }

                                tile.GbPixels[x, y] = bestMatch;
                                colourMapping.TryAdd(gbcColour, bestMatch);
                            }
                            else
                            {
                                colourMapping.TryAdd(gbcColour, gbColour);
                            }
                        }
                    }

                    _colourMappingCache.TryAdd(tile.GbcColours, colourMapping);
                }
            }
        }

        private void ProcessFourColours(Tile tile)
        {
            var lightestToDarkestColours = tile.GbcColours.OrderByDescending(x => x.GetBrightness()).ToList();
            var colourMapping = new Dictionary<Colour, Colour>();

            // Check if any color is similar to GBWhite
            // todo: might need to do the same for GBBlack
            var similarToWhite = lightestToDarkestColours.FirstOrDefault(c => Colour.IsCloseColour(c, Colour.GBWhite));

            if (!similarToWhite.IsDefault)
            {
                // Map the similar color to GBWhite
                colourMapping[similarToWhite] = Colour.GBWhite;
                lightestToDarkestColours.Remove(similarToWhite);

                // Zip the remaining colors based on brightness
                var remainingGbColours = Colour.GbColourList.Except(new[] { Colour.GBWhite }).ToList();
                var remainingMapping = lightestToDarkestColours.Zip(remainingGbColours, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

                // Add the remaining mappings to the colourMapping
                foreach (var kvp in remainingMapping)
                {
                    colourMapping[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                // Continue with the existing zipping if no color is similar to GBWhite
                colourMapping = lightestToDarkestColours.Zip(Colour.GbColourList, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
            }

            _colourMappingCache.TryAdd(tile.GbcColours, colourMapping);

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    tile.GbPixels[x, y] = colourMapping[tile.GbcPixels[x, y]];
                }
            }

            tile.IsProcessed = true;
        }

        private void ProcessFromCache(Tile tile)
        {
            var cachedMapping = _colourMappingCache[tile.GbcColours];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    tile.GbPixels[x, y] = cachedMapping[tile.GbcPixels[x, y]];
                }
            }

            tile.IsProcessed = true;
        }
    }
}
