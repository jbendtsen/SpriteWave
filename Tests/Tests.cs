using System;

namespace SpriteWave
{
	public class Tests
	{
		int nTests = 0;
		int nSuccesses = 0;
		const uint OrderAndDepth = 0x12305551;

		public void ViewResults(string testName, bool testRes)
		{
			nTests++;
			if (testRes)
				nSuccesses++;

			string status = testRes ? "Success!" : "Fail :(";
			Console.WriteLine(testName + ": " + status + "\n");
		}

		public void PrintResultsUInt(uint expected, uint actual)
		{
			Console.WriteLine("Expected: " + expected.ToString("X8") + ", Actual: " + actual.ToString("X8"));
		}

		public bool TestNativeToRGBA(uint expected, uint c)
		{
			uint actual = new ColourTable(OrderAndDepth, null).NativeToRGBA(c);
			PrintResultsUInt(expected, actual);
			return actual == expected;
		}

		public bool TestRGBAToNative(uint expected, uint rgba)
		{
			uint actual = new ColourTable(OrderAndDepth, null).RGBAToNative(rgba);
			PrintResultsUInt(expected, actual);
			return actual == expected;
		}

		public bool TestCreateSuffixException(string fmt)
		{
			string msg = null;

			try {
				var suff = new Suffix(fmt);
			}
			catch (Exception ex) {
				if (ex is ArgumentException || ex is ArgumentOutOfRangeException)
					msg = ex.Message;
				else
					throw;
			}

			Console.WriteLine(
				"new Suffix(\"{0}\") -> {1}",
				fmt, msg != null ? msg : "No exception"
			);

			return msg != null;
		}

		public bool TestSuffixGenerateException(string fmt, int num)
		{
			string msg = null;

			try {
				string str = new Suffix(fmt).Generate(num);
			}
			catch (Exception ex) {
				if (ex is ArgumentException || ex is ArgumentOutOfRangeException)
					msg = ex.Message;
				else
					throw;
			}

			Console.WriteLine(
				"new Suffix(\"{0}\").Generate({1}) -> {2}",
				fmt, num, msg != null ? msg : "No exception"
			);

			return msg != null;
		}

		public bool TestSuffixGenerate(string expected, string fmt, int num)
		{
			string output = new Suffix(fmt).Generate(num);
			Console.WriteLine("Expected: " + expected + ", Actual: " + output);
			return output == expected;
		}

		public void Run()
		{
			ViewResults("NativeToRGBA", TestNativeToRGBA(0xFF00FFFF, 0xFC1F));
			ViewResults("RGBAToNative", TestRGBAToNative(0x83FF, 0x00FFFFFF));
			ViewResults("RGBAToNative", TestRGBAToNative(0xFC1F, 0xFF00FFFF));
			ViewResults("RGBAToNative", TestRGBAToNative(0xFFE0, 0xFFFF00FF));
			ViewResults("RGBAToNative", TestRGBAToNative(0x7FFF, 0xFFFFFF00));

			ViewResults("CreateSuffix (trailing '{')", TestCreateSuffixException("_{{d2}"));
			ViewResults("CreateSuffix (too short)", TestCreateSuffixException("-{d}"));
			ViewResults("CreateSuffix (base type)", TestCreateSuffixException("-{l}"));
			ViewResults("CreateSuffix (nDigits < 1)", TestCreateSuffixException("_{d0}"));

			ViewResults("SuffixGenerate (num is -ve)", TestSuffixGenerateException("_{d2}", -10));
			ViewResults("SuffixGenerate (num exceeds limit)", TestSuffixGenerateException("_{b3}", 10));
			ViewResults("SuffixGenerate", TestSuffixGenerate("number 090 here", "number {d3} here", 90));
			ViewResults("SuffixGenerate", TestSuffixGenerate("_0f4", "_{x3}", 244));
			ViewResults("SuffixGenerate", TestSuffixGenerate("_0F4", "_{X3}", 244));

			Console.WriteLine("\nTests Passed: " + nSuccesses + "/" + nTests);
		}
	}

	public class TestsMain
	{
		public static void Main()
		{
			new Tests().Run();
		}
	}
}
