using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DebugUtil;
using DrawCurve;

public class PullableCurve
{
    private List<Vector3> pullablePoints;
    private List<Vector3> remainingPoints;
    private VRController vrController;
    private Vector3 controllerPosition;
    private List<float> weights;
    private int meridian;
    private float radius;
    private float distanceThreshold;

    public PullableCurve(
        List<Vector3> points,
        (int first, int second) chosenPoints,
        VRController vrController,
        int meridian = 10,
        float radius = 0.002f,
        float distanceThreshold = -1)
    {
        AdjustParameter.Shift(ref points, chosenPoints.first);
        int count = (chosenPoints.second - chosenPoints.first + 1 + points.Count) % points.Count;
        this.pullablePoints = points.Take(count).ToList();
        this.remainingPoints = points.Skip(count).ToList();
        this.vrController = vrController;
        this.controllerPosition = vrController.GetPosition();
        this.weights = this.GetWeights();
        this.meridian = meridian;
        this.radius = radius;

        if (distanceThreshold <= 0)
        {
            this.distanceThreshold = AdjustParameter.ArcLength(points, true) / points.Count;
        }
        else
        {
            this.distanceThreshold = distanceThreshold;
        }
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
        return new PullableCurve(points, (0, numPoints - 1), vrController);
    }

    public List<Vector3> GetPoints()
    {
        return this.pullablePoints.Concat(this.remainingPoints).ToList();
    }

    private float BumpFunction(float t)
    {
        float theta = Mathf.PI * (2 * t - 1);
        return (Mathf.Cos(theta) + 1) / 2;
    }

    private List<float> GetWeights()
    {
        List<float> weights = new List<float>();
        weights.Add(0);
        float arc = AdjustParameter.ArcLength(this.pullablePoints, false);
        float accDist = 0;

        for (int i = 1; i < this.pullablePoints.Count; i++)
        {
            accDist += Vector3.Distance(this.pullablePoints[i - 1], this.pullablePoints[i]);
            weights.Add(BumpFunction(accDist / arc));
        }

        return weights;
    }

    public Mesh GetMesh()
    {
        return MakeMesh.GetMesh(this.pullablePoints.Concat(this.remainingPoints).ToList(), this.meridian, this.radius, true);
    }

    public int GetCount()
    {
        return this.pullablePoints.Count;
    }

    public void Update(List<Curve> collisionCurves = null)
    {
        Vector3 controllerNewPosition = this.vrController.GetPosition();
        Vector3 vrControllerMove = controllerNewPosition - this.controllerPosition;

        if (this.distanceThreshold * 0.2f < vrControllerMove.magnitude)
        {
            vrControllerMove = vrControllerMove.normalized * this.distanceThreshold * 0.2f;
        }

        this.controllerPosition = controllerNewPosition;

        List<Vector3> newPullablePoints = this.UpdatePoints(vrControllerMove);
        List<Vector3> newPoints = newPullablePoints.Concat(remainingPoints).ToList();
        if (newPoints.Count >= 4 && this.MinSegmentDist(newPoints, true) <= this.distanceThreshold * 0.2f) return;

        foreach (Curve curve in collisionCurves)
        {
            if (this.CurveDistance(newPoints, true, curve) <= this.distanceThreshold * 0.2f) return;
        }
        
        this.pullablePoints = newPullablePoints;
        this.NormalizePoints();
    }

    List<Vector3> UpdatePoints(Vector3 vrControllerMove)
    {
        List<Vector3> newPoints = new List<Vector3>();

        for (int i = 0; i < this.pullablePoints.Count; i++)
        {
            newPoints.Add(this.pullablePoints[i] + vrControllerMove * this.weights[i]);
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
        NormalizePoints_Remove(ref this.pullablePoints, ref this.weights);
        NormalizePoints_Add(ref this.pullablePoints, ref this.weights);
        // AdjustParameter.Equalize(ref deformingPoints, this.distanceThreshold, false);
        // this.weights = this.GetWeights();
    }

    /*List<Vector3> NormalizePoints_Remove(List<Vector3> seq)
    {
        var newSeq = new List<Vector3>();
        int n = seq.Count;
        float accumDist = 0;
        // i = 0
        newSeq.Add(seq[0]);
        // 1 <= i < n - 1
        for (int i = 1; i < n - 1; i++)
        {
            accumDist += Vector3.Distance(seq[i - 1], seq[i]);
            if (accumDist > this.distanceThreshold / 2)
            {
                newSeq.Add(seq[i]);
                accumDist = 0;
            }
        }
        // i = n - 1
        newSeq.Add(seq[n - 1]);

        return newSeq;
    }

    List<Vector3> NormalizePoints_Add(List<Vector3> seq)
    {
        var newSeq = new List<Vector3>();
        int n = seq.Count;
        // i = 0
        newSeq.Add(seq[0]);
        // i <= i < n
        for (int i = 1; i < n; i++)
        {
            if (Vector3.Distance(seq[i - 1], seq[i]) > 2 * this.distanceThreshold)
            {
                Vector3 addedPoint = (seq[i - 1] + seq[i]) / 2;
                newSeq.Add(addedPoint);
                newSeq.Add(seq[i]);
            }
            else
            {
                newSeq.Add(seq[i]);
            }
        }

        return newSeq;
    }*/

    void NormalizePoints_Remove(ref List<Vector3> seq, ref List<float> wseq)
    {
        var newSeq = new List<Vector3>();
        var newWseq = new List<float>();
        int n = seq.Count;
        float accumDist = 0;
        // i = 0
        newSeq.Add(seq[0]);
        newWseq.Add(wseq[0]);
        // 1 <= i < n - 1
        for (int i = 1; i < n - 1; i++)
        {
            accumDist += Vector3.Distance(seq[i - 1], seq[i]);
            if (accumDist > this.distanceThreshold / 2)
            {
                newSeq.Add(seq[i]);
                newWseq.Add(wseq[i]);
                accumDist = 0;
            }
        }
        // i = n - 1
        newSeq.Add(seq[n - 1]);
        newWseq.Add(wseq[n - 1]);

        seq = newSeq;
        wseq = newWseq;
    }

    void NormalizePoints_Add(ref List<Vector3> seq, ref List<float> wseq)
    {
        var newSeq = new List<Vector3>();
        var newWseq = new List<float>();
        int n = seq.Count;
        // i = 0
        newSeq.Add(seq[0]);
        newWseq.Add(wseq[0]);
        // i <= i < n
        for (int i = 1; i < n; i++)
        {
            if (Vector3.Distance(seq[i - 1], seq[i]) > 2 * this.distanceThreshold)
            {
                Vector3 addedPoint = (seq[i - 1] + seq[i]) / 2;
                float addedWeight = (wseq[i - 1] + wseq[i]) / 2;
                newSeq.Add(addedPoint);
                newWseq.Add(addedWeight);
                newSeq.Add(seq[i]);
                newWseq.Add(wseq[i]);
            }
            else
            {
                newSeq.Add(seq[i]);
                newWseq.Add(wseq[i]);
            }
        }

        seq = newSeq;
        wseq = newWseq;
    }
}