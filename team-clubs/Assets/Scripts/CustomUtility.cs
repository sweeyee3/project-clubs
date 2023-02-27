using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomUtility
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

	public static float CalculateProjectileTime(Vector3 v, Vector3 g)
	{
		var h = -Mathf.Pow(v.y, 2) / (2 * g.y);
		var a = g.y;
		var b = 2 * v.y;
		var c = -2 * h;
		var b2m4ac = (Mathf.Pow(b, 2) - 4 * a * c);
		b2m4ac = b2m4ac < 0 ? 0 : b2m4ac;

		var t1 = (-b + Mathf.Sqrt(b2m4ac)) / (2 * a);
		var t2 = (-b - Mathf.Sqrt(b2m4ac)) / (2 * a);

		var t = (t1 > 0) ? t1 : (t2 > 0) ? t2 : 0;
		t *= 2;

		return t;
	}

	public static Vector3 CalculateProjectileVelocity(Vector3 u, Vector3 a, float t, bool isZ = true)
	{
		var ux = isZ ? u.z : u.x;
		var ax = isZ ? a.z : a.x;

		var uz = isZ ? u.x : u.z;
		var az = isZ ? a.x : a.z;

		var vx = ux + ax * t;
		var vy = u.y + a.y * t;
		var vz = uz + az * t;

		return isZ ? new Vector3(vz, vy, vx) : new Vector3(vx, vy, vz);
	}
}

