using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnotEnergy : MonoBehaviour
{
    // private Mesh mesh;
    [SerializeField] private Material material;

    private int longitude = 50;
    private int meridian = 20;
    private float radius = 0.1f;

    private Curve curve;
    // private Curve NewCurve;
    private List<Vector3> momentum;
    // private List<Vector3> NewMomentum;
    private SGD SGD;

    // private int n_longitude = 50; // number of vertices excluding the end point along longitude
    // private float scale = 2.0f; // scale of the curve

    private float lr = 1e-05f; // the learning rate
    private float alpha = 0.9f; // momentum

    // Start is called before the first frame update
    void Start()
    {
        SGD = new SGD(lr, alpha);

        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < longitude; i++)
        {
            float t = (float)i / longitude;
            positions.Add(DoubledUnknot(t));
        }

        // mesh = MakeMesh.GetMesh(positions, meridian, radius, true);

        curve = new Curve(false, false, false, true, positions, Vector3.zero, Quaternion.identity);

        momentum = new List<Vector3>();

        // initialize
        for (int i = 0; i <= longitude; i++)
        {
            momentum.Add(new Vector3(0, 0, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
        Graphics.DrawMesh(curve.mesh, Vector3.zero, Quaternion.identity, material, 0);

        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("aaaaaa");
            SGD.Step(curve, momentum);
            curve.MeshUpdate();
            Debug.Log(curve.positions[10]);
            Debug.Log(momentum[10]);
            // (NewCurve, NewMomentum) = SGD.Step(CurrentCurve, Momentum);
            // CurrentCurve = NewCurve;
            // Momentum = NewMomentum;

            // List<Vector3> tangents = CurrentCurve.GetTangents(true);
        }
    }

    private Vector3 Trefoil(float t)
    {
        float theta = 2 * Mathf.PI * t;
        float x = Mathf.Sin(theta) + 2 * Mathf.Sin(2 * theta);
        float y = Mathf.Cos(theta) - 2 * Mathf.Cos(2 * theta);
        float z = - Mathf.Sin(3 * theta);
        return new Vector3(x, y, z);
    }

    private Vector3 DoubledUnknot(float t) // t in [0, 1]
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
            z = -Mathf.Exp((float)-1 / (1 - Mathf.Pow((-4 * t + 3), 2)));
        }

        return new Vector3(x, y, z);
    }
}