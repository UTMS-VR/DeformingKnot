using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

// 暗黙の仮定：隣接する2点の間隔は一定
public class DiscreteMoebius
{
    private Curve curve;
    private List<Vector3> pos;
    private int len;
    private float arc;
    private float seg;
    private float lr = 1e-05f; // longitude 64, segment 0.03f -> 1e-05f;
    private float alpha = 0.95f;
    private List<Vector3> gradient;

    public DiscreteMoebius(Curve curve)
    {
        this.curve = curve;
        this.pos = curve.positions;
        this.len = curve.positions.Count;
        this.arc = ArcLength();
        this.seg = this.arc / this.len;
        this.gradient = ModifiedGradient();
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
        float energy = AuxiliaryEnergy();
        List<Vector3> gradient = new List<Vector3>();

        for (int i = 0; i < this.len; i++)
        {
            Vector3 first = new Vector3(0, 0, 0);

            for (int j = 1; j < this.len; j++)
            {
                first += CoulombDiff(i, Sum(i, j));
            }

            first *= 2 * Mathf.Pow(this.seg, 2);

            Vector3 second = energy * 2 * this.seg * SegmentDiff(i);

            gradient.Add(this.lr * (first + second));
        }

        return gradient;
    }

    public float Energy()
    {
        float energy = AuxiliaryEnergy() * Mathf.Pow(this.seg, 2);

        return energy - Mathf.Pow(Mathf.PI, 2) * this.len / 3 + 4;
    }

    private float AuxiliaryEnergy()
    {
        float energy = 0.0f;

        for (int i = 0; i < this.len; i++)
        {
            for (int j = 1; j < this.len; j++)
            {
                energy += Coulomb(i, Sum(i, j));
            }
        }

        return energy;
    }

    public List<Vector3> ModifiedGradient()
    {
        float energy = AuxiliaryModifiedEnergy();
        List<Vector3> gradient = new List<Vector3>();

        for (int i = 0; i < this.len; i++)
        {
            Vector3 first = new Vector3();
            first += CoulombDiff(i, Succ(i));
            first += CoulombDiff(i, Pred(i));

            for (int j = 2; j <= this.len - 2; j++)
            {
                first += ModifiedCoulombDiff(i, Sum(i, j));
            }

            first *= 2 * Mathf.Pow(this.seg, 2);

            Vector3 second = energy * 2 * this.seg * SegmentDiff(i);

            gradient.Add(this.lr * (first + second));
        }

        return gradient;
    }

    public float ModifiedEnergy()
    {
        return AuxiliaryModifiedEnergy() * Mathf.Pow(this.seg, 2);
    }

    private float AuxiliaryModifiedEnergy()
    {
        float energy = 0.0f;

        for (int i = 0; i < this.len; i++)
        {
            energy += Coulomb(i, Succ(i));
            energy += Coulomb(i, Pred(i));

            for (int j = 2; j <= this.len - 2; j++)
            {
                energy += ModifiedCoulomb(i, Sum(i, j));
            }
        }

        return energy;
    }

    private float Coulomb(int i, int j)
    {
        return 1 / Mathf.Pow(Distance(i, j), 2);
    }

    private Vector3 CoulombDiff(int i, int j)
    {
        return - 2 * (this.pos[i] - this.pos[j]) / Mathf.Pow(Distance(i, j), 4);
    }

    private float ModifiedCoulomb(int i, int j)
    {
        float denom = Distance(i, j) - this.seg / Mathf.Sqrt(2);
        return 1 / Mathf.Pow(denom, 2);
    }

    private Vector3 ModifiedCoulombDiff(int i, int j)
    {
        float denom = Distance(i, j) - this.seg / Mathf.Sqrt(2);
        return - 2 * (DistanceDiff(i, j) - SegmentDiff(i) / Mathf.Sqrt(2)) / Mathf.Pow(denom, 3);
    }

    private float ArcLength()
    {
        float arc = 0.0f;

        for (int i = 0; i < this.len; i++)
        {
            arc += Vector3.Distance(this.pos[i], this.pos[Succ(i)]);
        }

        return arc;
    }

    private Vector3 SegmentDiff(int i)
    {
        return (DistanceDiff(i, Succ(i)) + DistanceDiff(i, Pred(i))) / this.len;
    }

    private float Distance(int i, int j)
    {
        return Vector3.Distance(this.pos[i], this.pos[j]);
    }

    private Vector3 DistanceDiff(int i, int j)
    {
        return (this.pos[i] - this.pos[j]).normalized;
    }

    private float ElasticEnergy()
    {
        float energy = 0.0f;

        for (int i = 0; i < this.len; i++)
        {
            energy += Mathf.Pow(Vector3.Distance(this.pos[i], this.pos[Succ(i)]) - this.seg, 2);
        }

        return energy;
    }

    private Vector3 ElasticForce(int i)
    {
        Vector3 next = 2 * (Vector3.Distance(this.pos[i], this.pos[Succ(i)]) - this.seg) * (DistanceDiff(i, Succ(i)) - SegmentDiff(i));
        Vector3 previous = 2 * (Vector3.Distance(this.pos[i], this.pos[Pred(i)]) - this.seg) * (DistanceDiff(i, Pred(i)) - SegmentDiff(i));
        return next + previous;
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