using System;

namespace SpriteWave
{
	public enum FormatKind
	{
		NES, SNES
	}

	public class FileFormat
	{
		private Type _type;
		private ColourTable _table;
		private string _name;
		private string[] _exts;

		public string TypeString
		{
			get {
				string str = _type.ToString();
				int pos = str.LastIndexOf('.') + 1;
				return str.Substring(pos);
			}
		}

		public FileFormat(string n, Type t, string[] e, ColourTable c)
		{
			_type = t;
			_table = c;
			_name = n;
			_exts = e;
		}

		public ColourTable ColourTable { get { return _table; } }

		public string Filter
		{
			get {
				string flt = "";
				foreach (string e in _exts)
					flt += "*." + e + ";";

				flt = flt.Remove(flt.Length-1);

				return _name + " file (" + flt + ")|" + flt + "|";
			}
		}

		public Palette NewPalette()
		{
			return _table != null ? new Palette(_table) : null;
		}

		public Tile NewTile()
		{
			return Activator.CreateInstance(_type) as Tile;
		}
	}
}