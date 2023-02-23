using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityExtension
{
	public static List<T> Shuffle<T>(this List<T> list)
	{
		List<T> l = new List<T>();

		// populate temp list
		foreach (var item in list)
		{
			l.Add(item);
		}

		// Loops through array
		for (int i = l.Count - 1; i > 0; i--)
		{
			// Randomize a number between 0 and i (so that the range decreases each time)				
			int rnd = Random.Range(0, i+1);

			// Save the value of the current i, otherwise it'll overright when we swap the values
			T temp = l[i];

			// Swap the new and old values
			l[i] = l[rnd];
			l[rnd] = temp;
		}

		return l;
	}

	public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
	{
		Vector3 AB = b - a;
		Vector3 AV = value - a;
		return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
	}
}

