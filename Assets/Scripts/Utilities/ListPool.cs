/**
 * File Name: ListPool.cs
 * Description: This script is for a static reusable pool containing a stack of lists of generic type
 * 
 * Authors: Catlike Coding, Will Lacey
 * Date Created: September 24, 2020
 * 
 * Additional Comments: 
 *      The original version of this file can be found here: https://catlikecoding.com/unity/tutorials/hex-map/ within 
 *		Catlike Coding's tutorial series: Hex Map; this file has been updated it to better fit this repository
 *
 *		File Line Length: 120
 **/

using System.Collections.Generic;

/// <summary>
/// Static reusable pool containing a stack of lists
/// </summary>
public static class ListPool<T> 
{
	/************************************************************/
	#region Fields

	static Stack<List<T>> stack = new Stack<List<T>>();

	#endregion
	/************************************************************/
	#region Functions

	public static List<T> Get() 
	{
		if (stack.Count == 0) return new List<T>();
		else return stack.Pop();
	}

	public static void Add(List<T> list) 
	{
		list.Clear();
		stack.Push(list);
	}

	#endregion
	/************************************************************/
}