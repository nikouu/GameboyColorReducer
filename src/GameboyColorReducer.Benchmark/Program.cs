// See https://aka.ms/new-console-template for more information
using GameboyColorReducer.Core.ImageConverters;
using System.ComponentModel.DataAnnotations;

Console.WriteLine("Hello, World!");

var g = new ImageSharpImageConverter();

var b = File.ReadAllBytes("Images/mokki1.png");

var h = g.ConvertToWorkingImage(b);

var k = g.ConvertToByteArray(h);

File.WriteAllBytes("Images/mokki2.png", k);