using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnotEnergy
{
    private static float lr = 1.0f; // learning rate

    public static void RestrictedFlow(List<Vector3> positions) // 隣接する2点間の距離を保存するという制約付き
    {
        int length = positions.Count;
        Vector3[] gradients = RestrictedGradient(positions);

        for (int i = 0; i < length; i++)
        {
            positions[i] += lr * gradients[i] / Mathf.Pow(length, 2);
        }
    }

    public static void Flow(List<Vector3> positions)
    {
        int length = positions.Count;
        Vector3[] gradients = Gradient(positions);

        for (int i = 0; i < length; i++)
        {
            positions[i] += lr * gradients[i] / Mathf.Pow(length, 2);
        }
    }

    private static Vector3[] RestrictedGradient(List<Vector3> positions)
    {
        int length = positions.Count;
        Vector3[] gradient = Gradient(positions);
        Vector3[][] matrix = ONRestrictionMatrix(positions);

        float[] product = new float[length];

        for (int i = 0; i < length; i++)
        {
            product[i] = SequentialInnerProduct(matrix[i], gradient, length);
        }

        for (int j = 0; j < length; j++)
        {
            for (int i = 0; i < length; i++)
            {
                gradient[j] -= matrix[i][j];
            }
        }

        return gradient;
    }

    private static Vector3[] Gradient(List<Vector3> positions)
    {
        int length = positions.Count;
        Vector3[] gradient = new Vector3[length];

        for (int i = 0; i < length; i++)
        {
            gradient[i] = new Vector3(0, 0, 0);

            for (int j = 0; j < length; j++)
            {
                if (j != i)
                {
                    gradient[i] += Coulomb(positions[i], positions[j]);
                }
            }
        }

        return gradient;
    }

    private static Vector3 Tangent(List<Vector3> positions, int i)
    {
        int length = positions.Count;
        return positions[i] - positions[(i + 1) % length];
    }

    private static float SequentialNorm(Vector3[] sequence, int length)
    {
        float _sum = 0.0f;

        for (int i = 0; i < length - 1; i++)
        {
            _sum += sequence[i].sqrMagnitude;
        }

        return Mathf.Sqrt(_sum);
    }

    private static float SequentialInnerProduct(Vector3[] sequence1, Vector3[] sequence2, int length)
    {
        float _sum = 0.0f;

        for (int i = 0; i < length - 1; i++)
        {
            _sum += Vector3.Dot(sequence1[i], sequence2[i]);
        }

        return _sum;
    }

    private static void SequentialNormalize(Vector3[] sequence, int length)
    {
        float norm = SequentialNorm(sequence, length);

        for (int i = 0; i < length; i++)
        {
            sequence[i] = sequence[i] / Mathf.Sqrt(norm);
        }
    }

    private static Vector3[][] RestrictionMatrix(List<Vector3> positions)
    {
        int length = positions.Count;
        Vector3[][] _matrix = new Vector3[length][];

        for (int i = 0; i < length; i++)
        {
            _matrix[i] = new Vector3[length];

            for (int j = 0; j < length; j++)
            {
                _matrix[i][j] = Vector3.zero;
            }
        }

        for (int i = 0; i < length; i++)
        {
            _matrix[i][i] = Tangent(positions, i);
            _matrix[i][(i + 1) % length] = -Tangent(positions, i);
        }

        return _matrix;
    }

    private static Vector3[][] ONRestrictionMatrix(List<Vector3> positions)
    {
        int length = positions.Count;
        Vector3[][] _matrix = RestrictionMatrix(positions);

        SequentialNormalize(_matrix[0], length);

        for (int i = 1; i < length - 1; i++)
        {
            float product = SequentialInnerProduct(_matrix[i - 1], _matrix[i], length);

            for (int j = 0; j < length; j++)
            {
                _matrix[i][j] -= _matrix[i - 1][j] * product;
            }

            SequentialNormalize(_matrix[i], length);
        }

        for (int i = 0; i < length - 1; i++)
        {
            float product = SequentialInnerProduct(_matrix[i], _matrix[length - 1], length);

            for (int j = 0; j < length; j++)
            {
                _matrix[length - 1][j] -= _matrix[i][j] * product;
            }
        }

        SequentialNormalize(_matrix[length - 1], length);

        return _matrix;
    }

    private static Vector3 Coulomb(Vector3 v1, Vector3 v2)
    {
        return (v1 - v2) / Mathf.Pow((v1 - v2).sqrMagnitude, 2);
    }
}