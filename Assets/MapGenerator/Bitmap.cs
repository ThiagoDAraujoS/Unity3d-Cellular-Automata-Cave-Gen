using System;
using UnityEngine;

public struct Bitmap {
	/// <summary>
	/// This is the data stored in this container
	/// </summary>
	private byte[] data;

	/// <summary>
	/// the height of this bitmap
	/// </summary>
	public int Height { get; private set; }

	/// <summary>
	/// the width of this bitmap
	/// </summary>
	public int Width { get; private set; }

	/// <summary>
	/// initialize a new bitmap
	/// </summary>
	public Bitmap( int width, int height ) {
		Width = width;
		Height = height;
		int size = Mathf.CeilToInt((float)( width * height ) / 4.0f);
		data = new byte[width * height / 4];
	}

	/// <summary>
	/// Clone this data structure
	/// </summary>
	/// <returns>A clone</returns>
	public Bitmap Clone() {
		Bitmap clone = new Bitmap(Width, Height);
		for ( int i = 0; i < data.Length; i++ )
			clone.data[i] = data[i];
		return clone;
	}

	/// <summary>
	/// Wipes the data
	/// </summary>
	/// <param name="value">new value</param>
	public void Wipe( bool value ) {
		if ( value )
			for ( int i = 0; i < data.Length; i++ )
				data[i] = 1;
		else
			for ( int i = 0; i < data.Length; i++ )
				data[i] = 0;
	}

	/// <summary>
	/// 2d indexer for this bitmap
	/// </summary>
	/// <returns>if that bit is true or false</returns>
	public bool this[int x, int y] {
		get {
			int index = x * Height + y;
			return ( ( data[index / 4] & 1 << index % 4 ) == 1 << index % 4 );
		}
		set {
			int index = x * Height + y;
			data[( x * Height + y ) / 4] = (byte)( ( value ) ?
			    data[index / 4] | ( 1 << index % 4 ) :
			    data[index / 4] & ~( 1 << index % 4 ) );
		}
	}

}
