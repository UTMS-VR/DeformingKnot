using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGD
{
    private static float lr = 1e-07f;
    private static float alpha = 0.9f;

    public static void Step(Curve curve)
    {
        int N = curve.positions.Count;

        List<Vector3> _positions = new List<Vector3>();

        Loss loss = new Loss(curve);
        List<Vector3> grad = loss.Gradient();

        for (int i = 0; i < N; i++)
        {
            _positions.Add(curve.positions[i] - lr * grad[i]);
        }

        curve.positions = _positions;
    }

    // momentum SGD
    /*public static void Step(Curve curve, List<Vector3> momentum)
    {
        int N = curve.positions.Count;

        List<Vector3> _positions = new List<Vector3>();
        List<Vector3> _momentum = new List<Vector3>();

        Loss loss = new Loss(curve);
        List<Vector3> grad = loss.Gradient();

        for (int i = 0; i < N; i++)
        {
            Vector3 P = curve.positions[i];
            Vector3 DP = - alpha * momentum[i] + (1 - alpha) * lr * grad[i];
            _positions.Add(P - DP);
            _momentum.Add(DP);
        }

        curve.positions = _positions;

        for (int i = 0; i < N; i++)
        {
            momentum[i] = _momentum[i];
        }
    }*/
}