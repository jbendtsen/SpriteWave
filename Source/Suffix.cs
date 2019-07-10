using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteWave
{
	public class Suffix
	{
		private bool _insert = false;

		private string _preIns = null;
		private string _postIns = null;

		private int _fmtBase = 0;
		private int _nDigits = 0;
		private bool _upperCase = false;

		public Suffix(string fmt)
		{
			var subs = new[] { new { start = 0, len = 0} }.ToList();
			string ins = null;

			int insOff = -1;
			int level = 0;
			int idx = 0;
			foreach (char c in fmt)
			{
				if (c == '{')
				{
					if (level == 0)
						insOff = idx;

					level++;
				}
				if (c == '}')
				{
					if (level == 1 && insOff >= 0 && idx - insOff > 1)
					{
						if (insOff > 0)
							_preIns = fmt.Substring(0, insOff);

						ins = fmt.Substring(insOff + 1, idx - (insOff + 1));

						int post = idx + 1;
						if (post < fmt.Length - 1)
							_postIns = fmt.Substring(post);

						_insert = true;
					}

					level--;
				}

				idx++;
				if (_insert)
					break;
			}

			if (level != 0)
				throw new ArgumentException("Trailing curly-braces in suffix format");

			if (!_insert)
			{
				_preIns = fmt;
				return;
			}

			if (ins.Length <= 1)
				throw new ArgumentException("Suffix format is too short (must include a base type and a length, eg. d3)");

			switch (Char.ToLower(ins[0]))
			{
				case 'b':
					_fmtBase = 2;
					break;
				case 'o':
					_fmtBase = 8;
					break;
				case 'd':
					_fmtBase = 10;
					break;
				case 'x':
					_fmtBase = 16;
					break;
				default:
					throw new ArgumentException("Invalid base type '" + ins[0] + "' in suffix format \"" + ins + "\"");
			}

			_upperCase = Char.IsUpper(ins[0]);

			_nDigits = Convert.ToInt32(ins.Substring(1));
			if (_nDigits < 1)
				throw new ArgumentException("The suffix number must take at least one digit");
		}

		public string Generate(int num)
		{
			if (!_insert)
				return _preIns;

			if (num < 0)
				throw new ArgumentException("Negative numbers are not permitted in the suffix");

			int maxNum = (int)Math.Pow(_fmtBase, _nDigits) - 1;
			if (num > maxNum)
				throw new ArgumentException("The given number " + num + " exceeds the number limit of the given format (" + maxNum + ")");

			int n = num;
			string str = "";
			for (int i = 0; i < _nDigits; i++)
			{
				int d = n % _fmtBase;
				n /= _fmtBase;

				char c;
				if (d >= 10)
				{
					int a = _upperCase ? (int)'A' : (int)'a';
					c = Convert.ToChar(a - 10 + d);
				}
				else
					c = Convert.ToChar((int)'0' + d);

				str = c + str;
			}

			return _preIns + str + _postIns;
		}

		public int ValueOf(string str)
		{
			if (!_insert)
				throw new InvalidOperationException("This suffix does not contain a number insert (eg. {d3})");

			int preLen = _preIns != null ? _preIns.Length : 0;
			int postLen = _postIns != null ? _postIns.Length : 0;

			int len = preLen + _nDigits + postLen;
			if (str.Length != len)
				throw new ArgumentException(
					"The given string is the wrong size to be of this suffix\n" +
					"Expected: " + len + ", actual: " + str.Length
				);

			string pre = preLen > 0 ? str.Substring(0, preLen) : null;
			string post = postLen > 0 ? str.Substring(str.Length - postLen) : null;

			if (pre != _preIns || post != _postIns)
			{
				throw new ArgumentException(
					"The given string does not have the surrounding components of the suffix\n" +
					"(\"" + pre + "\" & \"" + post + "\" != \"" + _preIns + "\" & \"" + _postIns + "\")"
				);
			}

			int num = 0;
			for (int i = 0; i < _nDigits; i++)
			{
				num *= _fmtBase;

				char c = str[preLen + i];
				int d;
				if (c >= '0' && c <= '9')
					d = c - '0';
				else if (c >= 'A' && c <= 'Z')
					d = c - 'A' + 10;
				else if (c >= 'a' && c <= 'z')
					d = c - 'a' + 10;
				else
					throw new ArgumentOutOfRangeException("Character '" + c + "' at offset " + i + " is not an acceptable digit");

				if (d >= _fmtBase)
					throw new ArgumentOutOfRangeException("The number " + d + " inside the given string does not fit within this base (base-" + _fmtBase + ")");

				num += d;
			}

			return num;
		}

		public int[] ListOfValues(string[] strList)
		{
			if (strList == null)
				return null;

			var values = new List<int>();
			foreach (string str in strList)
			{
				int n;
				try {
					n = this.ValueOf(str);
					values.Add(n);
				}
				catch (Exception ex) {
					if (ex is InvalidOperationException)
						return null;

					if (ex is ArgumentNullException ||
					    ex is ArgumentException ||
					    ex is ArgumentOutOfRangeException)
					{
						continue;
					}
				}
			}

			if (values.Count < 1)
				return null;

			values.Sort();
			return values.ToArray();
		}
	}
}
