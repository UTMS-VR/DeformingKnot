using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGD
{
    private float lr;
    private float alpha;
    private Curve curve;

    public SGD(float lr = 1e-03f, float alpha = 0.9f)
    {
        this.lr = lr;
        this.alpha = alpha;
    }

    public (Curve, List<Vector3>) Step(Curve curve, List<Vector3> momentum)
    {
        this.curve = curve;
        int N = curve.GetLength();

        List<Vector3> CurrentPositions = this.curve.GetPositions();
        List<Vector3> NewPositions = new List<Vector3>();
        
        Loss loss = new Loss(this.curve);
        List<Vector3> grad = loss.Gradient();
        List<Vector3> _momentum = new List<Vector3>();

        // gradient descent
        for (int i = 0; i < N; i++)
        {
            Vector3 P = CurrentPositions[i];
            Vector3 DP = -this.alpha * momentum[i] + (1 - this.alpha) * this.lr * grad[i];
            _momentum.Add(DP);
            NewPositions.Add(P - DP);
        }

        Curve _curve = new Curve(NewPositions);
        return (_curve, _momentum);
    }
    
}
