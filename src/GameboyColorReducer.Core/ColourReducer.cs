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
        public void Process(WorkingImage workingImage)
        {
            QuantizeToGameBoyPalette(workingImage);
            //FirstSweep(workingImage);
            //SecondSweep(workingImage);
            //ThirdSweep(workingImage);
            //ProcessEasyTiles(workingImage);
            // ProcessTransparentTiles(workingImage);

            //ProcessBasedOnExistingTileColours(workingImage);
            //ProcessBasedOnBestNearestEstimate(workingImage);

            //_colourMappingCache.Clear();
        }

        private void QuantizeToGameBoyPalette(WorkingImage workingImage)
        {
            // Define the Game Boy palette
            var palette = new[]
            {
                Colour.GBWhite,
                Colour.GBLight,
                Colour.GBDark,
                Colour.GBBlack
            };

            foreach (var tile in workingImage.Tiles)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        var originalColor = tile.GbcPixels[x, y];
                        var closestColor = FindClosestColor(originalColor, palette);
                        tile.GbPixels[x, y] = closestColor;
                    }
                }
            }
        }

        private Colour FindClosestColor(Colour originalColor, Colour[] palette)
        {
            Colour closestColor = palette[0];
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
            int rDiff = c1.R - c2.R;
            int gDiff = c1.G - c2.G;
            int bDiff = c1.B - c2.B;
            return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
        }

        // colours whole tile
        private void FirstSweep(WorkingImage workingImage)
        {
            var processedTiles = new Dictionary<int, List<Tile>>();

            for (int i = 4; i > 0; i--)
            {
                processedTiles[i - 1] = [];

                var tiles = workingImage.Tiles.ToIEnumerable().Where(x => x.GbcColours.Length == i).ToList();

                foreach (var tile in tiles)
                {
                    if (_colourMappingCache.ContainsKey(tile.GbcColours))
                    {
                        ProcessFromCache(tile);
                    }

                    if (i == 4)
                    {
                        ProcessFourColours(tile);
                    }

                    var adjacentTiles = GetAdjacentTiles(tile, workingImage);
                    foreach (var adjacentTile in adjacentTiles)
                    {
                        if (adjacentTile.GbcColours.Length == i - 1)
                        {
                            ProcessFromSupersetTiles(adjacentTile, [tile]);
                            if (adjacentTile.IsProcessed)
                            {
                                processedTiles[adjacentTile.GbcColours.Length].Add(adjacentTile);
                            }
                        }
                    }
                }

                foreach (var tile in workingImage.Tiles.ToIEnumerable().Where(x => x.GbcColours.Length == i && !x.IsProcessed))
                {
                    if (_colourMappingCache.ContainsKey(tile.GbcColours))
                    {
                        ProcessFromCache(tile);
                    }
                }
            }

            // now for skip tiles, 4 to 2, 3 to 1

            for (int i = 4; i > 2; i--)
            {
                var tiles = workingImage.Tiles.ToIEnumerable().Where(x => x.GbcColours.Length == i).ToList();

                foreach (var tile in tiles)
                {
                    var adjacentTiles = GetAdjacentTiles(tile, workingImage);
                    foreach (var adjacentTile in adjacentTiles)
                    {
                        if (adjacentTile.GbcColours.Length == i - 2)
                        {
                            ProcessFromSupersetTiles(adjacentTile, [tile]);
                        }
                    }
                }

                foreach (var tile in workingImage.Tiles.ToIEnumerable().Where(x => x.GbcColours.Length == i - 2 && !x.IsProcessed))
                {
                    if (_colourMappingCache.ContainsKey(tile.GbcColours))
                    {
                        ProcessFromCache(tile);
                    }
                }
            }

            var tempCache = new Dictionary<Colour[], Dictionary<Colour, Colour>>();

            foreach (var tile in workingImage.Tiles.ToIEnumerable().Where(x => !x.IsProcessed))
            {
                foreach (var cacheItem in _colourMappingCache)
                {
                    if (cacheItem.Key.ContainsAll(tile.GbcColours))
                    {
                        var cache = _colourMappingCache[cacheItem.Key];
                        var newColourMapping = new Dictionary<Colour, Colour>();

                        for (int y = 0; y < 8; y++)
                        {
                            for (int x = 0; x < 8; x++)
                            {
                                tile.GbPixels[x, y] = cache[tile.GbcPixels[x, y]];
                                newColourMapping.TryAdd(tile.GbcPixels[x, y], tile.GbPixels[x, y]);
                            }
                        }

                        tempCache.TryAdd(tile.GbcColours, newColourMapping);
                        tile.IsProcessed = true;
                    }
                }
            }

            // unsure if this is needed
            foreach (var item in tempCache)
            {
                _colourMappingCache.TryAdd(item.Key, item.Value);
            }

            return;
        }

        // colours individual pixels
        private void SecondSweep(WorkingImage workingImage)
        {
            var finishedTiles = workingImage.Tiles.ToIEnumerable().Where(x => x.IsProcessed).ToList();

            foreach (var tile in finishedTiles)
            {
                var adjacentTiles = GetAdjacentTiles(tile, workingImage);
                foreach (var adjacentTile in adjacentTiles)
                {
                    if (!adjacentTile.IsProcessed)
                    {
                        if (_colourMappingCache.ContainsKey(adjacentTile.GbcColours))
                        {
                            ProcessFromCache(adjacentTile);
                        }
                        else
                        {
                            var tileCache = _colourMappingCache[tile.GbcColours];

                            if (adjacentTile.GbcColours.Any(gbcColour => tile.GbcColours.Contains(gbcColour)))
                            {
                                var newColourMapping = new Dictionary<Colour, Colour>();

                                var pixelCounter = 0;
                                for (int y = 0; y < 8; y++)
                                {
                                    for (int x = 0; x < 8; x++)
                                    {
                                        var gbcColour = adjacentTile.GbcPixels[x, y];

                                        if (tileCache.TryGetValue(gbcColour, out var gbColour))
                                        {
                                            adjacentTile.GbPixels[x, y] = gbColour;
                                            newColourMapping.TryAdd(gbcColour, gbColour);
                                            pixelCounter++;
                                        }
                                    }
                                }

                                // cant add to cache because its not completed
                                //_colourMappingCache.TryAdd(adjacentTile.GbcColours, newColourMapping);


                                if (pixelCounter == 64)
                                {
                                    adjacentTile.IsProcessed = true;
                                    _colourMappingCache.TryAdd(adjacentTile.GbcColours, newColourMapping);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ThirdSweep(WorkingImage workingImage)
        {
            // only dealing with unknown colours now
            // get the unfinished tiles, group by closest to furthest completed then process with the approx colour one from before

            var unfinishedTiles = workingImage.Tiles.ToIEnumerable().Where(x => !x.IsProcessed).ToList();
            var tileGrouping = unfinishedTiles
                .OrderByDescending(x => x.GbcColours.Length) // gets the colourful tiles out first
                .GroupBy(x => x.GbcColours.Length - x.GbPixels.Cast<Colour>().Distinct().Count()) // group by how many colours remain
                .OrderByDescending(x => x.Key).ToList(); // gets the most processed tile group first

            foreach (var tileGroup in tileGrouping)
            {
                foreach (var tile in tileGroup)
                {
                    if (_colourMappingCache.ContainsKey(tile.GbcColours))
                    {
                        ProcessFromCache(tile);
                        continue;
                    }

                    var missingColoursFromCache = tile.GbcColours.Where(gbcColour => _colourMappingCache.Keys.Any(key => key.Contains(gbcColour))).ToList();
                    if (missingColoursFromCache.Any())
                    {
                        var newColourMapping = new Dictionary<Colour, Colour>();

                        foreach (var cacheItem in _colourMappingCache)
                        {
                            foreach (var missingColour in missingColoursFromCache)
                            {
                                if (cacheItem.Key.Contains(missingColour))
                                {
                                    var cache = _colourMappingCache[cacheItem.Key];
                                    for (int y = 0; y < 8; y++)
                                    {
                                        for (int x = 0; x < 8; x++)
                                        {
                                            var gbcColour = tile.GbcPixels[x, y];
                                            if (cache.TryGetValue(gbcColour, out var gbColour))
                                            {
                                                tile.GbPixels[x, y] = gbColour;
                                                newColourMapping.TryAdd(gbcColour, gbColour);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (tile.GbcColours.Length == tile.GbPixels.Cast<Colour>().Where(x => !x.IsDefault).Distinct().Count())
                        {
                            _colourMappingCache.TryAdd(tile.GbcColours, newColourMapping);
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
                            tile.IsProcessed = true;
                        }
                    }
                }
            }
        }


        private List<Tile> GetAdjacentTiles(Tile tile, WorkingImage workingImage)
        {
            var adjacentTiles = new List<Tile>();

            if (tile.X > 0)
            {
                adjacentTiles.Add(workingImage.Tiles[tile.X - 1, tile.Y]);
            }
            if (tile.X < workingImage.Tiles.GetLength(0) - 1)
            {
                adjacentTiles.Add(workingImage.Tiles[tile.X + 1, tile.Y]);
            }
            if (tile.Y > 0)
            {
                adjacentTiles.Add(workingImage.Tiles[tile.X, tile.Y - 1]);
            }
            if (tile.Y < workingImage.Tiles.GetLength(1) - 1)
            {
                adjacentTiles.Add(workingImage.Tiles[tile.X, tile.Y + 1]);
            }

            return adjacentTiles;
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

        private void ProcessFromSupersetTiles(Tile tile, IEnumerable<Tile> compareToTiles)
        {
            foreach (var compareToTile in compareToTiles)
            {
                if (!compareToTile.IsProcessed)
                {
                    return;
                }

                if (compareToTile.GbcColours.ContainsAll(tile.GbcColours))
                {
                    var cache = _colourMappingCache[compareToTile.GbcColours];
                    var newColourMapping = new Dictionary<Colour, Colour>();

                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            tile.GbPixels[x, y] = cache[tile.GbcPixels[x, y]];
                            newColourMapping.TryAdd(tile.GbcPixels[x, y], tile.GbPixels[x, y]);
                        }
                    }

                    _colourMappingCache.TryAdd(tile.GbcColours, newColourMapping);
                    tile.IsProcessed = true;
                    return;
                }
            }
        }
    }
}
