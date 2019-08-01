using System;

namespace SpriteWave
{
	class MainClass
	{
		const int nTries = 10;

		static Vec3 randVec(Random rng)
		{
			return new Vec3(
				(float)rng.NextDouble(),
				(float)rng.NextDouble(),
				(float)rng.NextDouble()
			);
		}

		static string FormatVec(byte[] vec)
		{
			return String.Format("{0,3:###},{1,3:###},{2,3:###}", vec[0], vec[1], vec[2]);
		}

		static int DisplayResults(string startName, string convName, Vec3 startVec, Vec3 convVec, Vec3 backVec)
		{
			byte[] start = startVec.ByteVector();
			byte[] conv = convVec.ByteVector();
			byte[] back = backVec.ByteVector();

			int errors = (int)(Math.Sqrt(
				Math.Pow(startVec[0] - backVec[0], 2f) +
				Math.Pow(startVec[1] - backVec[1], 2f) +
				Math.Pow(startVec[2] - backVec[2], 2f)
			) * 255f);

			Console.WriteLine(
				startName + ": " + FormatVec(start) + " -> " +
				convName + ": " + FormatVec(conv) + " -> " +
				"Back: " + FormatVec(back) + " - " +
				errors
			);

			return errors;
		}

		static void Main()
		{
			Random rng = new Random();

			int rgbToHslErrs = 0;
			for (int i = 0; i < nTries; i++)
			{
				Vec3 rgb = randVec(rng);
				Vec3 hsl = ColorSpace.RGBtoHSL(rgb);
				Vec3 back = ColorSpace.HSLtoRGB(hsl);

				rgbToHslErrs += DisplayResults("RGB", "HSL", rgb, hsl, back);
			}
			Console.WriteLine("");

			int hslToRgbErrs = 0;
			for (int i = 0; i < nTries; i++)
			{
				Vec3 hsl = randVec(rng);
				Vec3 rgb = ColorSpace.HSLtoRGB(hsl);
				Vec3 back = ColorSpace.RGBtoHSL(rgb);

				hslToRgbErrs += DisplayResults("HSL", "RGB", hsl, rgb, back);
			}

			Console.WriteLine("\nRGB to HSL errors: {0} (avg. {1})", rgbToHslErrs, (float)rgbToHslErrs / (float)nTries);
			Console.WriteLine("HSL to RGB errors: {0} (avg. {1})\n", hslToRgbErrs, (float)hslToRgbErrs / (float)nTries);

			int errors = rgbToHslErrs + hslToRgbErrs;
			Console.WriteLine("Total errors: {0} (avg. {1})", errors, (float)errors / (float)nTries);
		}
	}
}
