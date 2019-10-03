using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpriteWave
{
	public class Collage : IPalette
	{
		private const int defCols = 16;

		private bool _readOnly;
		public bool ReadOnly { get { return _readOnly; } }

		private List<Tile> _tiles;
		public int nTiles { get { return _tiles.Count; } }

		private FileFormat _fmt;
		public FileFormat Format { get { return _fmt; } }

		private Tile _templ;
		public int TileW { get { return _templ.Width; } }
		public int TileH { get { return _templ.Height; } }

		private int _nCols;
		public int Columns { get { return _nCols; } }
		public int Rows
		{
			get
			{
				int rows = _tiles.Count / _nCols;
				if (_tiles.Count % _nCols != 0)
					rows++;

				return rows;
			}
		}

		public int Width { get { return Columns * TileW; } }
		public int Height { get { return Rows * TileH; } }

		private Bitmap _canvas;
		public Bitmap Bitmap { get { return _canvas; } }

		private Pen _gridPen;
		public Pen GridPen { get { return _gridPen; } }

		private readonly ColorTable _tbl;
		//public uint HighestColor { get { return _tbl.LastColor; } }

		private uint[] _nativeClrs;
		public uint[] NativeColors { get { return _nativeClrs; } }
		public int ColorCount { get { return _nativeClrs.Length; } }

		public uint this[int idx]
		{
			get { return Utils.RedBlueSwap(_tbl.NativeToRGBA(_nativeClrs[idx])); }
			set {
				_nativeClrs[idx] = _tbl.RGBAToNative(Utils.RedBlueSwap(value));
				UpdateGridPen();
			}
		}

		public uint[] GetList()
		{
			int len = _nativeClrs.Length;
			var clrs = new uint[len];
			for (int i = 0; i < len; i++)
				clrs[i] = this[i];

			return clrs;
		}

		public Collage(FileFormat fmt, int nCols = defCols, bool readOnly = true)
		{
			_fmt = fmt;
			_templ = _fmt.NewTile();

			_tiles = new List<Tile>();
			_nCols = nCols;
			_readOnly = readOnly;

			_tbl = _fmt.ColorTable;
			uint[] defs = _tbl.Defaults;
			_nativeClrs = new uint[defs.Length];
			Buffer.BlockCopy(defs, 0, _nativeClrs, 0, defs.Length * Utils.cLen);

			UpdateGridPen();
		}

		public byte[] BGRAPalette()
		{
			var clrs = new byte[_nativeClrs.Length * Utils.cLen];
			int idx = 0;
			for (int i = 0; i < _nativeClrs.Length; i++)
			{
				uint c = this[i];
				System.Diagnostics.Debug.WriteLine("this[{0}] = {1}", i, c.ToString("X8"));
				clrs[idx++] = (byte)(c >> 24);
				clrs[idx++] = (byte)((c >> 16) & 0xff);
				clrs[idx++] = (byte)((c >> 8) & 0xff);
				clrs[idx++] = (byte)(c & 0xff);
			}

			return clrs;
		}

		public void UpdateGridPen()
		{
			double red = 0, green = 0, blue = 0, alpha = 0;
			for (int i = 0; i < _nativeClrs.Length; i++)
			{
				uint clr = this[i];
				blue += (double)((clr >> 24) & 0xff);
				green += (double)((clr >> 16) & 0xff);
				red += (double)((clr >> 8) & 0xff);
				alpha += (double)(clr & 0xff);
			}

			double n = (double)_nativeClrs.Length;
			uint rgba =
				((uint)(red / n) & 0xff) << 24 |
				((uint)(green / n) & 0xff) << 16 |
				((uint)(blue / n) & 0xff) << 8 |
				(uint)(alpha / n) & 0xff
			;

			_gridPen = new Pen(Utils.FromRGB(rgba ^ 0xFFFFFF00));
		}

		public void AddTile(Tile t)
		{
			_tiles.Add(t);
		}

		public Tile TileAt(int idx)
		{
			return _tiles[idx];
		}
		public Tile TileAt(Position p)
		{
			return TileAt(p.row * _nCols + p.col);
		}

		public void SetTile(int idx, Tile t)
		{
			if (t == null)
				throw new ArgumentNullException();

			_tiles[idx] = t;
		}
		public void SetTile(Position p, Tile t)
		{
			SetTile(p.row * _nCols + p.col, t);
		}

		public void AddBlankTiles(int count)
		{
			for (int i = 0; i < count; i++)
				_tiles.Add(_fmt.NewTile());
		}

		// Each pixel is stored as four bytes, ordered B, G, R, then A
		public Bitmap RenderTile(Tile t)
		{
			return t.ToBitmap(BGRAPalette());
		}

		public bool LoadTiles(byte[] file, int offset)
		{
			if (file == null || offset < 0)
				return false;

			int idx = 0;
			while (offset < file.Length)
			{
				if (idx == _tiles.Count)
					_tiles.Add(_fmt.NewTile());

				int off = _tiles[idx].Import(file, offset);

				idx++;
				if (off <= offset)
					break;

				offset = off;
			}

			// 'idx' now represents the last tile index + 1, aka the true number of tiles
			if (_tiles.Count > idx)
				_tiles.RemoveRange(idx, _tiles.Count - idx);

			return true;
		}

		public void Render()
		{
			const int cLen = 4;

			int rows = Rows;
			int height = rows * TileH;
			int width = _nCols * TileW;
			byte[] collage = new byte[height * width * cLen];

			byte[] palBGRA = BGRAPalette();
			int idx = 0;
			foreach (Tile t in _tiles)
			{
				int tCol = idx % _nCols;
				int tRow = idx / _nCols;
				// tRow is inverted because BMPs are backwards
				tRow = rows - tRow - 1;

				int imgCol = tCol * TileW;
				int imgRow = tRow * TileH;

				int offset = (imgRow * _nCols * TileW) + imgCol;
				offset *= cLen;

				// Each pixel is stored as four bytes, ordered B, G, R, then A
				t.ApplyTo(collage, offset, width, palBGRA);
				idx++;
			}

			_canvas = Utils.BitmapFrom(collage, width, height);
		}

		public int InsertColumn(int idx)
		{
			if (idx < 0 || idx > _nCols)
				return 0;

			_nCols++;
			for (int i = 0; i < Rows; i++)
				_tiles.Insert(i * _nCols + idx, _fmt.NewTile());

			return 1;
		}

		public int InsertRow(int idx)
		{
			int rows = Rows;
			if (idx < 0 || idx > rows)
				return 0;

			for (int i = 0; i < _nCols; i++)
				_tiles.Insert(idx * _nCols + i, _fmt.NewTile());

			return 1;
		}

		public int DeleteColumn(int idx)
		{
			if (idx < 0 || idx >= _nCols || _nCols <= 1)
				return 0;

			for (int i = Rows - 1; i >= 0; i--)
				_tiles.RemoveAt(i * _nCols + idx);

			_nCols--;
			return -1;
		}

		public int DeleteRow(int idx)
		{
			int rows = Rows;
			if (idx < 0 || idx >= rows || rows <= 1)
				return 0;

			for (int i = _nCols - 1; i >= 0; i--)
				_tiles.RemoveAt(idx * _nCols + i);

			return -1;
		}
	}
}
