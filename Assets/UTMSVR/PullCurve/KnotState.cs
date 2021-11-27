using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using InputManager;
using DrawCurve;
using System;

#nullable enable

namespace PullCurve
{

    public interface IKnotState
    {
        IKnotState? Update();
        Curve GetCurve();
    }

    class KnotData
    {
        public ClosedCurve curve;
        public (int first, int second) chosenPoints;
        public readonly OculusTouch oculusTouch;
        public readonly float distanceThreshold;
        public readonly List<Curve>? collisionCurves;
        public readonly LogicalButton buttonA;
        public readonly LogicalButton buttonB;
        public readonly LogicalButton buttonC;
        public readonly LogicalButton buttonD;
        public readonly Material curveMaterial;
        public readonly Material pullableCurveMaterial;
        public readonly Material pointMaterial;

        public KnotData(
            ClosedCurve curve,
            (int first, int second) chosenPoints,
            OculusTouch oculusTouch,
            float distanceThreshold,
            List<Curve>? collisionCurves,
            LogicalButton? buttonA = null,
            LogicalButton? buttonB = null,
            LogicalButton? buttonC = null,
            LogicalButton? buttonD = null,
            Material? curveMaterial = null,
            Material? pullableCurveMaterial = null,
            Material? pointMaterial = null)
        {
            this.curve = curve;
            this.chosenPoints = chosenPoints;
            this.oculusTouch = oculusTouch;
            this.distanceThreshold = distanceThreshold;
            this.collisionCurves = collisionCurves;
            this.buttonA = buttonA ?? LogicalOVRInput.RawButton.A;
            this.buttonB = buttonB ?? LogicalOVRInput.RawButton.B;
            this.buttonC = buttonC ?? LogicalOVRInput.RawButton.RIndexTrigger;
            this.buttonD = buttonD ?? LogicalOVRInput.RawButton.RHandTrigger;
            this.curveMaterial = curveMaterial ?? Curve.RainbowCurveMaterial;
            this.pullableCurveMaterial = pullableCurveMaterial ?? Curve.RainbowCurveMaterial2;
            this.pointMaterial = pointMaterial ?? Curve.PointMaterial;
        }

        public (OpenCurve pullableRange, OpenCurve fixedRange) GetPullableAndFixedRange(
            (int first, int second)? chosenPoints = null)
        {
            (int first, int second) chosenPointsNonNull = chosenPoints ?? this.chosenPoints;
            int count = this.curve.Count;
            ClosedCurve shiftedCurve = this.curve.Shift(chosenPointsNonNull.first);
            int pullableRangeCount = (chosenPointsNonNull.second - chosenPointsNonNull.first + 1 + count) % count;

            OpenCurve pullableRange = shiftedCurve.Take(pullableRangeCount);
            int virtualCount = 1;
            OpenCurve fixedRange = shiftedCurve.Skip(pullableRangeCount, virtualCount, virtualCount);
            return (pullableRange, fixedRange);
        }

        public Mesh GetWholeMesh()
        {
            return this.curve.GetMesh();
        }

        public Mesh GetPullableMesh((int first, int second)? chosenPoints = null)
        {
            int count = this.curve.GetPoints().Count;
            var (pullableRange, _) = this.GetPullableAndFixedRange(chosenPoints);
            return pullableRange.GetMesh();
        }

        public Mesh GetFixedMesh((int first, int second)? chosenPoints = null)
        {
            int count = this.curve.GetPoints().Count;
            var (pullableRange, fixedRange) = this.GetPullableAndFixedRange(chosenPoints);
            return fixedRange.GetMesh();
            // ↓前は pullableRange の端点を fixedRange に追加することで，
            // 境界部分に隙間ができないようにしていた．
            // その隙間は GetBoundaryMesh() で隠れるから問題ないような気もするけど，
            // 本当に大丈夫…？
            // float floatShift = ((float)(shift + pullableRange.GetPoints().Count)) / count;
            // float fixedRate = ((float)fixedRange.GetPoints().Count) / count;
            // Func<Vector2, Vector2> uvTransformer = (uv) => new Vector2(uv.x, uv.y * fixedRate + floatShift);
            // List<Vector3> fixedPointsAppended =
            //     fixedRange.GetPoints().Prepend(pullableRange.GetPoints().Last()).Append(pullableRange.GetPoints().First()).ToList();
            // OpenCurve newCurve = new OpenCurve(fixedPointsAppended, this.curve.meridianCount, this.curve.radius);
            // return newCurve.GetMesh(uvTransformer);
        }

        public Mesh GetBoundaryMesh((int first, int second)? chosenPoints = null)
        {
            var (pullableRange, _) = this.GetPullableAndFixedRange(chosenPoints);
            var boundaryPoints = new List<Vector3>() { pullableRange.GetPoints().Last(), pullableRange.GetPoints().First() };
            VisiblePoints boundaryVisiblePoints = new VisiblePoints(boundaryPoints, this.curve.radius * 2.0f);
            return boundaryVisiblePoints.GetMesh();
        }

        public void ChooseDefaultPoints() {
            int first = 0;
            int second = this.curve.GetPoints().Count / 3;
            this.chosenPoints = (first, second);
        }
    }



    class KnotStateBase : IKnotState
    {
        private KnotData data;
        private Mesh pullableMesh;
        private Mesh fixedMesh;
        private Mesh boundaryMesh;

        public KnotStateBase(KnotData data)
        {
            this.data = data;
            this.pullableMesh = this.data.GetPullableMesh();
            this.fixedMesh = this.data.GetFixedMesh();
            this.boundaryMesh = this.data.GetBoundaryMesh();
        }

        public IKnotState? Update()
        {
            Graphics.DrawMesh(this.pullableMesh, Vector3.zero, Quaternion.identity, this.data.pullableCurveMaterial, 0);
            Graphics.DrawMesh(this.fixedMesh, Vector3.zero, Quaternion.identity, this.data.curveMaterial, 0);
            Graphics.DrawMesh(this.boundaryMesh, Vector3.zero, Quaternion.identity, this.data.pointMaterial, 0);

            if (this.data.oculusTouch.GetButtonDown(this.data.buttonA))
            {
                return new KnotStatePull(this.data);
            }
            else if (this.data.oculusTouch.GetButtonDown(this.data.buttonB))
            {
                return new KnotStateChoose1(this.data);
            }
            else if (this.data.oculusTouch.GetButtonDown(this.data.buttonC))
            {
                return new KnotStateDraw(this.data);
            }

            return null;
        }

        public Curve GetCurve()
        {
            return this.data.curve;
        }
    }

    class KnotStatePull : IKnotState
    {
        private KnotData data;
        private PullableCurve pullableCurve;

        public KnotStatePull(KnotData data)
        {
            this.data = data;
            var (pullableRange, postFixedRange) = this.data.GetPullableAndFixedRange();
            OpenCurve preFixedRange = new OpenCurve(new List<Vector3>(), pullableRange.meridianCount, pullableRange.radius);
            this.pullableCurve = new PullableCurve(pullableRange, preFixedRange, postFixedRange, this.data.oculusTouch, closed: true,
                distanceThreshold: this.data.distanceThreshold,
                collisionCurves: this.data.collisionCurves
            );
        }

        public IKnotState? Update()
        {
            // List<Vector3> collisionPoints = this.collisionPoints;
            // List<Vector3> collisionPoints = this.GetCompliment(this.chosenPoints.first, this.chosenPoints.second);
            // collisionPoints = collisionPoints.Concat(this.collisionPoints).ToList();
            this.pullableCurve.Update();
            Mesh knotMesh = this.pullableCurve.GetMesh();
            Graphics.DrawMesh(knotMesh, Vector3.zero, Quaternion.identity, this.data.curveMaterial, 0);

            if (this.data.oculusTouch.GetButtonDown(this.data.buttonA))
            {
                var (curve, pullableCount) = this.pullableCurve.GetEqualizedCurve();
                if (curve is ClosedCurve closedCurve) {
                    this.data.curve = closedCurve;
                    this.data.chosenPoints = (0, pullableCount - 1);
                    return new KnotStateBase(this.data);
                } else {
                    throw new Exception("This can't happen!");
                }
            }
            else if (this.data.oculusTouch.GetButton(this.data.buttonB))
            {
                return new KnotStateBase(this.data);
            }

            return null;
        }

        public Curve GetCurve()
        {
            return this.data.curve;
        }

        private List<Vector3> GetCompliment(int start, int end)
        {
            int numPoints = this.data.curve.GetPoints().Count;
            int margin = 2;
            if (start <= end)
            {
                List<Vector3> range1 = this.data.curve.GetPoints().GetRange(end + margin, numPoints - end - margin);
                List<Vector3> range2 = this.data.curve.GetPoints().GetRange(0, start - margin);
                return range1.Concat(range2).ToList();
            }
            else
            {
                return this.data.curve.GetPoints().GetRange(end + margin, start - end - margin);
            }
        }
    }

    class KnotStateChoose1 : IKnotState
    {
        private KnotData data;
        private Mesh knotMesh;
        private VisiblePoints visiblePoints;

        public KnotStateChoose1(KnotData data)
        {
            this.data = data;
            //float floatShift = ((float)this.data.shift) / this.data.points.Count;
            //Func<Vector2, Vector2> uvTransformer = (uv) => new Vector2(uv.x, uv.y + floatShift);
            //this.knotMesh = MakeMesh.GetMesh(this.data.points, this.data.meridian, this.data.radius, true, uvTransformer);
            this.knotMesh = this.data.GetWholeMesh();
            this.visiblePoints = new VisiblePoints(new List<Vector3>(), this.data.curve.radius * 2.0f);
        }

        public IKnotState? Update()
        {
            int ind1 = KnotStateChoose1.FindClosestPoint(this.data.oculusTouch, this.data.curve);
            var chosenPoints = new List<Vector3>() { this.data.curve.GetPoints()[ind1] };
            this.visiblePoints.SetPoints(chosenPoints);
            this.visiblePoints.UpdateMesh();
            Mesh pointMesh = this.visiblePoints.GetMesh();

            Graphics.DrawMesh(this.knotMesh, Vector3.zero, Quaternion.identity, this.data.curveMaterial, 0);
            Graphics.DrawMesh(pointMesh, Vector3.zero, Quaternion.identity, this.data.pointMaterial, 0);

            if (this.data.oculusTouch.GetButtonDown(this.data.buttonA))
            {
                return new KnotStateChoose2(this.data, ind1);
            }
            else if (this.data.oculusTouch.GetButtonDown(this.data.buttonB))
            {
                return new KnotStateBase(this.data);
            }

            return null;
        }

        public Curve GetCurve()
        {
            return this.data.curve;
        }

        public static int FindClosestPoint(OculusTouch oculusTouch, Curve curve)
        {
            // KnotStateChoose2 からも呼び出せるように static メソッドにした
            Vector3 controllerPosition = oculusTouch.GetPositionR();
            int closestIndex = 0;
            float closestDistance = Vector3.Distance(curve.GetPoints()[closestIndex], controllerPosition);
            for (int i = 1; i < curve.GetPoints().Count; i++)
            {
                float distance = Vector3.Distance(curve.GetPoints()[i], controllerPosition);
                if (distance < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance;
                }
            }
            return closestIndex;
        }
    }

    class KnotStateChoose2 : IKnotState
    {
        private KnotData data;
        private Mesh knotMesh;
        private int ind1;

        public KnotStateChoose2(KnotData data, int ind1)
        {
            this.data = data;
            this.knotMesh = this.data.curve.GetMesh();
            this.ind1 = ind1;
        }

        public IKnotState? Update()
        {
            int ind2 = KnotStateChoose1.FindClosestPoint(this.data.oculusTouch, this.data.curve);
            var chosenPoints = KnotStateChoose2.ChooseShorterPath((this.ind1, ind2), this.data.curve.GetPoints().Count);
            this.DrawMesh(chosenPoints);

            if (this.data.oculusTouch.GetButtonDown(this.data.buttonA))
            {
                if (this.ind1 != ind2)
                {
                    this.data.chosenPoints = chosenPoints;
                }
                return new KnotStateBase(this.data);
            }
            else if (this.data.oculusTouch.GetButtonDown(this.data.buttonB))
            {
                return new KnotStateBase(this.data);
            }

            return null;
        }

        private void DrawMesh((int first, int second) chosenPoints)
        {
            Mesh pullableMesh = this.data.GetPullableMesh(chosenPoints);
            Mesh fixedMesh = this.data.GetFixedMesh(chosenPoints);
            Mesh boundaryMesh = this.data.GetBoundaryMesh(chosenPoints);
            Graphics.DrawMesh(pullableMesh, Vector3.zero, Quaternion.identity, this.data.pullableCurveMaterial, 0);
            Graphics.DrawMesh(fixedMesh, Vector3.zero, Quaternion.identity, this.data.curveMaterial, 0);
            Graphics.DrawMesh(boundaryMesh, Vector3.zero, Quaternion.identity, this.data.pointMaterial, 0);
        }

        public Curve GetCurve()
        {
            return this.data.curve;
        }

        private static (int first, int second) ChooseShorterPath((int first, int second) points, int numPoints)
        {
            int smaller = Mathf.Min(points.first, points.second);
            int larger = Mathf.Max(points.first, points.second);
            if (2 * (larger - smaller) <= numPoints)
            {
                return (smaller, larger);
            }
            else
            {
                return (larger, smaller);
            }
        }
    }

    class KnotStateDraw : IKnotState
    {
        private KnotData data;
        private HandCurve drawnCurve;

        public KnotStateDraw(KnotData data)
        {
            this.data = data;
            this.drawnCurve = new HandCurve(
                curve: this.data.curve,
                segment: 0.03f
                );
        }

        public IKnotState? Update()
        {
            if (this.data.oculusTouch.GetButtonDown(this.data.buttonC))
            {
                int meridianCount = this.data.curve.meridianCount;
                float radius = this.data.curve.radius;
                // start drawing new curve
                this.drawnCurve.curve = new OpenCurve(new List<Vector3>(), meridianCount, radius);
            }
            if (!this.drawnCurve.closed)
            {
                this.drawnCurve.Draw();
            }
            this.drawnCurve.Move();

            Mesh knotMesh = this.drawnCurve.mesh;
            Vector3 position = this.drawnCurve.position;
            Quaternion rotation = this.drawnCurve.rotation;
            Graphics.DrawMesh(knotMesh, position, rotation, this.data.curveMaterial, 0);

            if (this.data.oculusTouch.GetButtonDown(this.data.buttonA))
            {
                if (this.drawnCurve.curve.Count <= 5) {
                    Debug.Log("Written curve is too short. Restored the previous curve.");
                    return new KnotStateBase(this.data);
                }
                this.data.curve = this.drawnCurve.curve.Close();
                this.data.ChooseDefaultPoints();
                return new KnotStateBase(this.data);
            }
            else if (this.data.oculusTouch.GetButtonDown(this.data.buttonB))
            {
                return new KnotStateBase(this.data);
            }

            return null;
        }

        public Curve GetCurve()
        {
            return this.data.curve;
        }
    }
}
