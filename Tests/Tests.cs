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

		public void Run()
		{
			ViewResults("NativeToRGBA", TestNativeToRGBA(0xFF00FFFF, 0xFC1F));
			ViewResults("RGBAToNative", TestRGBAToNative(0x83FF, 0x00FFFFFF));
			ViewResults("RGBAToNative", TestRGBAToNative(0xFC1F, 0xFF00FFFF));
			ViewResults("RGBAToNative", TestRGBAToNative(0xFFE0, 0xFFFF00FF));
			ViewResults("RGBAToNative", TestRGBAToNative(0x7FFF, 0xFFFFFF00));

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
