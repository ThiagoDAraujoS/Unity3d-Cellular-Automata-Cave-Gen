using CaveMapGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour {

	public CaveMap caveMap;
	public GameObject floorPrefab;
	public float heightDisplace;
	// Use this for initialization
	void Start () {
		float height;
		Tools.Foreach2D(caveMap.Map, caveMap.Size, (Coordinate c, ref bool cell) => {
			if(!cell) {
				height =   ( Mathf.Floor(( Mathf.PerlinNoise(c.x / 30f, c.y / 30f) * 10f ) * 5.0f) / 5f );
				height+= ( ( Mathf.Floor(caveMap.GetHeightData(c.x, c.y) * 10f) / 10.0f ) * heightDisplace );
				Instantiate(floorPrefab, new Vector3(c.x, height, c.y) + transform.position, transform.rotation, transform);
			}
		});
	}

}
