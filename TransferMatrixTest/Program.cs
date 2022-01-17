// See https://aka.ms/new-console-template for more information
using System.Globalization;
using System.Numerics;
using SharpTransferMatrix;

Console.WriteLine("C# rewrite of cppTransferMatrix");

string outputfile = "matrixtest.csv";

/**
 * Step 1
 * Define the frequency range
 * startFreq: starting frequency
 * endFreq: end frequency
 * step:  step in the frequency range
 * units: conversion to Hz
 */
double startFreq = 1.0; // THz
double endFreq = 10.0; // THz
double step = 0.1; // THz
double units = 1e12; // choose frequency unit : 1e12 for THz

/**
 * Step 2 
 * If we want to save the results of our calculation
 * we open a txt file here.
 */
List<Complex> rs = new List<Complex>();
List<Complex> ts = new List<Complex>();
List<double> freqList = new List<double>();

using StreamWriter calcFile = new(outputfile);

/**
 * Step 3 
 * Define the materials and layers
 */

Material air = new Material();
Material Si = new Material(12.1);
Layer superstrate = new Layer(1e-6, air, "superstrate");
Layer dielectricLayer = new Layer(1e-6, Si, "Si");
Layer substrate = new Layer(1e-6, air);

List<Layer> structure = new List<Layer>(new Layer[] { superstrate, dielectricLayer, substrate });

/**
 * Step 4
 * Run the calculation!
 */
for (double frequency = startFreq; frequency <= endFreq; frequency += step)
{
	var freq = Math.Round(frequency, 4);
	decimal w = 2.0m * (decimal)Math.PI * (decimal)freq * 1e12m;
	TransferMatrix system = new TransferMatrix((double)w, 0.0, structure);

	system.calculate();

	freqList.Add(freq);
	rs.Add(system.getRs());
	ts.Add(system.getTs());

}

Console.WriteLine($"Calculation completed for {freqList.Count} frequencies.");

calcFile.WriteLine($"Frequency, rs[real], rs[imaginary], ts[real], ts[imaginary]");
Console.WriteLine($"Frequency, rs[real], rs[imaginary], ts[real], ts[imaginary]");

for (int i = 0; i < freqList.Count; i++)
{
	//freqList[i] << ", " << rs[i].real() << ", " << rs[i].imag() << ", ";
	calcFile.WriteLine($"{freqList[i].ToString().Replace(",", ".")}, {Convert.ToString(rs[i].Real, CultureInfo.InvariantCulture)}, {Convert.ToString(rs[i].Imaginary, CultureInfo.InvariantCulture)}, {Convert.ToString(ts[i].Real, CultureInfo.InvariantCulture)}, {Convert.ToString(ts[i].Imaginary, CultureInfo.InvariantCulture)}");
	Console.WriteLine($"{freqList[i].ToString().Replace(",", ".")}, {Convert.ToString(rs[i].Real, CultureInfo.InvariantCulture)}, {Convert.ToString(rs[i].Imaginary, CultureInfo.InvariantCulture)}, {Convert.ToString(ts[i].Real, CultureInfo.InvariantCulture)}, {Convert.ToString(ts[i].Imaginary, CultureInfo.InvariantCulture)}");
}
calcFile.Close();