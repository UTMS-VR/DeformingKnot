using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 暗黙の仮定：隣接する2点の間隔は一定
public class DiscreteMoebius
{
    private List<Vector3> positions;
    private int length;
    private float lr;

    public DiscreteMoebius(List<Vector3> positions, float lr)
    {
        this.positions = positions;
        this.length = positions.Count;
        this.lr = lr;
    }

    public Vector3[] Gradient()
    {
        float arc = ArcLength(this.positions.ToArray());
        float energy = AuxiliaryEnergy(this.positions.ToArray());
        Vector3[] gradient = new Vector3[this.length];

        for (int i = 0; i < this.length; i++)
        {
            Vector3 first = new Vector3(0, 0, 0);

            for (int j = 0; j < this.length; j++)
            {
                if (j != i)
                {
                    first += 4 * Mathf.Pow(arc / this.length, 2) * Coulomb(this.positions[i], this.positions[j]);
                }
            }

            Vector3 next = this.positions[(i + 1) % this.length] - this.positions[i];
            Vector3 previous = this.positions[(i + this.length - 1) % this.length] - this.positions[i];
            Vector3 second = 2 * arc * energy / Mathf.Pow(this.length, 2) * (next.normalized + previous.normalized);

            gradient[i] = this.lr * (first + second);
        }

        return gradient;
    }

    /*public Vector3[] LineSearch()
    {
        float alpha = 1.0f;
        float epsilon = 1e-05f;
        Vector3[] gradient = Gradient();
        float energy = Energy(this.positions.ToArray());
        float norm = SequentialNorm(gradient);

        Vector3[] newPositions = new Vector3[this.length];

        while (true)
        {
            for (int i = 0; i < this.length; i++)
            {
                newPositions[i] = this.positions[i] + alpha * gradient[i];
            }

            if (Energy(newPositions) < energy - epsilon * alpha * Mathf.Pow(norm, 2))
            {
                break;
            }

            alpha /= 1.2f;
        }

        Debug.Log(alpha);

        return newPositions;
    }*/

    public float Energy(Vector3[] sequence)
    {
        float arc = ArcLength(sequence);
        float energy = Mathf.Pow(arc / this.length, 2) * AuxiliaryEnergy(sequence);

        return energy - Mathf.Pow(Mathf.PI, 2) * this.length / 3.0f + 4.0f;
    }

    private float AuxiliaryEnergy(Vector3[] sequence)
    {
        float energy = 0.0f;

        for (int i = 0; i < this.length; i++)
        {
            for(int j = 0; j < this.length; j++)
            {
                if (i != j)
                {
                    energy += 1.0f / (sequence[i] - sequence[j]).sqrMagnitude;
                }
            }
        }

        return energy;
    }

    private Vector3 Coulomb(Vector3 v1, Vector3 v2)
    {
        return (v1 - v2) / Mathf.Pow((v1 - v2).sqrMagnitude, 2);
    }

    public float ArcLength(Vector3[] sequence)
    {
        float arc = 0.0f;

        for (int i = 0; i < this.length; i++)
        {
            arc += Vector3.Distance(sequence[i], sequence[(i + 1) % this.length]);
        }

        return arc;
    }

    private float SequentialNorm(Vector3[] sequence)
    {
        float _sum = 0.0f;

        for (int i = 0; i < this.length - 1; i++)
        {
            _sum += sequence[i].sqrMagnitude;
        }

        return Mathf.Sqrt(_sum);
    }
}