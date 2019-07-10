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

		public bool TestException(string exType, Action action)
		{
			string msg = null;
			bool success = false;

			try {
				action();
			}
			catch (Exception ex) {
				msg = ex.Message;
				if (ex.GetType() == Type.GetType("System." + exType))
					success = true;
				else
					throw;
			}

			Console.WriteLine(msg != null ? msg : "No exception");
			return success;
		}

		public bool TestSuffixGenerate(string expected, string fmt, int num)
		{
			string output = new Suffix(fmt).Generate(num);
			Console.WriteLine("Expected: " + expected + ", Actual: " + output);
			return output == expected;
		}

		public bool TestSuffixValueOf(int expected, string fmt, string str)
		{
			int value = new Suffix(fmt).ValueOf(str);
			Console.WriteLine("Expected: " + expected + ", Actual: " + value);
			return value == expected;
		}

		public void Run()
		{
			ViewResults("NativeToRGBA", TestNativeToRGBA(0xFF00FFFF, 0xFC1F));
			ViewResults("RGBAToNative", TestRGBAToNative(0x83FF, 0x00FFFFFF));
			ViewResults("RGBAToNative", TestRGBAToNative(0xFC1F, 0xFF00FFFF));
			ViewResults("RGBAToNative", TestRGBAToNative(0xFFE0, 0xFFFF00FF));
			ViewResults("RGBAToNative", TestRGBAToNative(0x7FFF, 0xFFFFFF00));

			ViewResults("CreateSuffix (trailing '{')", TestException("ArgumentException", () => {var suff = new Suffix("_{{d2");}));
			ViewResults("CreateSuffix (too short)", TestException("ArgumentException", () => {var suff = new Suffix("-{d}");}));
			ViewResults("CreateSuffix (base type)", TestException("ArgumentException", () => {var suff = new Suffix("-{l}");}));
			ViewResults("CreateSuffix (nDigits < 1)", TestException("ArgumentException", () => {var suff = new Suffix("_{d0}");}));

			ViewResults("SuffixGenerate (num is -ve)", TestException("ArgumentException", () => {var str = new Suffix("_{d2}").Generate(-10);}));
			ViewResults("SuffixGenerate (num exceeds limit)", TestException("ArgumentException", () => {var str = new Suffix("_{b3}").Generate(10);}));

			ViewResults("SuffixGenerate", TestSuffixGenerate("number 090 here", "number {d3} here", 90));
			ViewResults("SuffixGenerate", TestSuffixGenerate("_0f4", "_{x3}", 244));
			ViewResults("SuffixGenerate", TestSuffixGenerate("_0F4", "_{X3}", 244));

			ViewResults("SuffixValueOf (no insert)", TestException("InvalidOperationException", () => {var num = new Suffix("nothing here").ValueOf("nothing here");}));
			ViewResults("SuffixValueOf (incorrect size 1)", TestException("ArgumentException", () => {var num = new Suffix("{d2} cc").ValueOf("not correct");}));
			ViewResults("SuffixValueOf (incorrect size 2)", TestException("ArgumentException", () => {var num = new Suffix("aa {d2} cc").ValueOf("not correct");}));
			ViewResults("SuffixValueOf (incorrect size 3)", TestException("ArgumentException", () => {var num = new Suffix("aa {d2}").ValueOf("not correct");}));
			ViewResults("SuffixValueOf (invalid input 1)", TestException("ArgumentException", () => {var num = new Suffix("{d2} cc").ValueOf("00 cd");}));
			ViewResults("SuffixValueOf (invalid input 2)", TestException("ArgumentException", () => {var num = new Suffix("aa {d2} cc").ValueOf("aa 00 cd");}));
			ViewResults("SuffixValueOf (invalid input 3)", TestException("ArgumentException", () => {var num = new Suffix("aa {d2} cc").ValueOf("ab 00 cc");}));
			ViewResults("SuffixValueOf (invalid input 4)", TestException("ArgumentException", () => {var num = new Suffix("aa {d2}").ValueOf("ab 00");}));
			ViewResults("SuffixValueOf (invalid input 4)", TestException("ArgumentOutOfRangeException", () => {var num = new Suffix("aa {d2}").ValueOf("aa 0-");}));
			ViewResults("SuffixValueOf (oob digit)", TestException("ArgumentOutOfRangeException", () => {var num = new Suffix("aa {o2}").ValueOf("aa 09");}));

			ViewResults("SuffixValueOf", TestSuffixValueOf(345, "the number is: {b10}", "the number is: 0101011001"));
			ViewResults("SuffixValueOf", TestSuffixValueOf(987, "{o4} is the number", "1733 is the number"));
			ViewResults("SuffixValueOf", TestSuffixValueOf(90, "number {d3} here", "number 090 here"));
			ViewResults("SuffixValueOf", TestSuffixValueOf(244, "_{x3}", "_0f4"));
			ViewResults("SuffixValueOf", TestSuffixValueOf(244, "_{X3}", "_0F4"));

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
