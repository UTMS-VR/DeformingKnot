using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DebugUtil;
using DrawCurve;

public class PullableCurve
{
    // private List<Vector3> initialPoints;
    private List<Vector3> points;
    private float distanceThreshold;
    private List<float> weights;
    private VRController vrController;
    // private Vector3 initialVRCPosition;
    private Vector3 controllerPosition;
    private int meridian;
    private float radius;
    // private Material material;
    bool closed;

    public PullableCurve(
        List<Vector3> points,
        VRController vrController,
        // Material material,
        List<float> weights = null,
        int meridian = 20,
        float radius = 0.1f,
        bool closed = false,
        float distanceThreshold = -1)
    {
        this.vrController = vrController;
        this.controllerPosition = vrController.GetPosition();
        this.points = points;
        if (distanceThreshold <= 0)
        {
            this.distanceThreshold = PullableCurve.DistanceAverage(points);
        }
        else
        {
            this.distanceThreshold = distanceThreshold;
        }        
        if (weights == null)
        {
            this.weights = PullableCurve.GetWeights(this.points.Count);
        }
        else
        {
            if (weights.Count != this.points.Count)
            {
                Debug.Log("PullableCurveのコンストラクタの引数において、weightsとpointsの長さが一致していません");
                throw new System.Exception("PullableCurveのコンストラクタの引数において、weightsとpointsの長さが一致していません");
            }
            this.weights = weights;
        }        
        // this.material = material;
        this.meridian = meridian;
        this.radius = radius;
        this.closed = closed;
    }
    public static PullableCurve Line(Vector3 start, Vector3 end, int numPoints, VRController vrController)
    {
        var points = new List<Vector3>();
        for (int i = 0; i < numPoints; i++)
        {
            float t = (float)i / numPoints;
            Vector3 p = (1.0f - t) * start + t * end;
            points.Add(p);
        }
        return new PullableCurve(points, vrController);
    }

    public List<Vector3> GetPoints()
    {
        return this.points;
    }

    private static float BumpFunction(float t)
    {
        float theta = Mathf.PI * (2 * t - 1);
        return (Mathf.Cos(theta) + 1) / 2;
    }

    private static float GetWeight(int index, int numPoints, int start = 0, int end = -1)
    {
        if (end == -1)
        {
            end = numPoints;
        }
        if (start <= end)
        {
            if (index < start || index >= end)
            {
                return 0;
            }
            else
            {
                int i = index - start;
                int n = end - start;
                float t = (float)i / n;
                return PullableCurve.BumpFunction(t);
            }
        } 
        else
        {
            if (index <= end)
            {
                int i = (numPoints + index) - start;
                int n = (numPoints + end) - start;
                float t = (float)i / n;
                return PullableCurve.BumpFunction(t);
            } 
            else if (end < index && index < start)
            {
                return 0;
            }
            else
            {
                int i = index - start;
                int n = (numPoints + end) - start;
                float t = (float)i / n;
                return PullableCurve.BumpFunction(t);
            }
        }
    }

    public static List<float> GetWeights(int numPoints, int start = 0, int end = -1)
    {
        return Enumerable.Range(0, numPoints).Select(i => PullableCurve.GetWeight(i, numPoints, start, end)).ToList();
    }

    public Mesh GetMesh()
    {
        return MakeMesh.GetMesh(this.points, this.meridian, this.radius, this.closed);
    }

    public void Update(List<Curve> collisionCurves = null)
    {
        Vector3 controllerNewPosition = this.vrController.GetPosition();
        Vector3 vrControllerMove = controllerNewPosition - this.controllerPosition;
        this.controllerPosition = controllerNewPosition;
        List<Vector3> newPoints = this.UpdatePoints(vrControllerMove);

        bool noSelfIntersection = (this.points.Count <= 3)
                                    || (this.MinSegmentDist(newPoints, this.closed) > this.distanceThreshold * 0.2f);
        bool noIntersection = true;

        foreach (Curve curve in collisionCurves)
        {
            if (this.CurveDistance(newPoints, this.closed, curve) <= this.distanceThreshold * 0.1f)
            {
                noIntersection = false;
            }
        }

        if (noSelfIntersection && noIntersection)
        {
            this.points = newPoints;
            this.NormalizePoints();
        }
        //Mesh mesh = MakeMesh.GetMesh(this.points, this.meridian, this.radius, this.closed);
        //Mesh meshAtPositions = MakeMesh.GetMeshAtPositions(this.points, this.radius * 2.0f);
        //Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, MakeMesh.CurveMaterial, 0);
        //Graphics.DrawMesh(meshAtPositions, Vector3.zero, Quaternion.identity, MakeMesh.PositionMaterial, 0);

        for (int i = 0; i < this.points.Count; i++)
        {
            Debug.Log(this.weights[i]);
        }
    }

    List<Vector3> UpdatePoints(Vector3 vrControllerMove)
    {
        List<Vector3> newPoints = new List<Vector3>();

        for (int i = 0; i < this.points.Count; i++)
        {
            newPoints.Add(this.points[i] + vrControllerMove * this.weights[i]);
        }

        return newPoints;
    }

    float MinSegmentDist(List<Vector3> seq, bool closed)
    {
        int n = seq.Count;
        float min = SegmentDist.SSDist(seq[0], seq[1], seq[2], seq[3]);
        int endi = closed ? n - 3 : n - 4;

        for (int i = 0; i <= endi; i++)
        {
            int endj = (i == 0 || !closed) ? n - 2 : n - 1;
            for (int j = i + 2; j <= endj; j++)
            {
                float dist = SegmentDist.SSDist(seq[i], seq[i + 1], seq[j], seq[(j + 1) % n]);
                if (dist < min) min = dist;
            }
        }

        return min;
    }

    float CurveDistance(List<Vector3> seq1, bool closed, Curve curve)
    {
        List<Vector3> seq2 = curve.positions;
        float min = SegmentDist.SSDist(seq1[0], seq1[1], seq2[0], seq2[1]);
        int end1 = closed ? seq1.Count - 1 : seq1.Count - 2;
        int end2 = curve.close ? seq2.Count - 1 : seq2.Count - 2;

        for (int i = 0; i <= end1; i++)
        {
            for (int j = 0; j <= end2; j++)
            {
                float dist = SegmentDist.SSDist(seq1[i], seq1[(i + 1) % seq1.Count], seq2[j], seq2[(j + 1) % seq2.Count]);
                if (dist < min) min = dist;
            }
        }

        return min;
    }

    void NormalizePoints()
    {
        // AdjustParameter.EqualizeWithWeight(ref this.points, this.distanceThreshold, this.closed, ref this.weights);
        this.NormalizePoints_Remove();
        this.NormalizePoints_Add();
    }

    void NormalizePoints_Remove()
    {
        // float ave = this.DistanceAverage();
        var newPoints = new List<Vector3>();
        var newWeights = new List<float>();
        int n = this.points.Count;
        float accumDist = 0;
        // i = 0
        newPoints.Add(this.points[0]);
        newWeights.Add(this.weights[0]);
        // 1 <= i < n
        for (int i = 1; i < n; i++)
        {
            accumDist += Vector3.Distance(this.points[i - 1], this.points[i]);
            if (this.weights[i] == 0 || accumDist > this.distanceThreshold / 2)
            {
                newPoints.Add(this.points[i]);
                newWeights.Add(this.weights[i]);
                accumDist = 0;
            }
        }
        this.points = newPoints;
        this.weights = newWeights;
    }

    void NormalizePoints_Add()
    {
        // float ave = this.DistanceAverage();
        var newPoints = new List<Vector3>();
        var newWeights = new List<float>();
        int n = this.points.Count;
        // i = 0
        newPoints.Add(this.points[0]);
        newWeights.Add(this.weights[0]);
        // i <= i < n
        for (int i = 1; i < n; i++)
        {
            if (Vector3.Distance(this.points[i - 1], this.points[i]) > 2 * this.distanceThreshold)
            {
                Vector3 addedPoint = (this.points[i - 1] + this.points[i]) / 2;
                float addedWeight = (this.weights[i - 1] + this.weights[i]) / 2;
                newPoints.Add(addedPoint);
                newWeights.Add(addedWeight);
                newPoints.Add(this.points[i]);
                newWeights.Add(this.weights[i]);
            }
            else
            {
                newPoints.Add(this.points[i]);
                newWeights.Add(this.weights[i]);
            }
        }

        if (Vector3.Distance(this.points[n - 1], this.points[0]) > 2 * this.distanceThreshold)
        {
            Vector3 addedPoint = (this.points[n - 1] + this.points[0]) / 2;
            float addedWeight = (this.weights[n - 1] + this.weights[0]) / 2;
            newPoints.Add(addedPoint);
            newWeights.Add(addedWeight);
        }

        this.points = newPoints;
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

    public (int first, int second) ChosenPoints()
    {
        int first = -1;
        int second = -1;

        if (this.weights[0] == 0)
        {
            for (int i = 0; i < this.weights.Count; i++)
            {
                if (first < 0 && this.weights[i] > 0) first = i - 1;
                if (first >= 0 && second < 0 && this.weights[i] == 0) second = i;
            }    
        }
        else
        {
            for (int i = 0; i < this.weights.Count; i++)
            {
                if (second < 0 && this.weights[i] == 0) second = i;
                if (second >= 0 && first < 0 && this.weights[i] > 0) first = i - 1;
            }
        }

        return (first, second);
    }
}