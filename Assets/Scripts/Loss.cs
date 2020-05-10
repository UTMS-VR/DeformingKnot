using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loss
{
    private const float epsilon = 1e-03f;

    private Curve _curve;
    private int N;
    
    public Loss(Curve curve)
    {
        this._curve = curve;
        this.N = curve.GetLength();
    }

    public List<Vector3> Gradient()
    {
        List<Vector3> _gradient = new List<Vector3>();

        for (int i = 0; i < N - 1; i++)
        {
            List<Vector3> integrand = Integrand(i);
            _gradient.Add(IntegralAlongCurve(integrand, 0, N));
        }

        // knot case
        _gradient.Add(_gradient[0]);

        return _gradient;
    }

    private List<Vector3> Integrand(int i)
    {
        List<Vector3> _integrand = new List<Vector3>();
        for (int j = 0; j < N - 1; j++)
        {
            _integrand.Add(Function(i, j));
        }

        // knot case
        _integrand.Add(_integrand[0]);

        return _integrand;
    }

    private Vector3 Function(int i, int j)
    {
        List<Vector3> p = this._curve.GetPositions();
        List<Vector3> t = this._curve.GetTangents();

        int k = (i != N - 1) ? i + 1 : 1;
        int l = (i != 0) ? i - 1 : N - 2;
        Vector3 _curvature = (t[k].normalized - t[l].normalized) / epsilon;
        float _norm = (p[j] - p[i]).magnitude;

        if (i == j)
        {
            return new Vector3(0, 0, 0);
        }
        else
        {
            return OrthogonalProjection(t[i],
            (4 / (float) Mathf.Pow(_norm, 4)) * (p[j] - p[i]) - (2 / (float) Mathf.Pow(_norm, 2)) * _curvature);
        }
    }

    private Vector3 IntegralAlongCurve(List<Vector3> integrand, int StartIndex, int EndIndex)
    {
        Vector3 _integral = new Vector3(0, 0, 0);
        List<Vector3> tangents = this._curve.GetTangents();
        int n = EndIndex - StartIndex;

        // trapezoidal methods
        for (int i = StartIndex; i < EndIndex; i++)
        {
            float velocity = tangents[i].magnitude;
            int k = (i != EndIndex - 1) ? i + 1 : StartIndex;
            _integral += (integrand[i] + integrand[k]) * (velocity / ((float) 2 * n));
        }

        return _integral;
    }

    // U maps to the orthoganl projection of V
    private Vector3 OrthogonalProjection(Vector3 V, Vector3 U)
    {
        return U - (Vector3.Dot(V, U) / Mathf.Pow(V.magnitude, 2)) * V;
    }
}
