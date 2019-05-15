///-----------------------------------------------------------------
///   Struct:         Tools
///   Description:    Miscelaneous tools to help the execution of this task
///                   the tools here auxiliate 2d foreach processes
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
///
/// <summary>
/// this delegate is used by the foreach2d.
/// </summary>
/// <typeparam name="T">type of the collection</typeparam>
/// <param name="x">x index of the iterated object</param>
/// <param name="y">y index of the iterated object</param>
/// <param name="obj">ref to the object passing through the iteration</param>
public delegate void Foreach2DExplicitDelegate<T>(Coordinate c, ref T obj);

public struct Coordinate
{
	public int x, y;
	public Coordinate(int x = 0, int y = 0)
	{
		this.x = x;
		this.y = y;
	}
	public static Coordinate operator +(Coordinate first, Coordinate seccond)
	{
		Coordinate result = new Coordinate(first.x + seccond.x, first.y + seccond.y);
		return result;
	}
	public static Coordinate operator -(Coordinate first, Coordinate seccond)
	{
		Coordinate result = new Coordinate(first.x - seccond.x, first.y - seccond.y);
		return result;
	}
}


/// <summary>
/// this delegate is used by the foreach2d.
/// </summary>
/// <typeparam name="T">Type of the colection</typeparam>
/// <param name="obj">ref to the object passing through the iteration</param>
public delegate void Foreach2DImplicitDelegate<T>(ref T obj);

/// <summary>
/// Static class containing some facilitators wrote for this assingment
/// </summary>
public static class Tools
{
    /// <summary>
    /// This method facilitates the process of 2d foreaches,
    /// it no only exposes the coordinates of the iterate object for further use but
    /// also exposes the object itself as a ref (allowing me to manipulate the object itself)
    /// things that a normal foreach would not allow
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <param name="array">The collection</param>
    /// <param name="function">The function that will be operated in each object of the collection</param>
    public static void Foreach2D<T>(T[,] array, Foreach2DExplicitDelegate<T> function){
	
		for (int y = 0; y < array.GetLength(1); y++)
            for (int x = 0; x < array.GetLength(0); x++)
                function(new Coordinate(x,y), ref array[x, y]);
    }

    /// <summary>
    /// This version maps receive a 1d array but treats it as a 2d array
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <param name="array">The collection</param>
    /// <param name="function">The function that will be operated in each object of the collection</param>
    public static void Foreach2D<T>(T[ ] array,int size, Foreach2DExplicitDelegate<T> function){
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                function(new Coordinate(x, y), ref array[x * size + y]);
    }
    
    /// <summary>
    /// This version do not exposes the x and y of the iteration but it still makes possible to
    /// alter the iterated object (different from regular foreachs)
    /// </summary>
    /// <typeparam name="T">The type of the collection</typeparam>
    /// <param name="array">The collection</param>
    /// <param name="function">The function that will be operated in each object of the collection</param>
    public static void Foreach2D<T>(T[,] array, Foreach2DImplicitDelegate<T> function) {
        for (int y = 0; y < array.GetLength(1); y++)
            for (int x = 0; x < array.GetLength(0); x++)
                function(ref array[x, y]);
    }
}
