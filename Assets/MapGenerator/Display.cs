///-----------------------------------------------------------------
///   Class:          Display
///   Description:    Map editor display class, it generates
///                   a texture holding a map visualisation.
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using UnityEditor;
using UnityEngine;

namespace CaveMapGenerator {
	public class Display {
		/// <summary>
		/// The image stored inside this display
		/// </summary>
		public Texture2D Image { get; set; }

		/// <summary>
		/// Local color buffer
		/// </summary>
		private Color[] textureColors;

		/// <summary>
		/// The actual resolution of the display
		/// </summary>
		public int Resolution {
			get { return Image.width; }
			set {
				textureColors = new Color[value * value];
				Image = new Texture2D(value, value);
			}
		}

		/// <summary>
		/// Set the pixels at the display and summit them
		/// </summary>
		private void FlushTexture() {
			//set the buffer to the texture
			Image.SetPixels(0, 0, Resolution, Resolution, textureColors);

			//apply changes onto the texture
			Image.Apply();
		}

		/// <summary>
		/// This sets the display texture to full white, its used for initialization
		/// </summary>
		private void SetTextureToWhite() {
			for ( int i = 0; i < textureColors.Length; i++ )
				textureColors[i] = Color.white;

			FlushTexture();
		}

		public void UpdateDisplay( bool[,] map, float[,] height ) {
			int cx = 0, cy = 0;
			float i;
			float ratio = map.GetLength(0) / (float)Resolution;
			Tools.Foreach2D(textureColors, Resolution, ( Coordinate c, ref Color color ) => {
				cx = Mathf.FloorToInt(ratio * c.x);
				cy = Mathf.FloorToInt(ratio * c.y);
				i = Mathf.Lerp(0.3f, 1f, height[cx, cy]);
				i = Mathf.Floor(i * 10f) / 10f;
				color = ( map[cx, cy] ) ?
				  Color.black :
				  new Color(i, i, i);
			}  );

			FlushTexture();
		}

		public void UpdateDisplay( CaveMap map ) {
			int cx = 0, cy = 0;
			float i;
			float ratio = map.Size / (float)Resolution;
			Tools.Foreach2D(textureColors, Resolution, (Coordinate c, ref Color color) => {
				cx = Mathf.FloorToInt(ratio * c.x);
				cy = Mathf.FloorToInt(ratio * c.y);
				i = Mathf.Lerp(0.3f, 1f, map.GetHeightData(cx, cy));
				i = Mathf.Floor(i * 10f) / 10f;
				color = ( map.GetMapData(cx, cy) ) ?
				  Color.black :
				  new Color(i, i, i);
			});

			FlushTexture();
		}

		/// <summary>
		/// Initialize the display
		/// </summary>
		/// <param name="resolution">the initial resolution the display is going to use</param>
		public Display( int resolution ) {
			//set the resolution variable
			Resolution = resolution;

			//initialize the texture to white
			SetTextureToWhite();
		}

		public Rect GUIDisplay( float height, float width ) {
			//set display color to white
			GUI.color = Color.white;

			//the display port properties
			Rect displayPort = new Rect(1, height, width, width);

			//draw the texture
			EditorGUI.DrawPreviewTexture(displayPort, Image, null, ScaleMode.ScaleToFit, 1.0f);

			return displayPort;
		}

	}
}