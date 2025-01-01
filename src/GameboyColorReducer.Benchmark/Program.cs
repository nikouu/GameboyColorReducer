// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using GameboyColorReducer.Core;
using GameboyColorReducer.Core.ImageConverters;


Console.WriteLine("Hello, World!");

var filename = "PokemonCrystalCeruleanCity";

var g = new ImageSharpImageConverter();

var b = File.ReadAllBytes($"Images/{filename}.png");

var h = g.ToWorkingImage(b);

var e = new ColourReducer();

e.QuantizePerTile(h);

var k = g.ToByteArray(h);

File.WriteAllBytes($"Images/{filename}-reduced.png", k);

e.QuantizeToGameBoyPalette(h);

File.WriteAllBytes($"Images/{filename}-reduced-gb-palette.png", g.ToByteArray(h));






#if !DEBUG
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif