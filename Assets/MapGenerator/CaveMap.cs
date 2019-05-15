///-----------------------------------------------------------------
///   Class:          CaveMap
///   Description:    Scriptable Object holding a data structure containing a map.
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using UnityEngine;

/// <summary>
/// Data structure wich holds a map's data
/// </summary>
public class CaveMap : ScriptableObject
{
	/// <summary>
	/// The map information stored in this object
	/// the map is stored in a id array form due the fact that 2d arrays are not serealized in unity
	/// </summary>
	[SerializeField]
	[HideInInspector]
	private bool[] map;

	[SerializeField]
	[HideInInspector]
	private float[] height;

	/// <summary>
	/// The lateral size of the map
	/// </summary>
	[SerializeField]
	private int size;

	/// <summary>
	/// Getter to the size element;
	/// </summary>
	public int Size { get { return size; } }

	public bool[] Map { get { return map; } }
	public float[] Height { get { return height; } }

	/// <summary>
	/// Indexer that provides a better get/set to the map object
	/// it maps d1 arrays into 2d arrays
	/// </summary>
	public bool GetMapData(int x, int y)
	{
		return map[x * size + y];
	}
	public void SetMapData(int x, int y, bool value)
	{
		map[x * size + y] = value;
	}

	public float GetHeightData(int x, int y)
	{
		return height[x * size + y];
	}
	public void SetHeightData(int x, int y, float value)
	{
		height[x * size + y] = value;
	}

	/// <summary>
	/// Initialize a map objct
	/// </summary>
	/// <param name="map">the map stored here</param>
	public void InitializeMap(bool[,] map, float[,] height)
	{
		size = map.GetLength(0);
		this.map = new bool[size * size];
		Tools.Foreach2D(map, (Coordinate c, ref bool cell) => this.map[c.x * size + c.y] = cell);

		this.height = new float[size * size];
		Tools.Foreach2D(height, (Coordinate c, ref float cell) => this.height[c.x * size + c.y] = cell);
	}
}

