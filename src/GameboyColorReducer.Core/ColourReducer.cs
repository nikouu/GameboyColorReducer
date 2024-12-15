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

        public void Process(WorkingImage workingImage)
        {
            ProcessEasyTiles(workingImage);
            ProcessTransparentTiles(workingImage);
            ProcessBasedOnExistingTileColours(workingImage);
        }

        private void ProcessEasyTiles(WorkingImage workingImage)
        {
            // Creates a group of tiles by number of colours in colour count descending order. (I.e. 4 colour tile group, 3 colour, 2 then 1).
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

                            ProcessFromSimilarColouredTiles(tile, compareToTiles);
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

            foreach (var tile in workingImage.Tiles.ToIEnumerable().Where(x => !x.IsProcessed))
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        var gbcColour = tile.GbcPixels[x, y];

                        if (step4.TryGetValue(gbcColour, out Colour value))
                        {
                            tile.GbPixels[x, y] = value;
                        }
                    }
                }

                tile.IsProcessed = true;
            }
        }

        private void ProcessFourColours(Tile tile)
        {
            var lightestToDarkestColours = tile.GbcColours.OrderByDescending(x => x.GetBrightness());
            var colourMapping = lightestToDarkestColours.Zip(Colour.GbColourList, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
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

        private void ProcessFromSimilarColouredTiles(Tile tile, IEnumerable<Tile> compareToTiles)
        {
            foreach (var compareToTile in compareToTiles)
            {
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
