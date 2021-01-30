using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InputManager;
using DrawCurve;

public class PullableCurve
{
    // private List<Vector3> initialPoints;
    private List<Vector3> pullablePoints;
    private List<Vector3> fixedPoints;
    private float distanceThreshold;
    private List<float> weights;
    private OculusTouch oculusTouch;
    // private Vector3 initialVRCpoint;
    private Vector3 controllerPosition;
    private int meridian;
    private float radius;
    // private Material material;
    bool closed;

    public PullableCurve(
        List<Vector3> pullablePoints,
        List<Vector3> fixedPoints,
        OculusTouch oculusTouch,
        // Material material,
        List<float> weights = null,
        int meridian = 20,
        float radius = 0.1f,
        bool closed = false,
        float distanceThreshold = -1)
    {
        this.oculusTouch = oculusTouch;
        this.controllerPosition = oculusTouch.GetPositionR();
        this.pullablePoints = pullablePoints;
        this.fixedPoints = fixedPoints;
        if (distanceThreshold <= 0)
        {
            this.distanceThreshold = PullableCurve.DistanceAverage(this.pullablePoints);
        }
        else
        {
            this.distanceThreshold = distanceThreshold;
        }        
        if (weights == null)
        {
            this.weights = PullableCurve.GetWeights(this.pullablePoints.Count);
        }
        else
        {
            if (weights.Count != this.pullablePoints.Count)
            {
                Debug.Log("PullableCurveのコンストラクタの引数において、weightsの長さとpullablePointsの長さが一致していません");
                throw new System.Exception("PullableCurveのコンストラクタの引数において、weightsとpullablePointsの長さが一致していません");
            }
            this.weights = weights;
        }        
        // this.material = material;
        this.meridian = meridian;
        this.radius = radius;
        this.closed = closed;
    }
    public static PullableCurve Line(Vector3 start, Vector3 end, int numPoints, OculusTouch oculusTouch)
    {
        var points = new List<Vector3>();
        for (int i = 0; i < numPoints; i++)
        {
            float t = (float)i / numPoints;
            Vector3 p = (1.0f - t) * start + t * end;
            points.Add(p);
        }
        return new PullableCurve(points, new List<Vector3>(), oculusTouch);
    }

    public List<Vector3> GetPoints()
    {
        return this.pullablePoints.Concat(this.fixedPoints).ToList();
    }

    public int GetCount()
    {
        return this.pullablePoints.Count;
    }

    private static float BumpFunction(float t)
    {
        float theta = Mathf.PI * (2 * t - 1);
        return (Mathf.Cos(theta) + 1) / 2;
    }

    public static List<float> GetWeights(int numPoints)
    {
        int n = numPoints - 1;
        return Enumerable.Range(0, numPoints).Select(i => PullableCurve.BumpFunction((float)i / n)).ToList();
    }

    public Mesh GetMesh()
    {
        return MakeMesh.GetMesh(this.GetPoints(), this.meridian, this.radius, this.closed);
    }

    public void Update(List<Curve> collisionCurves = null)
    {
        this.UpdatePoints(collisionCurves);
        this.NormalizePoints();
        //Mesh mesh = MakeMesh.GetMesh(this.points, this.meridian, this.radius, this.closed);
        //Mesh meshAtPoints = MakeMesh.GetMeshAtPoints(this.points, this.radius * 2.0f);
        //Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, MakeMesh.CurveMaterial, 0);
        //Graphics.DrawMesh(meshAtPoints, Vector3.zero, Quaternion.identity, MakeMesh.PointMaterial, 0);
    }

    void UpdatePoints(List<Curve> collisionCurves = null)
    {
        float epsilon = this.distanceThreshold * 0.2f;
        Vector3 controllerNewPosition = this.oculusTouch.GetPositionR();
        Vector3 vrControllerMove = controllerNewPosition - this.controllerPosition;
        if (epsilon < vrControllerMove.magnitude)
        {
            vrControllerMove = vrControllerMove.normalized * epsilon;
        }
        this.controllerPosition = controllerNewPosition;

        List<Vector3> newPullablePoints = new List<Vector3>();
        for (int i = 0; i < this.pullablePoints.Count; i++)
        {
            newPullablePoints.Add(this.pullablePoints[i] + vrControllerMove * this.weights[i]);
        }

        List<Vector3> newPoints = newPullablePoints.Concat(this.fixedPoints).ToList();
        if (newPoints.Count >= 4 && PullableCurve.MinSegmentDist(newPoints, true) <= epsilon) return;
        if (collisionCurves != null)
        {
            foreach (Curve curve in collisionCurves)
            {
                if (PullableCurve.CurveDistance(newPoints, curve.points, true, curve.closed) <= epsilon) return;
            }
        }
        this.pullablePoints = newPullablePoints;
    }

    public static float MinSegmentDist(List<Vector3> points, bool closed)
    {
        int n = points.Count;
        float min = SegmentDist.SSDist(points[0], points[1], points[2], points[3]);
        int endi = closed ? n - 3 : n - 4;

        for (int i = 0; i <= endi; i++)
        {
            int endj = (i == 0 || !closed) ? n - 2 : n - 1;
            for (int j = i + 2; j <= endj; j++)
            {
                float dist = SegmentDist.SSDist(points[i], points[i + 1], points[j], points[(j + 1) % n]);
                if (dist < min) min = dist;
            }
        }

        return min;
    }

    public static float CurveDistance(List<Vector3> points1, List<Vector3> points2, bool closed1, bool closed2)
    {
        float min = SegmentDist.SSDist(points1[0], points1[1], points2[0], points2[1]);
        int end1 = closed1 ? points1.Count - 1 : points1.Count - 2;
        int end2 = closed2 ? points2.Count - 1 : points2.Count - 2;

        for (int i = 0; i <= end1; i++)
        {
            for (int j = 0; j <= end2; j++)
            {
                float dist = SegmentDist.SSDist(points1[i], points1[(i + 1) % points1.Count], points2[j], points2[(j + 1) % points2.Count]);
                if (dist < min) min = dist;
            }
        }

        return min;
    }

    void NormalizePoints()
    {
        this.NormalizePoints_Remove();
        this.NormalizePoints_Add();
    }

    void NormalizePoints_Remove()
    {
        // float ave = this.DistanceAverage();
        var newPoints = new List<Vector3>();
        var newWeights = new List<float>();
        int n = this.pullablePoints.Count;
        float accumDist = 0;
        // i = 0
        newPoints.Add(this.pullablePoints[0]);
        newWeights.Add(this.weights[0]);
        // 1 <= i < n - 1
        for (int i = 1; i < n - 1; i++)
        {
            accumDist += Vector3.Distance(this.pullablePoints[i - 1], this.pullablePoints[i]);
            if (accumDist > this.distanceThreshold / 2)
            {
                newPoints.Add(this.pullablePoints[i]);
                newWeights.Add(this.weights[i]);
                accumDist = 0;
            }
        }
        // i = n - 1
        newPoints.Add(this.pullablePoints[n - 1]);
        newWeights.Add(this.weights[n - 1]);
        this.pullablePoints = newPoints;
        this.weights = newWeights;
    }

    void NormalizePoints_Add()
    {
        // float ave = this.DistanceAverage();
        var newPoints = new List<Vector3>();
        var newWeights = new List<float>();
        int n = this.pullablePoints.Count;
        // i = 0
        newPoints.Add(this.pullablePoints[0]);
        newWeights.Add(this.weights[0]);
        // i <= i < n
        for (int i = 1; i < n; i++)
        {
            if ( Vector3.Distance(this.pullablePoints[i - 1], this.pullablePoints[i]) > 2 * this.distanceThreshold)
            {
                Vector3 addedPoint = (this.pullablePoints[i - 1] + this.pullablePoints[i]) / 2;
                float addedWeight = (this.weights[i - 1] + this.weights[i]) / 2;
                newPoints.Add(addedPoint);
                newWeights.Add(addedWeight);
                newPoints.Add(this.pullablePoints[i]);
                newWeights.Add(this.weights[i]);
            }
            else
            {
                newPoints.Add(this.pullablePoints[i]);
                newWeights.Add(this.weights[i]);
            }
        }
        this.pullablePoints = newPoints;
        this.weights = newWeights;
    }

    private static float DistanceAverage(List<Vector3> points)
    {
        float distanceSum = 0;
        int n = points.Count;
        for (int i = 0; i < n; i++)
        {
            if (i < n - 1)
            {
                distanceSum += Vector3.Distance(points[i], points[i + 1]);
            }
            else
            {
                distanceSum += Vector3.Distance(points[n - 1], points[0]);
            }
        }
        return distanceSum / n;
    }

//    private float BumpFunction(float t)
//    {
//        if (t < 0.5f)
//        {
//            return 2 * t;
//        }
//        else
//        {
//            return 2 - 2 * t;
//        }
//    }
}
