using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using InputManager;

public class BezierTest : MonoBehaviour
{
    [Range(-10.0f, 10.0f)] public float x;
    [Range(-10.0f, 10.0f)] public float y;
    [Range(-10.0f, 10.0f)] public float z;
    private Vector3 origin = new Vector3(3, 0, 0);
    private BezierCurve bezierCurve;
    private HandCurve curve;

    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> controllPoints = new List<Vector3>();
        controllPoints.Add(new Vector3(0, 0, 0) + origin);
        controllPoints.Add(new Vector3(1, 0, 1) + origin);
        controllPoints.Add(new Vector3(0, 1, 1) + origin);
        controllPoints.Add(new Vector3(1, 1, 0) + origin);
        controllPoints.Add(new Vector3(x, y, z) + origin);

        bezierCurve = new BezierCurve(controllPoints);
        List<Vector3> points = MakeCurve(bezierCurve);
        
        float segment = AdjustParameter.ArcLength(points, false) / (points.Count - 1);
        curve = new HandCurve(points, false, segment: segment, radius: 0.01f);

        List<int> power = new List<int>();
        for (int i = 0; i < 100; i++)
        {
            int n = Last3DigitOfPower(i);
            /*foreach(int k in power)
            {
                if (n == k)
                {
                    //Debug.Log((i, n));
                    break;
                }
            }*/
            //power.Add(Last3DigitOfPower(i));
            if (n % 100 == i)
            {
                Debug.Log((i, n));
            }
        }
        Debug.Log(Last3DigitOfPower(100));
        Debug.Log(Last3DigitOfPower(386));
        Debug.Log(Last3DigitOfPower(387));
        Debug.Log(Last3DigitOfPower(388));
        Debug.Log(Last4DigitOfPower(500));
    }

    int Last3DigitOfPower (int i)
    {
        if (i == 0)
        {
            return 1;
        }
        else
        {
            return (Last3DigitOfPower(i - 1) * 3) % 1000;
        }
    }

    int Last4DigitOfPower (int i)
    {
        if (i == 0)
        {
            return 1;
        }
        else
        {
            return (Last4DigitOfPower(i - 1) * 3) % 10000;
        }
    }

    // Update is called once per frame
    void Update()
    {
        bezierCurve.ControlPoints[4] = new Vector3(x, y, z) + origin;
        curve.points = MakeCurve(bezierCurve);
        curve.MeshUpdate();
        Graphics.DrawMesh(curve.mesh, Vector3.zero, Quaternion.identity, MakeMesh.CurveMaterial, 0);
    }

    private List<Vector3> MakeCurve(BezierCurve bezierCurve)
    {
        int num = 20;
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < num; i++)
        {
            points.Add(bezierCurve.GetPosition((float)i / num));
        }

        return points;
    }

    private Vector3 Trefoil(float t)
    {
        float theta = 2 * Mathf.PI * t;
        float x = Mathf.Sin(theta) + 2 * Mathf.Sin(2 * theta);
        float y = Mathf.Cos(theta) - 2 * Mathf.Cos(2 * theta);
        float z = - Mathf.Sin(3 * theta);
        return new Vector3(x, y, z);
    }
}
