using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

public class Constraint
{
    private Curve curve;
    private List<Vector3> pos;
    private int len;
    private float seg;
    private float intlr = 1e-12f;
    private float crosslr = 1e-10f;
    private float alpha = 0.95f;
    private List<Vector3> intGradient;
    private List<Vector3> crossGradient;

    public Constraint(Curve curve)
    {
        this.curve = curve;
        this.pos = curve.positions;
        this.len = curve.positions.Count;
        this.seg = curve.segment;
        this.intGradient = IntervalGradient();
        this.crossGradient = CrossingGradient();
    }

    public void Flow()
    {
        for (int i = 0; i < this.len; i++)
        {
            //this.curve.positions[i] -= this.intGradient[i] + this.crossGradient[i];
            this.curve.positions[i] -= this.intGradient[i];
        }
    }

    // momentum SGD
    public void MomentumFlow()
    {
        for (int i = 0; i < this.len; i++)
        {
            this.curve.momentum[i] = this.alpha * this.curve.momentum[i] + this.intGradient[i] + this.crossGradient[i];
            this.curve.positions[i] -= this.curve.momentum[i];
        }
    }

    public List<Vector3> IntervalGradient()
    {
        List<Vector3> gradient = new List<Vector3>();

        for (int i = 0; i < this.len; i++)
        {
            Vector3 next = IntervalErrorDiff(i, Succ(i));
            Vector3 previous = IntervalErrorDiff(i, Pred(i));
            gradient.Add(this.intlr * (next + previous));
        }

        return gradient;
    }

    public List<Vector3> CrossingGradient()
    {
        List<Vector3> gradient = new List<Vector3>();

        for (int i = 0; i < this.len; i++)
        {
            Vector3 v = new Vector3();

            for (int j = 2; j <= this.len - 2; j++)
            {
                v += this.crosslr * 2 * ModifiedCoulombDiff(i, Sum(i, j));
            }

            gradient.Add(v);
        }

        return gradient;
    }

    public float IntervalEnergy()
    {
        float energy = 0.0f;

        Debug.Log(this.seg);
        Debug.Log("start");

        for (int i = 0; i < this.len; i++)
        { 
            Debug.Log(Distance(i, Succ(i)) - this.seg);
            energy += IntervalError(i, Succ(i));
        }

        Debug.Log("end");

        float groundEnergy = this.len / Mathf.Pow(0.5f * this.seg, 2);
        Debug.Log(groundEnergy);

        return energy - groundEnergy;
    }

    public float CrossingEnergy()
    {
        float energy = 0.0f;

        for (int i = 0; i < this.len; i++)
        {
            for (int j = 2; j <= this.len - 2; j++)
            {
                energy += ModifiedCoulomb(i, Sum(i, j));
            }
        }

        return energy;
    }

    private float IntervalError(int i, int j)
    {
        return 1 / (Mathf.Pow(0.5f * this.seg, 2) - Mathf.Pow(Distance(i, j) - this.seg, 2));
    }

    private Vector3 IntervalErrorDiff(int i, int j)
    {
        float denom =  Mathf.Pow(0.1f * this.seg, 2) - Mathf.Pow(Distance(i, j) - this.seg, 2);
        return 2 * (Distance(i, j) - this.seg) * DistanceDiff(i, j) / Mathf.Pow(denom, 2);
    }

    private float ModifiedCoulomb(int i, int j)
    {
        float denom = Distance(i, j) - this.seg / Mathf.Sqrt(2);
        return 1 / Mathf.Pow(denom, 2);
    }

    private Vector3 ModifiedCoulombDiff(int i, int j)
    {
        float denom = Distance(i, j) - this.seg / Mathf.Sqrt(2);
        return - 2 * DistanceDiff(i, j) / Mathf.Pow(denom, 3);
    }

    private float Distance(int i, int j)
    {
        return Vector3.Distance(this.pos[i], this.pos[j]);
    }

    private Vector3 DistanceDiff(int i, int j)
    {
        return (this.pos[i] - this.pos[j]).normalized;
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
