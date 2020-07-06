using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elasticity
{

    private List<Vector3> positions;
    private int length;
    private float lr;

    public Elasticity(List<Vector3> positions, float lr)
    {
        this.positions = positions;
        this.length = positions.Count;
        this.lr = lr;
    }

    public Vector3[] Gradient(float segment)
    {
        int length = this.positions.Count;
        Vector3[] direction = new Vector3[length];

        for (int i = 0; i < this.length; i++)
        {
            Vector3 next = this.positions[(i + 1) % this.length] - this.positions[i];
            Vector3 previous = this.positions[(i + this.length - 1) % this.length] - this.positions[i];
            direction[i] = this.lr * (Spring(next, segment) + Spring(previous, segment));
        }

        return direction;
    }

    public float Energy(float segment)
    {
        float energy = 0.0f;

        for (int i = 0; i < this.length; i++)
        {
            Vector3 tangent = this.positions[(i + 1) % length] - this.positions[i];
            energy += Mathf.Pow(tangent.magnitude - segment, 2);
        }

        return energy;
    }

    private Vector3 Spring(Vector3 v, float segment)
    {
        return (v.magnitude - segment) * v / v.magnitude;
    }
}
