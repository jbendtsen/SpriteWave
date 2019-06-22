using System;
using System.Collections.Generic;
using System.Drawing;

using System.Windows.Forms;

namespace SpriteWave
{
	public class Collage
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

		private Palette _palette;
		public uint MeanColour { get { return _palette.Mean; } }

		private Bitmap _canvas;
		public Bitmap Bitmap { get { return _canvas; } }

		public Collage(FileFormat fmt, int nCols = defCols, bool readOnly = true)
		{
			_fmt = fmt;
			_templ = _fmt.NewTile();
			_palette = _fmt.NewPalette();

			_tiles = new List<Tile>();
			_nCols = nCols;
			_readOnly = readOnly;
		}

		public void SetColour(int idx, uint tblClr)
		{
			_palette.SetColour(idx, tblClr);
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

		public Bitmap RenderTile(Tile t)
		{
			return t.ToBitmap(_palette);
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

				t.ApplyTo(collage, offset, width, _palette); 
				idx++;
			}

			_canvas = Utils.BitmapFrom(collage, width, height);
		}

		public void InsertColumn(int idx)
		{
			if (idx < 0 || idx > _nCols)
				return;

			_nCols++;
			for (int i = 0; i < Rows; i++)
				_tiles.Insert(i * _nCols + idx, _fmt.NewTile());
		}

		public void InsertRow(int idx)
		{
			int rows = Rows;
			if (idx < 0 || idx > rows)
				return;

			for (int i = 0; i < _nCols; i++)
				_tiles.Insert(idx * _nCols + i, _fmt.NewTile());
		}

		public void DeleteColumn(int idx)
		{
			if (idx < 0 || idx >= _nCols || _nCols <= 1)
				return;

			for (int i = Rows - 1; i >= 0; i--)
				_tiles.RemoveAt(i * _nCols + idx);

			_nCols--;
		}

		public void DeleteRow(int idx)
		{
			int rows = Rows;
			if (idx < 0 || idx >= rows || rows <= 1)
				return;

			for (int i = _nCols - 1; i >= 0; i--)
				_tiles.RemoveAt(idx * _nCols + i);
		}
	}
}
