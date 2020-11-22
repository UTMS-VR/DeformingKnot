using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Matrix3
{
    private float[,] matrix;

    private Matrix3(float[,] matrix)
    {
        this.matrix = matrix;
    }

    public Matrix3(float x)
    {
        this.matrix = new float[3,3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (i == j) this.matrix[i, j] = x;
                else this.matrix[i, j] = 0.0f;
            }
        }
    }

    public Matrix3(Vector3 v, Vector3 w)
    {
        this.matrix = new float[3,3];
        this.matrix[0, 0] = v.x * w.x;
        this.matrix[0, 1] = v.x * w.y;
        this.matrix[0, 2] = v.x * w.z;
        this.matrix[1, 0] = v.y * w.x;
        this.matrix[1, 1] = v.y * w.y;
        this.matrix[1, 2] = v.y * w.z;
        this.matrix[2, 0] = v.z * w.x;
        this.matrix[2, 1] = v.z * w.y;
        this.matrix[2, 2] = v.z * w.z;
    }

    static public Matrix3 Add(Matrix3 a, Matrix3 b)
    {
        float[,] sum = new float[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                sum[i, j] = a.matrix[i, j] + b.matrix[i, j];
            }
        }

        return new Matrix3(sum);
    }

    public Matrix3 Mult(float x)
    {
        float[,] prod = new float[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                prod[i, j] = this.matrix[i, j] * x;
            }
        }

        return new Matrix3(prod);
    }

    public Vector3 Apply(Vector3 v)
    {
        Vector3 w = new Vector3();
        w.x = this.matrix[0, 0] * v.x + this.matrix[0, 1] * v.y + this.matrix[0, 2] * v.z;
        w.y = this.matrix[1, 0] * v.x + this.matrix[1, 1] * v.y + this.matrix[1, 2] * v.z;
        w.z = this.matrix[2, 0] * v.x + this.matrix[2, 1] * v.y + this.matrix[2, 2] * v.z;

        return w;
    }

    public Matrix3 Inverse()
    {
        float[,] inv = new float[3, 3];
        float determinant = this.matrix[0, 0] * this.matrix[1, 1] * this.matrix[2, 2]
                            + this.matrix[0, 1] * this.matrix[1, 2] * this.matrix[2, 0]
                            + this.matrix[0, 2] * this.matrix[1, 0] * this.matrix[2, 1]
                            - this.matrix[0, 0] * this.matrix[1, 2] * this.matrix[2, 1]
                            - this.matrix[0, 1] * this.matrix[1, 0] * this.matrix[2, 2]
                            - this.matrix[0, 2] * this.matrix[1, 1] * this.matrix[2, 0];
        // 正則じゃない時に処理する？
        inv[0, 0] = (this.matrix[1, 1] * this.matrix[2, 2] - this.matrix[1, 2] * this.matrix[2, 1]) / determinant;
        inv[0, 1] = -(this.matrix[1, 0] * this.matrix[2, 2] - this.matrix[1, 2] * this.matrix[2, 0]) / determinant;
        inv[0, 2] = (this.matrix[1, 0] * this.matrix[2, 1] - this.matrix[1, 1] * this.matrix[2, 0]) / determinant;
        inv[1, 0] = -(this.matrix[0, 1] * this.matrix[2, 2] - this.matrix[0, 2] * this.matrix[2, 1]) / determinant;
        inv[1, 1] = (this.matrix[0, 0] * this.matrix[2, 2] - this.matrix[0, 2] * this.matrix[2, 0]) / determinant;
        inv[1, 2] = -(this.matrix[0, 0] * this.matrix[2, 1] - this.matrix[0, 1] * this.matrix[2, 0]) / determinant;
        inv[2, 0] = (this.matrix[0, 1] * this.matrix[1, 2] - this.matrix[0, 2] * this.matrix[1, 1]) / determinant;
        inv[2, 1] = -(this.matrix[0, 0] * this.matrix[1, 2] - this.matrix[0, 2] * this.matrix[1, 0]) / determinant;
        inv[2, 2] = (this.matrix[0, 0] * this.matrix[1, 1] - this.matrix[0, 1] * this.matrix[1, 0]) / determinant;

        return new Matrix3(inv);
    }
}
