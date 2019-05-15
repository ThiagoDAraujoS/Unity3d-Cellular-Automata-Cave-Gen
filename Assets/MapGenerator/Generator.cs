///-----------------------------------------------------------------
///   Class:          Generator
///   Description:    It uses cellular automata to generate a cave complex
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using Bubble = System.Collections.Generic.List<Coordinate>;
namespace CaveMapGenerator {

	public class Generator {
		/// <summary>
		/// the swapchain containing the buffers that will receive the operations
		/// </summary>

		private Swapchain<bool> map = new Swapchain<bool>(128);
		private Swapchain<float> height = new Swapchain<float>(128);

		/// <summary>
		/// Return a copy of the read buffer stored inside the map
		/// </summary>
		public bool[,] GetMapBufferCopy() {
			return map.GetBufferCopy();
		}

		/// <summary>
		/// Return a copy of the read buffer stored inside the map
		/// </summary>
		/// 
		public float[,] GetHeightBufferCopy() {
			return height.GetBufferCopy();
		}

		/// <summary>
		/// Return the size of the map
		/// </summary>
		public int Size { get { return map.Size; } }

		/// <summary>
		/// Fill a buffer with random noise
		/// </summary>
		public void NewMap(int size, float initialDensity, AnimationCurve spreadCurve) {
			//reset the swapchain if needed
			if(size != map) {
				map = size;
				height = size;
			}
			map.Write((Coordinate c) => {
				float x = ( ( (float)c.x / (float)size ) - 0.5f ) * 1.5f;
				float y = ( ( (float)c.y / (float)size ) - 0.5f ) * 1.5f;
				return Random.value < spreadCurve.Evaluate(Mathf.Sqrt(( x * x ) + ( y * y )));
			});

			float rndPositionX = Random.Range(-50, 50);
			float rndPositionY = Random.Range(-50, 50);
			float rndDimention = 0.05f;
			height.Write((Coordinate c) => {
				return Mathf.PerlinNoise(
					( (float)c.x + rndPositionX ) * rndDimention,
					( (float)c.y + rndPositionY ) * rndDimention);
			});
		}

		/// <summary>
		/// Iterate through the cells to refine the cave
		/// </summary>
		public void Refine(float minThreshold, float maxThreshold) {
			int count;
			bool result;
			map.Write((coordinate, cell) => {
				count = CountAliveAdjacentCells(coordinate, 1);
				result = ( cell ) ? count >= minThreshold : count > maxThreshold;
				return result;
			});
			height.Write((coordinate, cell) => {
				return GetMass(1, coordinate);
			});
		}

		/// <summary>
		/// Find isles and remove them
		/// </summary>
		/// <param name="c">anchor cell for the bubble removal algorithm</param>
		public void RemoveIsle(Coordinate c) {
			bool[,] buffer = map.GetBufferCopy();
			Flood(ref buffer, c.x, c.y, !buffer[c.x, c.y]);
			map.Write(buffer);
		}

		/// <summary>
		/// Remove noise from the main chamber
		/// </summary>
		/// <param name="maxPilarSize">max noise size (to be considered  a noise)</param>
		public void RemovePillars(int maxPilarSize) {
			Bubble mask = new Bubble();
			List<Bubble> bubblelist = ListBubbles(true);
			foreach(Bubble pilar in bubblelist)
				if(pilar.Count <= maxPilarSize)
					mask.AddRange(pilar);
			map.Write(mask, () => false);
		}

		/// <summary>
		/// return the count of alive cells around a given coordinate
		/// </summary>
		/// <param name="c">coordinate</param>
		/// <param name="range">count range</param>
		/// <returns></returns>
		public int CountAliveAdjacentCells(Coordinate c, int range) {
			int result = 0;
			Coordinate auxCoord = new Coordinate();
			//run on a range X range matrix
			for(int j = -range; j <= range; j++)
				for(int i = -range; i <= range; i++) {

					//in-buffer coordinate
					auxCoord.x = i + c.x;
					auxCoord.y = j + c.y;

					//If its not middle cell
					if(!( i == 0 && j == 0 ) &&

					   //If coordinates are outside bounds   (outside bounds cells are count as filled)
					   ( map.IsCoordinateOutsideBounds(auxCoord) ||

						 //if the cell is "true"
						 map[auxCoord] ))

						//add 1 to the counter
						result++;
				}
			//return counter
			return result;
		}

		public float GetMass(int range, Coordinate c) {
			Coordinate auxCoord = new Coordinate();
			float result = 0;
			//run on a range X range matrix
			for(int j = -range; j <= range; j++)
				for(int i = -range; i <= range; i++) {
					auxCoord.x = i + c.x;
					auxCoord.y = j + c.y;
					if(height.IsCoordinateWithinBounds(auxCoord) &&
						 !map[auxCoord]) {
						result += height[auxCoord];
					}
				}

			return result / 8f;
		}

		public void InvertHeight() {
			height.Write((cell) => 1f - cell);
		}

		/// <summary>
		/// Read the buffers and identify 'island' bubbles 
		/// </summary>
		/// <returns>size of the chamber</returns>
		public int RemoveDisconectedChambers() {
			//this reference has multiple purposes, it is generaly used as an auxiliar reference.
			Bubble mainChamber = null;

			//get the biggest bubble found (the main cavern)
			foreach(var hole in ListBubbles(false))
				if(mainChamber == null || mainChamber.Count < hole.Count)
					mainChamber = hole;

			//if there isn't a bubble (it might be full black)
			if(mainChamber != null)
				map.Write(mainChamber, () => false, () => true);

			return ( mainChamber != null ) ? mainChamber.Count : 0;
		}

		/// <summary>
		/// Recursive method that floods the cave to discover bubbles in it.
		/// </summary>
		public void Flood(ref bool[,] buffer, int x, int y, bool target) {
			//if this cell is valid and it's empty
			if(!( x < 0 || y < 0 || x >= buffer.GetLength(0) || y >= buffer.GetLength(0) ) && buffer[x, y] != target) {

				//write fill it
				buffer[x, y] = target;

				//reverberate to adjacent cells
				Flood(ref buffer, x + 1, y, target);
				Flood(ref buffer, x - 1, y, target);
				Flood(ref buffer, x, y + 1, target);
				Flood(ref buffer, x, y - 1, target);
			}
		}


		/// <summary>
		/// Recursive method that floods the cave to discover holes in it, it fills a hole object that was given during the method invocation
		/// </summary>
		/// <param name="bubble">The hole that is going to be filled</param>
		private void Flood(ref bool[,] buffer, Bubble bubble, int x, int y, bool target) {
			//if this cell is valid and it's empty
			if(!( x < 0 || y < 0 || x >= buffer.GetLength(0) || y >= buffer.GetLength(0) ) &&
				buffer[x, y] != target) {
				//write fill it
				buffer[x, y] = target;

				//add its coordinate to the hole's list
				bubble.Add(new Coordinate(x, y));

				//reverberate to adjacent cells
				Flood(ref buffer, bubble, x + 1, y, target);
				Flood(ref buffer, bubble, x - 1, y, target);
				Flood(ref buffer, bubble, x, y + 1, target);
				Flood(ref buffer, bubble, x, y - 1, target);
			}
		}

		/// <summary>
		/// Builds a list of bubbles
		/// </summary>
		/// <param name="target">if the bubbles are negative or positive</param>
		/// <returns>a list of bubbles</returns>
		private List<Bubble> ListBubbles(bool target) {
			//Initialize a bubble list to hold every bubble this map has
			List<Bubble> bubbles = new List<Bubble>();

			//this reference has multiple purposes, it is generaly used as an auxiliar reference.
			Bubble auxBubble = null;

			//extract buffer from data structure for more complex operations.
			bool[,] buffer = map.GetBufferCopy();

			//extract bubble from buffer
			Tools.Foreach2D(buffer, (Coordinate c, ref bool cell) => {
				if(cell == target) {
					auxBubble = new Bubble();
					Flood(ref buffer, auxBubble, c.x, c.y, !target);
					if(auxBubble.Count > 0)
						bubbles.Add(auxBubble);
				}
			});

			return bubbles;
		}
	}
}
