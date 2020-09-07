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
                Debug.Log("PullableCurveのコンストラクタの引数において、weightsとinitialPointsの長さが一致していません");
                throw new System.Exception("PullableCurveのコンストラクタの引数において、weightsとinitialPointsの長さが一致していません");
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

    public void Update(List<Vector3> collisionPoints = null)
    {
        this.UpdatePoints(collisionPoints);
        this.NormalizePoints();
        //Mesh mesh = MakeMesh.GetMesh(this.points, this.meridian, this.radius, this.closed);
        //Mesh meshAtPositions = MakeMesh.GetMeshAtPositions(this.points, this.radius * 2.0f);
        //Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, MakeMesh.CurveMaterial, 0);
        //Graphics.DrawMesh(meshAtPositions, Vector3.zero, Quaternion.identity, MakeMesh.PositionMaterial, 0);
    }

    void UpdatePoints(List<Vector3> collisionPoints = null)
    {
        var newPoints = new List<Vector3>();
        int n = this.points.Count;
        Vector3 controllerNewPosition = this.vrController.GetPosition();
        Vector3 vrControllerMove = controllerNewPosition - this.controllerPosition;
        this.controllerPosition = controllerNewPosition;
        for (int i = 0; i < n; i++)
        {
            // float t = (float)i / n;
            Vector3 p;
            if (this.weights[i] == 0)
            {
                // weight が 0 なら衝突判定はしない
                p = this.points[i];
            }
            else
            {
                p = this.points[i] + vrControllerMove * this.weights[i];
                if (collisionPoints != null)
                {
                    foreach (var collision in collisionPoints)
                    {
                        if (Vector3.Distance(p, collision) < this.radius)
                        {
                            return;
                        }
                    }
                }
            }
            newPoints.Add(p);
        }
        this.points = newPoints;
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
        int n = this.points.Count;
        float accumDist = 0;
        // i = 0
        newPoints.Add(this.points[0]);
        newWeights.Add(this.weights[0]);
        // 1 <= i < n
        for (int i = 1; i < n; i++)
        {
            accumDist += Vector3.Distance(this.points[i - 1], this.points[i]);
            if ( accumDist > this.distanceThreshold / 2)
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
            if ( Vector3.Distance(this.points[i-1], this.points[i]) > 2 * this.distanceThreshold)
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
