using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

public class Elasticity
{
    private Curve curve;
    private List<Vector3> pos;
    private int len;
    private float seg;
    private float lr = 1e-02f;
    private float alpha = 0.95f;
    private List<Vector3> gradient;

    public Elasticity(Curve curve)
    {
        this.curve = curve;
        this.pos = curve.positions;
        this.len = curve.positions.Count;
        this.seg = curve.segment;
        this.gradient = Gradient();
    }

    public void Flow()
    {
        for (int i = 0; i < this.len; i++)
        {
            this.curve.positions[i] -= this.gradient[i];
        }
    }

    // momentum SGD
    public void MomentumFlow()
    {
        for (int i = 0; i < this.len; i++)
        {
            this.curve.momentum[i] = this.alpha * this.curve.momentum[i] + this.gradient[i];
            this.curve.positions[i] -= this.curve.momentum[i];
        }
    }

    public List<Vector3> Gradient()
    {
        List<Vector3> gradient = new List<Vector3>();

        for (int i = 0; i < this.len; i++)
        {
            Vector3 next = this.pos[i] - this.pos[Succ(i)];
            Vector3 previous = this.pos[i] - this.pos[Pred(i)];
            gradient.Add(this.lr * (Spring(next) + Spring(previous)));
        }

        return gradient;
    }

    public float Energy()
    {
        float energy = 0.0f;

        for (int i = 0; i < this.len; i++)
        {
            energy += Mathf.Pow(Vector3.Distance(this.pos[i], this.pos[Succ(i)]) - this.seg, 2) / 2;
        }

        return energy;
    }

    public float MaxError()
    {
        float max = 0.0f;

        for (int i = 0; i < this.len; i++)
        {
            float error = Vector3.Distance(this.pos[i], this.pos[Succ(i)]) - this.seg;

            if (max < error)
            {
                max = error;
            }
        }

        return max;
    }

    private Vector3 Spring(Vector3 v)
    {
        return (v.magnitude - this.seg) * v.normalized;
    }

        private int Succ(int i)
    {
        return Sum(i, 1);
    }

    private int Pred(int i)
    {
        return Sum(i, this.len - 1);
    }

    private int Sum(int i, int j)
    {
        return (i + j) % this.len;
    }
}
