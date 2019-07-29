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
		private ColorTable _table;
		private string _name;
		private string[] _exts;
		
		public string Name { get { return _name; } }

		public FileFormat(string n, Type t, string[] e, ColorTable c)
		{
			_type = t;
			_table = c;
			_name = n;
			_exts = e;
		}

		public ColorTable ColorTable { get { return _table; } }

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