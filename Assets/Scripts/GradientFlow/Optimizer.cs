using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

public class Optimizer
{
    private Curve curve;
    private float length;
    private Loss loss;
    private Electricity electricity;
    private DiscreteMoebius discreteMoebius;
    private Elasticity elasticity;
    private float alpha = 0.95f;

    public Optimizer(Curve curve)
    {
        this.curve = curve;
        this.length = curve.positions.Count;
        this.loss = new Loss(curve.positions, 1e-08f);
        this.electricity = new Electricity(curve);
        this.discreteMoebius = new DiscreteMoebius(curve);
        this.elasticity = new Elasticity(curve);
    }

    public void Flow()
    {
        List<Vector3> gradient = this.discreteMoebius.ModifiedGradient();
        Debug.Log(this.curve.ArcLength());
        Debug.Log(this.curve.positions.Count);
        Debug.Log(this.discreteMoebius.ModifiedEnergy());

        for (int i = 0; i < this.length; i++)
        {
            this.curve.positions[i] -= gradient[i];
        }
    }

    // momentum SGD
    public void MomentumFlow()
    {
        List<Vector3> gradient = this.discreteMoebius.Gradient();
        Debug.Log ("flow");
        Debug.Log(this.curve.segment);
        Debug.Log(this.curve.ArcLength() / this.curve.positions.Count);
        Debug.Log(this.elasticity.Energy());
        Debug.Log(this.elasticity.MaxError());

        for (int i = 0; i < this.length; i++)
        {
            this.curve.momentum[i] = this.alpha * this.curve.momentum[i] + gradient[i];
            this.curve.positions[i] -= this.curve.momentum[i];
        }
    }
}