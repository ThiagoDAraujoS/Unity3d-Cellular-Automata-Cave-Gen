///-----------------------------------------------------------------
///   Struct:         SwapChain
///   Description:    SwapChain Design patern: 
///                   It contains 2 buffers holding the information of the map being generated.
///                   These buffers have a swapping mechanism to auxiliate the process of the cellular automata.
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CaveMapGenerator {
/*	public abstract class SwapChainBase {
		/// <summary>
		/// Other SwapChains linked to thisone
		/// </summary>
		List<SwapChainBase> linkedSwapChains = new List<SwapChainBase>();

		/// <summary>
		/// Set a swapchain to be linked with this one
		/// </summary>
		/// <param name="listener"></param>
		public void LinkTo(SwapChainBase listener ) {
			linkedSwapChains.Add(listener);
		}

		/// <summary>
		/// Base Swap Method
		/// </summary>
		public virtual void Swap() {
			foreach ( SwapChainBase sc in linkedSwapChains ) 
				sc.Swap();
		}
	}*/

	/// <summary>
	/// This struct holds a simple structure of buffers, 
	/// these buffers are the canvas for the celular automata
	/// </summary>
	public class Swapchain<T> : ICloneable, IEnumerable<T> {
		
		//-----------------------------------DATA------------------------------------
		/// <summary>
		/// Internal buffers
		/// </summary>
		private T[,]
			writeBuffer,
			readBuffer;

		//-------------------------DATA ACCESS PROPERTIES--------------------------
		/// <summary>
		/// Buffer indexer
		/// </summary>
		/// <param name="x">Coordinate x</param>
		/// <param name="y">Coorinate y</param>
		/// <param name="value">data written on the write buffer</param>
		/// <returns>data on the read buffer</returns>
		public T this[int x, int y] {
			get { return readBuffer[x, y]; }
			set { writeBuffer[x, y] = value; }
		}

		/// <summary>
		/// Buffer indexer
		/// </summary>
		/// <param name="c">coordinate</param>
		/// <param name="value">data written on the write buffer</param>
		/// <returns>data on the read buffer</returns>
		public T this[Coordinate c] {
			get { return readBuffer[c.x, c.y]; }
			set { writeBuffer[c.x, c.y] = value; }
		}

		/// <summary>
		/// Buffer bounds
		/// </summary>
		public int Size {
			get { return readBuffer.GetLength(0); }
		}

		/// <param name="c">given coodinate</param>
		/// <returns> if coordinate is outside buffer's bounds</returns>
		public bool IsCoordinateOutsideBounds( Coordinate c ) {
			return ( c.x < 0 || c.y < 0 || c.x >= Size || c.y >= Size );
		}

		/// <param name="c">given coodinate</param>
		/// <returns> if coordinate is within buffer's bounds</returns>
		public bool IsCoordinateWithinBounds( Coordinate c ) {
			return !IsCoordinateOutsideBounds(c);
		}

		//---------------------------------FUNCTIONS---------------------------------

		/// <summary>
		/// Swap read and write buffers
		/// </summary>
		public void Swap() {
			var auxiliarBuffer = writeBuffer;
			writeBuffer = readBuffer;
			readBuffer = auxiliarBuffer;
		}

		/// <summary>
		/// Get a copy of the read buffer to external operations
		/// </summary>
		public T[,] GetBufferCopy() {
			return readBuffer.Clone() as T[,];
		}

		/// <summary>
		/// Deep clone this swapchain
		/// </summary>
		/// <returns>a copy of this swapchain</returns>
		public object Clone() {
			return new Swapchain<T>(this);
		}

		//------------------------------READ AND WRITE-------------------------------
		/// <summary>
		/// Read the buffer
		/// </summary>
		public IEnumerator<T> GetEnumerator() {
			foreach ( T cell in this )
				yield return cell;
		}

		/// <summary>
		/// Read the buffer
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator() {
			for ( int y = 0; y < Size; y++ )
				for ( int x = 0; x < Size; x++ )
					yield return readBuffer[x, y];
		}

		/// <summary>
		/// Read the data inside the buffers
		/// </summary>
		/// <param name="function">Read iteration function, Coordinate = cell coordinate </param>
		public void Read( Action<Coordinate, T> function ) {
			Coordinate c = new Coordinate();
			for ( c.y = 0; c.y < Size; c.y++ )
				for ( c.x = 0; c.x < Size; c.x++ )
					function(c, this[c]);
		}

		/// <summary>
		/// Iterate the buffers and write data into the writeBuffer
		/// </summary>
		/// <param name="function">Iteration function, Coordinate = iterated coordinate, bool = cell value, return = saved data into the write buffer></param>
		public void Write( Func<Coordinate, T, T> function ) {
			T[,] writeref = writeBuffer;
			Read(( c, cell ) => writeref[c.x, c.y] = function(c, cell));
			Swap();
		}

		/// <summary>
		/// Iterate the buffers and write data into the writeBuffer
		/// </summary>
		/// <param name="function">Iteration function, bool = cell value, return = saved data into the write buffer></param>
		public void Write( Func<T, T> function ) {
			Write(( c, cell ) => function(cell));
		}

		/// <summary>
		/// Iterate the buffers and write data into the writeBuffer
		/// </summary>
		/// <param name="function">Iteration function, Coordinate = iterated coordinate, return = saved data into the write buffer></param>
		public void Write( Func<Coordinate, T> function ) {
			Write(( c, cell ) => function(c));
		}

		/// <summary>
		/// Iterate the buffers and write data into the writeBuffer
		/// </summary>
		/// <param name="function">Iteration function, return = saved data into the write buffer></param>
		public void Write( Func<T> function ) {
			Write(( c, cell ) => function());
		}

		/// <summary>
		/// Copy external buffer into the data structure. Buffers need to have the same size
		/// </summary>
		/// <param name="buffer">the buffer to be copied</param>
		public void Write( T[,] buffer ) {
			if ( buffer.GetLength(0) == Size ) {
				writeBuffer = buffer.Clone() as T[,];
				Swap();
			} else
				throw new Exception("Atempt to write an unmatching buffer into the swapchain");
		}

		/// <summary>
		/// Write data using a coordinate list as mask
		/// </summary>
		/// <param name="mask">mask as coordinate list</param>
		/// <param name="baseFunction">what should be written on the buffer</param>
		/// <param name="maskFunction">what should be written on the masked part of the buffer</param>
		public void Write( List<Coordinate> mask, Func<T> maskFunction, Func<T> baseFunction = null ) {
			T[,] writeref = writeBuffer;
			if ( baseFunction != null )
				Read(( c, cell ) => writeref[c.x, c.y] = baseFunction());
			else
				CopyReadToWrite();
			foreach ( Coordinate coordinate in mask )
				this[coordinate] = maskFunction();
			Swap();
		}

		/// <summary>
		/// Copy read buffer into the write buffer
		/// </summary>
		public void CopyReadToWrite() {
			writeBuffer = readBuffer.Clone() as T[,];
		}

		//-------------------------------CONSTRUCTORS-------------------------------
		/// <summary>
		/// Build a new buffer set with the given size
		/// </summary>
		/// <param name="size">square size of the buffer</param>
		public Swapchain( int size ) {
			writeBuffer = new T[size, size];
			readBuffer = new T[size, size];
		}

		/// <summary>
		/// Build a new buffer set with the given size
		/// </summary>
		/// <param name="size">square size of the buffer</param>
		public static implicit operator Swapchain<T>( int size ) {
			return new Swapchain<T>(size);
		}

		/// <summary>
		/// Build a new Swapchain using a buffer as base
		/// </summary>
		/// <param name="buffer">the base buffer</param>
		public Swapchain( T[,] buffer ) {
			writeBuffer = buffer.Clone() as T[,];
			readBuffer = buffer.Clone() as T[,];
		}

		/// <summary>
		/// Build a new Swapchain using a buffer as base
		/// </summary>
		/// <param name="buffer">the base buffer</param>
		public static implicit operator Swapchain<T>( T[,] buffer ) {
			return new Swapchain<T>(buffer);
		}

		/// <summary>
		/// Build a swapchain from another swapchain (clone)
		/// </summary>
		/// <param name="previousSwapChain">the base swapchain for clonnage</param>
		public Swapchain( Swapchain<T> previousSwapChain ) {
			writeBuffer = previousSwapChain.writeBuffer.Clone() as T[,];
			readBuffer = previousSwapChain.readBuffer.Clone() as T[,];
		}

		/// <summary>
		/// Build a swapchain from another swapchain (clone)
		/// </summary>
		/// <param name="previousSwapChain">the base swapchain for clonnage</param>
		public static implicit operator int( Swapchain<T> obj ) {
			return obj.Size;
		}


	}



}