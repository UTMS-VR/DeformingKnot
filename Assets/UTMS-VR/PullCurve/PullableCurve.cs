using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InputManager;
using DrawCurve;

namespace PullCurve
{
    public class PullableCurve
    {
        // private List<Vector3> initialPoints;
        private List<Vector3> pullablePoints;
        private List<Vector3> preFixedPoints;
        private List<Vector3> postFixedPoints;
        private List<(Curve curve, CurveDistanceHandler handler)> collisionCurves;
        private SelfDistanceHandler selfDistanceHandler;
        private float distanceThreshold;
        private float epsilon;
        private List<float> weights;
        private OculusTouch oculusTouch;
        // private Vector3 initialVRCpoint;
        private Vector3 controllerPosition;
        private int meridian;
        private float radius;
        // private Material material;
        private bool closed;

        public PullableCurve(
            List<Vector3> pullablePoints,
            List<Vector3> preFixedPoints,
            List<Vector3> postFixedPoints,
            OculusTouch oculusTouch,
            // Material material,
            List<float> weights = null,
            int meridian = 20,
            float radius = 0.1f,
            bool closed = false,
            float distanceThreshold = -1,
            List<Curve> collisionCurves = null)
        {
            this.oculusTouch = oculusTouch;
            this.controllerPosition = oculusTouch.GetPositionR();
            this.pullablePoints = pullablePoints;
            this.preFixedPoints = preFixedPoints;
            this.postFixedPoints = postFixedPoints;
            if (distanceThreshold <= 0)
            {
                this.distanceThreshold = PullableCurve.DistanceAverage(this.pullablePoints);
            }
            else
            {
                this.distanceThreshold = distanceThreshold;
            }
            this.epsilon = this.distanceThreshold * 0.2f;

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

            int count = this.pullablePoints.Count;
            int preCount = this.preFixedPoints.Count;
            int postCount = this.postFixedPoints.Count;

            if (collisionCurves != null)
            {
                this.collisionCurves = collisionCurves.Select(
                    curve => (
                        curve,
                        //new TrivialCurveDistanceHandler(
                        //    length1: this.pullablePoints.Count, // 注意: pullablePoints に対してだけ衝突判定を考える
                        //    length2: curve.points.Count,
                        //    closed1: this.closed,
                        //    closed2: curve.closed
                        //    )
                        (CurveDistanceHandler)new SimpleCurveDistanceHandler(
                            length1: this.pullablePoints.Count, // 注意: pullablePoints に対してだけ衝突判定を考える
                            length2: curve.points.Count,
                            closed1: false,
                            closed2: curve.closed,
                            epsilon: this.epsilon,
                            dist: (i, j) => PullableCurve.CurveSegmentDistance(this.pullablePoints, curve.points, i, j)
                        )
                    )
                ).ToList();
            }
            List<Vector3> points = this.GetPoints();
            //this.selfDistanceHandler = new TrivialSelfDistanceHandler(
            //    length: points.Count,
            //    closed: this.closed
            //    );
            this.selfDistanceHandler = new SimpleSelfDistanceHandler(
            length: points.Count,
            closed: this.closed,
            epsilon: this.epsilon,
            dist: (i, j) => PullableCurve.CurveSegmentDistance(points, points, i, j));
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
            return new PullableCurve(points, new List<Vector3>(), new List<Vector3>(), oculusTouch);
        }

        public List<Vector3> GetPoints(List<Vector3> points = null)
        {
            // equalize しない (ことを想定して内部で利用している)
            if (points == null) points = this.pullablePoints;
            List<Vector3> prePoints = this.preFixedPoints.Concat(points).ToList();
            return prePoints.Concat(this.postFixedPoints).ToList();
        }

        public (List<Vector3> points, int pullableCount) GetEqualizedPoints()
        {
            List<Vector3> equalizedPullablePoints = AdjustParameter.Equalize(
                this.pullablePoints, this.distanceThreshold, false);
            int pullableCount = equalizedPullablePoints.Count;
            List<Vector3> equalizedPoints = GetPoints(equalizedPullablePoints);
            return (equalizedPoints, pullableCount);
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

        public void Update()
        {
            this.UpdatePoints();
            if (this.collisionCurves != null)
            {
                foreach (var (curve, handler) in this.collisionCurves)
                {
                    handler.Update((i, j) => PullableCurve.CurveSegmentDistance(this.pullablePoints, curve.points, i, j));
                }
            }
            List<Vector3> points = GetPoints();
            this.selfDistanceHandler.Update((i, j) => PullableCurve.CurveSegmentDistance(points, points, i, j));
        }

        void UpdatePoints()
        {
            Vector3 controllerNewPosition = this.oculusTouch.GetPositionR();
            Vector3 vrControllerMove = controllerNewPosition - this.controllerPosition;
            if (this.epsilon < vrControllerMove.magnitude)
            {
                vrControllerMove = vrControllerMove.normalized * this.epsilon;
            }
            this.controllerPosition = controllerNewPosition;

            List<Vector3> newPullablePoints = new List<Vector3>();
            for (int i = 0; i < this.pullablePoints.Count; i++)
            {
                newPullablePoints.Add(this.pullablePoints[i] + vrControllerMove * this.weights[i]);
            }

            List<Vector3> newPoints = this.GetPoints(newPullablePoints);
            if (this.selfDistanceHandler.Distance((i, j) => PullableCurve.CurveSegmentDistance(
                newPoints, newPoints, i, j)) <= this.epsilon) return;
             // if (newPoints.Count >= 4 && PullableCurve.MinSegmentDist(newPoints, true) <= this.epsilon) return;
            if (collisionCurves != null)
            {
                foreach (var (curve, handler) in this.collisionCurves)
                {
                    float dist = handler.Distance((i, j) => PullableCurve.CurveSegmentDistance(
                        newPullablePoints, curve.points, i, j));
                    if (dist < this.epsilon) return;
                }
            }
            this.pullablePoints = newPullablePoints;
        }

        private static float CurveSegmentDistance(List<Vector3> points1, List<Vector3> points2, int index1, int index2)
        {
            int count1 = points1.Count;
            int count2 = points2.Count;
            return SegmentDist.SSDist(
                points1[index1], points1[(index1 + 1) % count1],
                points2[index2], points2[(index2 + 1) % count2]
                );
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

        //private float BumpFunction(float t)
        //{
        //    if (t < 0.5f)
        //    {
        //        return 2 * t;
        //    }
        //    else
        //    {
        //        return 2 - 2 * t;
        //    }
        //}
    }
}