using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnotEnergy
{
    private static float lr = 1.0f; // learning rate

    public static List<Vector3> Flow(List<Vector3> positions)
    {
        int length = positions.Count;

        List<Vector3> _positions = new List<Vector3>();

        for (int i = 0; i < length; i++)
        {
            _positions.Add(positions[i] + lr * Gradient(positions, i) / Mathf.Pow(length, 2));
        }

        return _positions;
    }

    private static Vector3 Gradient(List<Vector3> positions, int i)
    {
        Vector3 gradient = new Vector3(0, 0, 0);

        for (int j = 0; j < positions.Count; j++)
        {
            if (j != i)
            {
                gradient += Coulomb(positions[i], positions[j]);
            }
        }

        return gradient;
    }

    private static Vector3 Coulomb(Vector3 v1, Vector3 v2)
    {
        return (v1 - v2) / Mathf.Pow((v1 - v2).sqrMagnitude, 2);
    }
}
