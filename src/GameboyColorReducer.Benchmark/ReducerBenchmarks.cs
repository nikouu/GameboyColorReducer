﻿using BenchmarkDotNet.Attributes;
using GameboyColorReducer.Core;
using GameboyColorReducer.Core.ImageConverters;
using GameboyColorReducer.Core.Models;

namespace GameboyColorReducer.Benchmark
{
    [MemoryDiagnoser]
    public class ReducerBenchmarks
    {
        private byte[] _imageData;
        private ColourReducer _reducer;
        private ImageSharpImageConverter _imageConverter;
        private WorkingImage _workingImage;

        [GlobalSetup]
        public void Setup()
        {
            _imageData = File.ReadAllBytes("Images/PokemonCrystalCeruleanCity.png");
            _reducer = new ColourReducer();
            _imageConverter = new ImageSharpImageConverter();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _workingImage = _imageConverter.ToWorkingImage(_imageData);
        }

        [Benchmark(Baseline = true)]
        public WorkingImage QuantizePerTileOriginal()
        {
            _reducer.QuantizePerTile(_workingImage);
            return _workingImage;
        }
    }
}
