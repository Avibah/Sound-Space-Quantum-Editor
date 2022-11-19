using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Linq;
using Sound_Space_Editor.Properties;

namespace Sound_Space_Editor
{
	class ColorSequence
	{
		private readonly Color[] _colors;
		private int _index;


		public ColorSequence()
		{
			_colors = EditorWindow.Instance.NoteColors.ToArray();
		}

		public Color Next()
		{
			var color = _colors[_index];

			_index = (_index + 1) % _colors.Length;

			return color;
		}

		public void Reset()
		{
			_index = 0;
		}
	}
}