using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGD
{
    private float lr;
    private float alpha;
    // private Curve curve;

    public SGD(float lr = 1e-03f, float alpha = 0.9f)
    {
        this.lr = lr;
        this.alpha = alpha;
    }

    public void Step(Curve curve, List<Vector3> momentum)
    {
        // Debug.Log("bbbbbbbbbbbbbbbb");

        // this.curve = curve;
        int N = curve.positions.Count;

        // List<Vector3> CurrentPositions = curve.positions;
        List<Vector3> newPositions = new List<Vector3>();
        
        Loss loss = new Loss(curve);
        List<Vector3> grad = loss.Gradient();
        List<Vector3> _momentum = new List<Vector3>();

        // gradient descent
        for (int i = 0; i < N; i++)
        {
            Vector3 P = curve.positions[i];
            Vector3 DP = -this.alpha * momentum[i] + (1 - this.alpha) * this.lr * grad[i];
            // Debug.Log(grad[i]);
            // Debug.Log((1 - this.alpha));
            // Debug.Log(this.lr);
            // Debug.Log((1 - this.alpha) * this.lr * grad[i]);
            _momentum.Add(DP);
            newPositions.Add(P - DP);
        }

        curve.positions = newPositions;

        for (int i = 0; i < N; i++)
        {
            momentum[i] = _momentum[i];
        }

        // Debug.Log(grad[10]);
        // Debug.Log(curve.positions[10]);
        // Debug.Log(momentum[10]);
    }
}