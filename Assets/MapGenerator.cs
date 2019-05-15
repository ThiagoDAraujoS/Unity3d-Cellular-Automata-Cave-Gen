using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct TileData {
	public int height;
	public int style;
	public int type;
}
public struct GlobalData {
	public int length;
};

public class MapGenerator : MonoBehaviour {
	public ComputeShader cShader;
	public ComputeBuffer outBuffer, globalBuffer;
	public AnimationCurve spreadCurve;
	private GlobalData globalData = new GlobalData() { length = 16 };
	private TileData[] tileData;


	public int Size { get { return globalData.length; } }
	private int TrueSize { get { return Size * Size; } }


	private int Index(int x, int y) {
		return x + ( y * Size );
	}
	private void InitializeData() {
		tileData = new TileData[TrueSize];

		float x, y,
			  rndPositionX = Random.Range(-50, 50),
			  rndPositionY = Random.Range(-50, 50),
			  rndDimention = 0.05f;

		Tools.Foreach2D(tileData, globalData.length, (Coordinate c, ref TileData data) => {
			x = ( ( (float)c.x / (float)Size ) - 0.5f ) * 1.5f;
			y = ( ( (float)c.y / (float)Size ) - 0.5f ) * 1.5f;

			if(Random.value < spreadCurve.Evaluate(Mathf.Sqrt(( x * x ) + ( y * y ))))
				data.type = 1;
			else
				data.type = 0;

			data.height = (int)( Mathf.PerlinNoise(
				( (float)c.x + rndPositionX ) * rndDimention,
				( (float)c.y + rndPositionY ) * rndDimention) * 256f );
		});
	}
	private void InitializeBuffers() {
		unsafe {
			outBuffer = new ComputeBuffer(TrueSize, sizeof(TileData));
			globalBuffer = new ComputeBuffer(1, sizeof(GlobalData));
		}
		outBuffer.SetData(tileData);
		globalBuffer.SetData(new GlobalData[]{globalData});
	}

	void Start() {
		InitializeData();
		InitializeBuffers();

		int kernel = cShader.FindKernel("Initialize");
		cShader.SetBuffer(kernel, "outBuffer", outBuffer);
		cShader.SetBuffer(kernel, "globalBuffer", globalBuffer);
		cShader.Dispatch(kernel, 2, 2, 1);
		outBuffer.GetData(tileData);
		outBuffer.Dispose();
		globalBuffer.Dispose();

		string debug = "";
		int idx, idy;

		for(int j = 0; j < Size; j++) {
			for(int i = 0; i < Size; i++) {
				idx = tileData[Index(i,j)].style;
				idy = tileData[Index(i, j)].height;
				debug += ((idx<10)?"0":"")+idx+"x"+((idy<10)?"0":"")+idy + " ";
			}
			debug += "\n";
		}


		Debug.Log(debug);
	}

}