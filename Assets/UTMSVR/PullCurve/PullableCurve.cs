using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InputManager;
using DrawCurve;
using System;

namespace PullCurve
{

    public class PullableCurve
    {
        // private List<Vector3> initialPoints;
        private OpenCurve pullableRange;
        private OpenCurve preFixedRange;
        private OpenCurve postFixedRange;
        private IEnumerable<(HandCurve handCurve, CurveDistanceHandler handler)> collisionCurves;
        private SelfDistanceHandler selfDistanceHandler;
        private float distanceThreshold;
        private float epsilon;
        private List<float> weights;
        private OculusTouch oculusTouch;
        // private Vector3 initialVRCpoint;
        private Vector3 controllerPosition;
        // private Material material;
        bool closed;
        int shift;
        private readonly int meridianCount;
        private readonly float radius;

        public PullableCurve(
            OpenCurve pullableRange,
            OpenCurve preFixedRange,
            OpenCurve postFixedRange,
            OculusTouch oculusTouch,
            // Material material,
            List<float> weights = null,
            bool closed = false,
            float distanceThreshold = -1,
            List<HandCurve> collisionCurves = null,
            int shift = 0)
        {
            this.oculusTouch = oculusTouch;
            this.controllerPosition = oculusTouch.GetPositionR();
            // pullableRange
            this.pullableRange = pullableRange;
            this.meridianCount = pullableRange.meridianCount;
            this.radius = pullableRange.radius;
            // preFixedRange
            this.preFixedRange = preFixedRange;
            if (preFixedRange.meridianCount != this.meridianCount) {
                throw new Exception("preFixedRange.meridianCount != pullableRange.meridianCount");
            }
            if (preFixedRange.radius != this.radius) {
                throw new Exception("preFixedRange.radius != pullableRange.radius");
            }
            // postFixedRange
            this.postFixedRange = postFixedRange;
            if (preFixedRange.meridianCount != this.meridianCount) {
                throw new Exception("preFixedRange.meridianCount != pullableRange.meridianCount");
            }
            if (postFixedRange.radius != this.radius) {
                throw new Exception("postFixedRange.radius != pullableRange.radius");
            }
            // distanceThreshold
            if (distanceThreshold <= 0)
            {
                this.distanceThreshold = PullableCurve.DistanceAverage(this.pullableRange);
            }
            else
            {
                this.distanceThreshold = distanceThreshold;
            }
            this.epsilon = this.distanceThreshold * 0.2f;
            if (weights == null)
            {
                this.weights = PullableCurve.GetWeights(this.pullableRange.GetPoints().Count);
            }
            else
            {
                if (weights.Count != this.pullableRange.GetPoints().Count)
                {
                    Debug.Log("PullableCurveのコンストラクタの引数において、weightsの長さとpullablePointsの長さが一致していません");
                    throw new System.Exception("PullableCurveのコンストラクタの引数において、weightsとpullablePointsの長さが一致していません");
                }
                this.weights = weights;
            }
            // this.material = material;
            this.closed = closed;
            this.shift = shift;
            if (collisionCurves != null)
            {
                this.collisionCurves = collisionCurves.Select(
                    handCurve => (
                    handCurve,
                    //(CurveDistanceHandler) new TrivialCurveDistanceHandler(
                    //    length1: this.pullablePoints.Count, // 注意: pullablePoints に対してだけ衝突判定を考える
                    //    length2: curve.points.Count,
                    //    closed1: this.closed,
                    //    closed2: curve.closed
                    //    )
                    (CurveDistanceHandler)new SimpleCurveDistanceHandler(
                        length1: this.pullableRange.GetPoints().Count, // 注意: pullablePoints に対してだけ衝突判定を考える
                        length2: handCurve.curve.GetPoints().Count,
                        closed1: this.closed,
                        closed2: handCurve.closed,
                        epsilon: this.epsilon,
                        dist: (i, j) => PullableCurve.CurveSegmentDistance(this.pullableRange, handCurve.curve, i, j)
                        )
                    )
                    ).ToList(); // ToList しないと、毎回中身が生成される (handler の constructor が毎回呼ばれる)
            }
            Curve curve = this.GetCurve();
            //this.selfDistanceHandler = new TrivialSelfDistanceHandler(
            //    length: points.Count,
            //    closed: this.closed
            //    );
            this.selfDistanceHandler = new SimpleSelfDistanceHandler(
            length: curve.GetPoints().Count,
            closed: this.closed,
            epsilon: this.epsilon,
            dist: (i, j) => PullableCurve.CurveSegmentDistance(curve, curve, i, j));
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
            OpenCurve pullableRange = new OpenCurve(points);
            OpenCurve preFixedRange = new OpenCurve(new List<Vector3>());
            OpenCurve postFixedRange = new OpenCurve(new List<Vector3>());
            return new PullableCurve(pullableRange, preFixedRange, postFixedRange, oculusTouch);
        }

        public Curve GetCurve(Curve curve = null)
        {
            // equalize しない (ことを想定して内部で利用している)
            if (curve == null) curve = this.pullableRange;
            List<Vector3> prePoints = this.preFixedRange.GetPoints().Concat(curve.GetPoints()).ToList();
            return Curve.create(prePoints.Concat(this.postFixedRange.GetPoints()).ToList(), this.closed, curve.meridianCount, curve.radius);
        }

        public (Curve curve, int pullableCount, int shift) GetEqualizedCurve()
        {
            Curve equalizedPullableRange = this.pullableRange.Equalize(this.distanceThreshold);
            int pullableCount = equalizedPullableRange.GetPoints().Count;
            Curve equalizedCurve = GetCurve(equalizedPullableRange);
            return (equalizedCurve, pullableCount, this.shift);
        }

        public int GetCount()
        {
            return this.pullableRange.GetPoints().Count;
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
            Curve curve = this.GetCurve();
            float floatShift = ((float)this.shift) / curve.GetPoints().Count;
            Func<Vector2, Vector2> uvTransformer = (uv) => new Vector2(uv.x, uv.y + floatShift);
            return curve.GetMesh(uvTransformer);
        }

        public void Update()
        {
            this.UpdatePoints();
            if (this.collisionCurves != null)
            {
                foreach (var (handCurve, handler) in this.collisionCurves)
                {
                    handler.Update((i, j) => PullableCurve.CurveSegmentDistance(this.pullableRange, handCurve.curve, i, j));
                }
            }
            Curve curve = this.GetCurve();
            this.selfDistanceHandler.Update((i, j) => PullableCurve.CurveSegmentDistance(curve, curve, i, j));
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

            OpenCurve newPullableRange = new OpenCurve(new List<Vector3>(), this.meridianCount, this.radius);
            for (int i = 0; i < this.pullableRange.GetPoints().Count; i++)
            {
                newPullableRange.Add(this.pullableRange.GetPoints()[i] + vrControllerMove * this.weights[i]);
            }

            Curve newCurve = this.GetCurve(newPullableRange);
            if (this.selfDistanceHandler.Distance((i, j) => PullableCurve.CurveSegmentDistance(
                newCurve, newCurve, i, j)) <= this.epsilon) return;
            if (this.collisionCurves != null)
            {
                foreach (var (handCurve, handler) in this.collisionCurves)
                {
                    float dist = handler.Distance((i, j) => PullableCurve.CurveSegmentDistance(
                        newPullableRange, handCurve.curve, i, j));
                    if (dist <= this.epsilon) return;
                }
            }
            this.pullableRange = newPullableRange;
        }

        private static float CurveSegmentDistance(Curve curve1, Curve curve2, int index1, int index2)
        {
            List<Vector3> points1 = curve1.GetPoints();
            List<Vector3> points2 = curve2.GetPoints();
            int count1 = curve1.GetPoints().Count;
            int count2 = curve2.GetPoints().Count;
            return SegmentDist.SSDist(
                points1[index1], points1[(index1 + 1) % count1],
                points2[index2], points2[(index2 + 1) % count2]
                );
        }

        private static float DistanceAverage(Curve curve)
        {
            float distanceSum = 0;
            List<Vector3> points = curve.GetPoints();
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
}
