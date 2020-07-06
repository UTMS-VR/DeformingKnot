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
        float arc = ArcLength();
        float energy = AuxiliaryEnergy();
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

    public Vector3[] RestrictedGradient()
    {
        Vector3[] gradient = Gradient();
        Vector3[][] matrix = ONRestrictionMatrix();

        float[] product = new float[this.length - 1];

        for (int i = 0; i < this.length - 1; i++)
        {
            product[i] = SequentialInnerProduct(matrix[i], gradient);
        }

        for (int j = 0; j < this.length; j++)
        {
            for (int i = 0; i < this.length - 1; i++)
            {
                gradient[j] -= product[i] * matrix[i][j];
            }
        }

        return gradient;
    }

    public float Energy()
    {
        float arc = ArcLength();
        float energy = Mathf.Pow(arc / this.length, 2) * AuxiliaryEnergy();

        return energy - Mathf.Pow(Mathf.PI, 2) * this.length / 3.0f + 4.0f;
    }

    private float AuxiliaryEnergy()
    {
        float energy = 0.0f;

        for (int i = 0; i < this.length; i++)
        {
            for(int j = 0; j < this.length; j++)
            {
                if (i != j)
                {
                    energy += 1.0f / (this.positions[i] - this.positions[j]).sqrMagnitude;
                }
            }
        }

        return energy;
    }

    private Vector3 Coulomb(Vector3 v1, Vector3 v2)
    {
        return (v1 - v2) / Mathf.Pow((v1 - v2).sqrMagnitude, 2);
    }

    public float ArcLength()
    {
        float arc = 0.0f;

        for (int i = 0; i < length; i++)
        {
            arc += Vector3.Distance(positions[i], positions[(i + 1) % this.length]);
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

    private float SequentialInnerProduct(Vector3[] sequence1, Vector3[] sequence2)
    {
        float _sum = 0.0f;

        for (int i = 0; i < this.length - 1; i++)
        {
            _sum += Vector3.Dot(sequence1[i], sequence2[i]);
        }

        return _sum;
    }

    private void SequentialNormalize(Vector3[] sequence)
    {
        float norm = SequentialNorm(sequence);

        for (int i = 0; i < this.length; i++)
        {
            sequence[i] = sequence[i] / norm;
        }
    }

    private Vector3[][] RestrictionMatrix()
    {
        Vector3[][] _matrix = new Vector3[this.length - 1][];

        for (int i = 0; i < this.length - 1; i++)
        {
            _matrix[i] = new Vector3[this.length];

            for (int j = 0; j < this.length; j++)
            {
                _matrix[i][j] = Vector3.zero;
            }
        }

        for (int i = 0; i < this.length - 1; i++)
        {
            _matrix[i][i] = this.positions[i + 1] - this.positions[i];
            _matrix[i][i + 1] = this.positions[i] - this.positions[(i + 2) % this.length];
            _matrix[i][(i + 2) % this.length] = this.positions[(i + 2) % this.length] - this.positions[i + 1];
        }

        return _matrix;
    }

    private Vector3[][] ONRestrictionMatrix()
    {
        Vector3[][] _matrix = RestrictionMatrix();

        SequentialNormalize(_matrix[0]);
        SequentialNormalize(_matrix[1]);

        for (int i = 2; i < this.length - 2; i++)
        {
            float product1 = SequentialInnerProduct(_matrix[i - 2], _matrix[i]);
            float product2 = SequentialInnerProduct(_matrix[i - 1], _matrix[i]);

            for (int j = 0; j < this.length; j++)
            {
                _matrix[i][j] -= _matrix[i - 2][j] * product1 + _matrix[i - 1][j] * product2;
            }

            SequentialNormalize(_matrix[i]);
        }

        for (int i = 0; i < this.length - 2; i++)
        {
            float product = SequentialInnerProduct(_matrix[i], _matrix[this.length - 2]);

            for (int j = 0; j < this.length; j++)
            {
                _matrix[this.length - 2][j] -= _matrix[i][j] * product;
            }
        }

        SequentialNormalize(_matrix[this.length - 2]);

        return _matrix;
    }
}