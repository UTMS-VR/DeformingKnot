using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve
{
    private List<Vector3> ControlPoints;

    public BezierCurve(List<Vector3> Positions)
    {
        this.ControlPoints = Positions;
    }

    public Vector3 GetPosition(float t)
    {
        var NewPosition = new Vector3(0, 0, 0);
        int N = ControlPoints.Count;

        for (int i = 0; i < N; i++)
        {
            NewPosition += ControlPoints[i] * BernsteinBasis(N - 1, i, t);
        }

        return NewPosition;
    }

    private float BernsteinBasis(int n, int i, float t)
    {
        return Combination(n, i) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
    }

    private int Combination(int n, int k)
    {
        if (k == 0)
        {
            return 1;
        }
        if (n == 0)
        {
            return 0;
        }
        return n * Combination(n - 1, k - 1) / k;
    }
}
