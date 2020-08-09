using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

public class EnergyTest : MonoBehaviour
{
    private int longitude = 64;
    int repeat = 1;
    private Curve curve;

    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < longitude; i++)
        {
            float t = (float)i / longitude;
            positions.Add(ExampleCurve1(t));
        }

        float segment = AdjustParameter.ArcLength(positions, true) / positions.Count;
        curve = new Curve(positions, true, segment);
        curve.MomentumInitialize();
        curve.MeshAtPositionsUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            for (int i = 0; i < repeat; i++)
            {
                Optimizer optimizer = new Optimizer(curve);
                optimizer.MomentumFlow();
            }

            curve.MeshUpdate();
            curve.MeshAtPositionsUpdate();
        }

        Graphics.DrawMesh(curve.mesh, new Vector3(0, 0, 0.3f), Quaternion.identity, MakeMesh.CurveMaterial, 0);
        Graphics.DrawMesh(curve.meshAtPositions, new Vector3(0, 0, 0.3f), Quaternion.identity, MakeMesh.PositionMaterial, 0);
    }

    //circle
    private Vector3 ExampleCurve(float t)
    {
        float theta = 2 * Mathf.PI * (t + 0.0f);
        float x = 0.2f * Mathf.Cos(theta);
        float y = 0.2f * Mathf.Sin(theta);
        return new Vector3(x, y, 0);
    }

    //trefoil
    private Vector3 ExampleCurve0(float t)
    {
        float theta = 2 * Mathf.PI * t;
        float x = 0.1f * Mathf.Sin(theta) + 0.2f * Mathf.Sin(2 * theta);
        float y = 0.1f * Mathf.Cos(theta) - 0.2f * Mathf.Cos(2 * theta);
        float z = - 0.1f * Mathf.Sin(3 * theta);
        return new Vector3(x, y, z);
    }

    // trefoil knot on a torus
    private Vector3 ExampleCurve1(float t) // t in [0, 1]
    {
        float theta = 2 * Mathf.PI * t;
        float x = 0.1f * (2 + Mathf.Cos(3 * theta)) * Mathf.Cos(2 * theta);
        float y = 0.1f * (2 + Mathf.Cos(3 * theta)) * Mathf.Sin(2 * theta);
        float z = 0.1f * Mathf.Sin(3 * theta);
        return new Vector3(x, y, z);
    }

    // ellipse
    private Vector3 ExampleCurve2(float t) // t in [0, 1]
    {
        float theta = 2 * Mathf.PI * t;
        float x = 0.2f * Mathf.Cos(theta);
        float y = 0.1f * Mathf.Sin(theta);
        float z = 0;
        return new Vector3(x, y, z);
    }

    // unknot + perturbation
    private Vector3 ExampleCurve3(float t) // t in [0, 1]
    {
        float theta = 2 * Mathf.PI * t;
        float x = 0.2f * Mathf.Cos(theta) + 0.2f * Mathf.Exp((float)-1 / (1 - Mathf.Pow((2 * t - 1), 2)));
        float y = 0.2f * Mathf.Sin(theta);
        float z = 0;
        return new Vector3(x, y, z);
    }

    // "figure-8" unknot
    private Vector3 ExampleCurve4(float t) // t in [0, 1]
    {
        float theta = 2 * Mathf.PI * t;
        float x = 0.2f * Mathf.Cos(theta) / (1 + Mathf.Pow(Mathf.Sin(theta), 2));
        float y = 0.2f * Mathf.Cos(theta) * Mathf.Sin(theta) / (1 + Mathf.Pow(Mathf.Sin(theta), 2));
        float z;

        if (t <= 0.5f)
        {
            z = 0.1f * Mathf.Exp((float)-1 / (1 - Mathf.Pow((4 * t - 1), 2)));
        }
        else
        {
            z = - 0.1f * Mathf.Exp((float)-1 / (1 - Mathf.Pow((-4 * t + 3), 2)));
        }

        return new Vector3(x, y, z);
    }

    // NTT
    private Vector3 ExampleCurve5(float t) // t in [0, 1]
    {
        float theta = 2 * Mathf.PI * t;
        float x;
        float y;
        float z;

        if (t <= 0.25f || 0.75f <= t)
        {
            x = 0.2f * Mathf.Cos(2 * theta);
            y = 0.2f * Mathf.Sin(2 * theta);
        }
        else
        {
            x = 0.1f * Mathf.Cos(2 * theta) - 0.1f;
            y = 0.1f * Mathf.Sin(2 * theta);
        }

        if (t <= 0.5f)
        {
            z = 0.1f * Mathf.Exp((float)-1 / (1 - Mathf.Pow((4 * t - 1), 2)));
        }
        else
        {
            z = - 0.1f * Mathf.Exp((float)-1 / (1 - Mathf.Pow((-4 * t + 3), 2)));
        }

        return new Vector3(x, y, z);
    }
}