// See https://aka.ms/new-console-template for more information
using GameboyColorReducer.Core;
using GameboyColorReducer.Core.ImageConverters;
using System.ComponentModel.DataAnnotations;

Console.WriteLine("Hello, World!");

var g = new ImageSharpImageConverter();

var b = File.ReadAllBytes("Images/PokemonCrystalCeruleanCity.png");

var h = g.ToWorkingImage(b);

var e = new ColourReducer();

e.Process(h);

var k = g.ToByteArray(h);

File.WriteAllBytes("Images/PokemonCrystalCeruleanCity-reduced.png", k);