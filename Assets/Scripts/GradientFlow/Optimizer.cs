using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Optimizer
{
    private Curve curve;
    private float length;
    private Loss loss;
    private Electricity electricity;
    private DiscreteMoebius discreteMoebius;
    private Elasticity elasticity;
    private float alpha = 0.9f;

    public Optimizer(Curve curve)
    {
        this.curve = curve;
        this.length = curve.positions.Count;
        this.loss = new Loss(curve.positions, 1e-08f);
        this.electricity = new Electricity(curve.positions, 1e-03f);
        this.discreteMoebius = new DiscreteMoebius(curve.positions, 1e-05f); // longitude 64, segment 0.03f -> 1e-05f
        this.elasticity = new Elasticity(curve.positions, 1.0f);
    }

    public void Flow()
    {
        Vector3[] newPositions = this.discreteMoebius.LineSearch();
        Debug.Log(this.discreteMoebius.Energy(this.curve.positions.ToArray()));

        for (int i = 0; i < this.length; i++)
        {
            this.curve.positions[i] = newPositions[i];
        }
    }

    // momentum SGD
    public void MomentumFlow()
    {
        Vector3[] gradient = this.discreteMoebius.Gradient();
        Debug.Log(this.discreteMoebius.Energy(this.curve.positions.ToArray()));

        for (int i = 0; i < this.length; i++)
        {
            this.curve.momentum[i] = this.alpha * this.curve.momentum[i] + gradient[i];
            this.curve.positions[i] += this.curve.momentum[i];
        }
    }
}