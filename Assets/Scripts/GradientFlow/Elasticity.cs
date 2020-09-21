using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

public class Elasticity
{
    private List<Vector3> pos;
    private List<Vector3> momentum;
    private int len;
    private float seg; 
    private float lr = 1e-02f;
    private float alpha = 0.95f;
    private List<Vector3> gradient;

    public Elasticity(List<Vector3> positions, List<Vector3> momentum, float segment)
    {
        this.pos = positions;
        this.len = positions.Count;
        this.seg = segment;
        this.gradient = Gradient();
    }

    public void Flow()
    {
        for (int i = 0; i < this.len; i++)
        {
            this.pos[i] -= this.gradient[i];
        }
    }

    // momentum SGD
    public void MomentumFlow()
    {
        for (int i = 0; i < this.len; i++)
        {
            this.momentum[i] = this.alpha * this.momentum[i] + this.gradient[i];
            this.pos[i] -= this.momentum[i];
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
