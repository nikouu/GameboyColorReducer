using BenchmarkDotNet.Attributes;
using GameboyColorReducer.Core.ImageConverters;
using GameboyColorReducer.Core.Models;

namespace GameboyColorReducer.Benchmark
{
    [MemoryDiagnoser]
    public class ImageLoadingBenchmarks
    {
        private byte[] _imageData;
        private ImageSharpImageConverter _imageConverter;

        [GlobalSetup]
        public void Setup()
        {
            _imageData = File.ReadAllBytes("Images/PokemonCrystalCeruleanCity.png");
            _imageConverter = new ImageSharpImageConverter();
        }

        [Benchmark(Baseline = true)]
        public WorkingImage ToWorkingImage()
        {
            return _imageConverter.ToWorkingImage(_imageData);
        }
    }
}
