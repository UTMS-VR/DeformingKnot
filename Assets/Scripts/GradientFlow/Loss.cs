using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loss
{
    private List<Vector3> positions;
    private int length;
    private float lr;

    public Loss(List<Vector3> positions, float lr)
    {
        this.positions = positions;
        this.length = positions.Count;
        this.lr = lr;
    }

    public Vector3[] Gradient()
    {
        Vector3[] _gradient = new Vector3[this.length];

        for (int i = 0; i < this.length; i++)
        {
            Vector3[] integrand = Integrand(i);
            _gradient[i] = -this.lr * IntegralAlongCurve(integrand, 0, this.length);
        }

        return _gradient;
    }

    private Vector3[] Integrand(int i)
    {
        Vector3[] _integrand = new Vector3[this.length];

        for (int j = 0; j < this.length; j++)
        {
            _integrand[j] = Function(i, j);
        }

        return _integrand;
    }

    private Vector3 Function(int i, int j)
    {
        List<Vector3> p = this.positions;
        int N = this.length;
        Vector3[] t = Tangents();

        int k = (i + 1) % N;
        int l = (i + N - 1) % N;
        Vector3 _curvature = (t[k].normalized - t[i].normalized) * this.length;
        float _norm = (p[j] - p[i]).magnitude;

        if (i == j)
        {
            return new Vector3(0, 0, 0);
        }
        else
        {
            return (4.0f * Vector3.ProjectOnPlane(t[i], (p[j] - p[i])) / Mathf.Pow(_norm, 4)
                    - 2.0f * _curvature / (t[i].magnitude * Mathf.Pow(_norm, 2))) * t[j].magnitude;
        }
    }

    private Vector3 IntegralAlongCurve(Vector3[] integrand, int StartIndex, int EndIndex)
    {
        Vector3 _integral = new Vector3(0, 0, 0);
        int n = EndIndex - StartIndex;

        // trapezoidal methods
        for (int i = StartIndex; i < EndIndex; i++)
        {
            int k = (i != EndIndex - 1) ? i + 1 : StartIndex;
            _integral += (integrand[i] + integrand[k]) / ((float) 2 * n);
        }

        return _integral;
    }

    private Vector3[] Tangents()
    {
        Vector3[] tangents = new Vector3[this.length];

        for (int i = 0; i < this.length; i++)
        {
            tangents[i] = (this.positions[(i + 1) % this.length] - this.positions[i]) * this.length;
        }

        return tangents;
    }

    public float Energy()
    {
        float energy = 0.0f;

        for (int i = 0; i < this.length; i++)
        {
            for (int j = 0; j < this.length; j++)
            {
                energy += EnergyIntegrand(i, j) / Mathf.Pow(this.length, 2);
            }
        }

        return energy;
    }

    private float EnergyIntegrand(int i, int j)
    {
        if (i == j)
        {
            return 0.0f;
        }
        else
        {
            float first = 1.0f / (this.positions[j] - this.positions[i]).sqrMagnitude;
            float second = 1.0f / Mathf.Pow(Mathf.Min(ArcLength(i, j), ArcLength(j, i)), 2);

            return (first - second) * Tangents()[i].magnitude * Tangents()[j].magnitude;
        }
    }

    private float ArcLength(int i, int j)
    {
        float arc = 0.0f;

        for (int k = i; (k % this.length) != j; k++) 
        {
            arc += Vector3.Distance(this.positions[k % this.length], this.positions[(k + 1) % this.length]);
        }

        return arc;
    }
}
