// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using GameboyColorReducer.Core;
using GameboyColorReducer.Core.ImageConverters;


Console.WriteLine("Hello, World!");

var g = new ImageSharpImageConverter();

var b = File.ReadAllBytes("Images/PokemonCrystalCeruleanCity.png");

var h = g.ToWorkingImage(b);

var e = new ColourReducer();

e.QuantizePerTile(h);

var k = g.ToByteArray(h);

File.WriteAllBytes("Images/PokemonCrystalCeruleanCity-reduced.png", k);

//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);