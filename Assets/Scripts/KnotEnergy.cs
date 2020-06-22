using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnotEnergy : MonoBehaviour
{
    [SerializeField] private Material material;
    private int longitude = 50;

    private Curve curve;

    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < longitude; i++)
        {
            float t = (float)i / longitude;
            positions.Add(Circle(t));
        }

        curve = new Curve(false, false, false, true, positions, Vector3.zero, Quaternion.identity);
        curve.segment = curve.ArcLength() / curve.positions.Count;
        curve.MeshAtPositionsUpdate();

        // curve.momentum = new List<Vector3>();

        /*for (int i = 0; i <= longitude; i++)
        {
            curve.momentum.Add(new Vector3(0, 0, 0));
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        Graphics.DrawMesh(curve.mesh, new Vector3(0, 0, 3), Quaternion.identity, material, 0);
        Graphics.DrawMesh(curve.meshAtPositions, new Vector3(0, 0, 3), Quaternion.identity, MakeMesh.PositionMaterial, 0);

        if (Input.GetKey(KeyCode.Space))
        {
            // Debug.Log("optimize");
            curve.positions = FlowAlongGradient.Optimize(curve.positions);
            // SGD.Step(curve);
            Debug.Log("a" + curve.DivisionNumber());
            curve.ParameterExchange();
            Debug.Log("b" + curve.positions.Count);
            curve.MeshUpdate();
            curve.MeshAtPositionsUpdate();
        }
    }

    //circle
    private Vector3 Circle(float t)
    {
        float theta = 2 * Mathf.PI * (t + 0.3f);
        float x = 3 * Mathf.Cos(theta);
        float y = 3 * Mathf.Sin(theta);
        return new Vector3(x, y, 0);
    }

    //trefoil
    private Vector3 ExampleCurve0(float t)
    {
        float theta = 2 * Mathf.PI * t;
        float x = Mathf.Sin(theta) + 2 * Mathf.Sin(2 * theta);
        float y = Mathf.Cos(theta) - 2 * Mathf.Cos(2 * theta);
        float z = - Mathf.Sin(3 * theta);
        return new Vector3(x, y, z);
    }

    // trefoil knot on a torus
    private Vector3 ExampleCurve1(float t) // t in [0, 1]
    {
        float theta = 2 * Mathf.PI * t;
        float x = (2 + Mathf.Cos(3 * theta)) * Mathf.Cos(2 * theta);
        float y = (2 + Mathf.Cos(3 * theta)) * Mathf.Sin(2 * theta);
        float z = Mathf.Sin(3 * theta);
        return new Vector3(x, y, z);
    }

    // ellipse
    private Vector3 ExampleCurve2(float t) // t in [0, 1]
    {
        float theta = 2 * Mathf.PI * t;
        float x = 3 * Mathf.Cos(theta);
        float y = 2 * Mathf.Sin(theta);
        float z = 0;
        return new Vector3(x, y, z);
    }

    // unknot + perturbation
    private Vector3 ExampleCurve3(float t) // t in [0, 1]
    {
        float theta = 2 * Mathf.PI * t;
        float x = 2 * Mathf.Cos(theta) + 2 * Mathf.Exp((float)-1 / (1 - Mathf.Pow((2 * t - 1), 2)));
        float y = 2 * Mathf.Sin(theta);
        float z = 0;
        return new Vector3(x, y, z);
    }

    // "figure-8" unknot
    private Vector3 ExampleCurve4(float t) // t in [0, 1]
    {
        float theta = 2 * Mathf.PI * t;
        float x = 2 * Mathf.Cos(theta) / (1 + Mathf.Pow(Mathf.Sin(theta), 2));
        float y = 2 * Mathf.Cos(theta) * Mathf.Sin(theta) / (1 + Mathf.Pow(Mathf.Sin(theta), 2));
        float z;

        if (t <= 0.5f)
        {
            z = Mathf.Exp((float)-1 / (1 - Mathf.Pow((4 * t - 1), 2)));
        }
        else
        {
            z = -Mathf.Exp((float)-1 / (1 - Mathf.Pow((-4 * t + 3), 2)));
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
            x = 2 * Mathf.Cos(2 * theta);
            y = 2 * Mathf.Sin(2 * theta);
        }
        else
        {
            x = Mathf.Cos(2 * theta) - 1;
            y = Mathf.Sin(2 * theta);
        }

        if (t <= 0.5f)
        {
            z = Mathf.Exp((float)-1 / (1 - Mathf.Pow((4 * t - 1), 2)));
        }
        else
        {
            z = - Mathf.Exp((float)-1 / (1 - Mathf.Pow((-4 * t + 3), 2)));
        }

        return new Vector3(x, y, z);
    }
}