using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

public class EnergyTest : MonoBehaviour
{
    [SerializeField] private Material material;
    private int longitude = 64;
    int repeat = 10;

    private Curve curve1;
    private Curve curve2;

    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> positions1 = new List<Vector3>();
        List<Vector3> positions2 = new List<Vector3>();

        for (int i = 0; i < longitude; i++)
        {
            float t = (float)i / longitude;
            positions1.Add(ExampleCurve(t));
            positions2.Add(ExampleCurve(t));
        }

        curve1 = new Curve(false, false, false, true, positions1, Vector3.zero, Quaternion.identity);
        curve1.segment = AdjustParameter.ArcLength(curve1.positions, curve1.isClosed) / curve1.positions.Count;
        curve1.MeshAtPositionsUpdate();

        curve2 = new Curve(false, false, false, true, positions2, Vector3.zero, Quaternion.identity);
        curve2.segment = AdjustParameter.ArcLength(curve2.positions, curve2.isClosed) / curve2.positions.Count;
        curve2.MeshAtPositionsUpdate();

        // curve1.momentum = new List<Vector3>();

        /*for (int i = 0; i <= longitude; i++)
        {
            curve1.momentum.Add(new Vector3(0, 0, 0));
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        Graphics.DrawMesh(curve1.mesh, new Vector3(0, 0, 3), Quaternion.identity, material, 0);
        Graphics.DrawMesh(curve1.meshAtPositions, new Vector3(0, 0, 3), Quaternion.identity, MakeMesh.PositionMaterial, 0);
        Graphics.DrawMesh(curve2.mesh, new Vector3(0, 0, 3), Quaternion.identity, material, 0);
        // Graphics.DrawMesh(curve2.meshAtPositions, new Vector3(0, 0, 3), Quaternion.identity, MakeMesh.PositionMaterial, 0);

        if (Input.GetKey(KeyCode.Space))
        {
            for (int i = 0; i < repeat; i++)
            {
                // SGD.Step(curve1);
                KnotEnergy.Flow(curve1.positions);
                KnotEnergy.RestrictedFlow(curve2.positions);
                // AdjustParameter.EqualizeP(ref curve1.positions, curve1.segment, curve1.isClosed);
                // Debug.Log(curve1.positions.Count);
                // AdjustParameter.EqualizeL(ref curve1.positions, curve1.segment, curve1.positions.Count, curve1.isClosed);
                // AdjustParameter.EqualizeD(ref curve1.positions, curve1.positions.Count, curve1.isClosed);
                // AdjustParameter.Shift(ref curve1.positions, 7);
            }

            curve1.MeshUpdate();
            curve1.MeshAtPositionsUpdate();
            curve2.MeshUpdate();
            curve2.MeshAtPositionsUpdate();
        }
    }

    //circle
    private Vector3 ExampleCurve(float t)
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