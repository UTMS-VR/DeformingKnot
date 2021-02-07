using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using InputManager;
using DrawCurve;

namespace PullCurve
{
    public interface IKnotState
    {
        IKnotState Update();
        List<Vector3> GetPoints();
    }

    public class KnotData
    {
        public List<Vector3> points;
        public (int first, int second) chosenPoints;
        public readonly OculusTouch oculusTouch;
        public readonly int meridian;
        public readonly float radius;
        public readonly float distanceThreshold;
        public readonly List<Curve> collisionCurves;
        public readonly LogicalButton buttonA;
        public readonly LogicalButton buttonB;
        public readonly LogicalButton buttonC;
        public readonly LogicalButton buttonD;
        public readonly Material curveMaterial;
        public readonly Material pullableCurveMaterial;
        public readonly Material pointMaterial;

        public KnotData(
            List<Vector3> points,
            (int first, int second) chosenPoints,
            OculusTouch oculusTouch,
            float radius,
            int meridian,
            float distanceThreshold,
            List<Curve> collisionCurves,
            LogicalButton buttonA = null,
            LogicalButton buttonB = null,
            LogicalButton buttonC = null,
            LogicalButton buttonD = null,
            Material curveMaterial = null,
            Material pullableCurveMaterial = null,
            Material pointMaterial = null
        )
        {
            this.points = points;
            this.chosenPoints = chosenPoints;
            this.oculusTouch = oculusTouch;
            this.meridian = meridian;
            this.radius = radius;
            this.distanceThreshold = distanceThreshold;
            this.collisionCurves = collisionCurves;
            this.buttonA = buttonA ?? LogicalOVRInput.RawButton.A;
            this.buttonB = buttonB ?? LogicalOVRInput.RawButton.B;
            this.buttonC = buttonC ?? LogicalOVRInput.RawButton.RIndexTrigger;
            this.buttonD = buttonD ?? LogicalOVRInput.RawButton.RHandTrigger;
            this.curveMaterial = curveMaterial ?? MakeMesh.CurveMaterial;
            this.pullableCurveMaterial = pointMaterial ?? MakeMesh.PointMaterial;
            this.pointMaterial = pointMaterial ?? MakeMesh.PointMaterial;
        }

        public (List<Vector3> pullablePoints, List<Vector3> fixedPoints) GetPullableAndFixedPoints(int first, int second)
        {
            int count = this.points.Count;
            List<Vector3> shiftedPoints = AdjustParameter.Shift(this.points, first);
            int pullableRange = (second - first + 1 + count) % count;
            List<Vector3> pullablePoints = shiftedPoints.Take(pullableRange).ToList();
            List<Vector3> fixedPoints = shiftedPoints.Skip(pullableRange).ToList();
            return (pullablePoints, fixedPoints);
        }

        public Mesh GetPullableMesh(int first, int second)
         {
             var (pullablePoints, _) = this.GetPullableAndFixedPoints(first, second);
             return MakeMesh.GetMesh(pullablePoints, this.meridian, this.radius, false);
         }

         public Mesh GetFixedMesh(int first, int second)
         {
             var (pullablePoints, fixedPoints) = this.GetPullableAndFixedPoints(first, second);
             List<Vector3> fixedPointsAppended = 
                 fixedPoints.Prepend(pullablePoints.Last()).Append(pullablePoints.First()).ToList();
             return MakeMesh.GetMesh(fixedPointsAppended, this.meridian, this.radius, false);
         }

         public Mesh GetBoundaryMesh(int first, int second)
         {
             var (pullablePoints, _) = this.GetPullableAndFixedPoints(first, second);
             var boundaryPoints = new List<Vector3>() { pullablePoints.Last(), pullablePoints.First() };
             return MakeMesh.GetMeshAtPoints(boundaryPoints, this.radius * 3);
         }
    }



    public class KnotStateBase : IKnotState
    {
        private KnotData data;
        private Mesh pullableMesh;
        private Mesh fixedMesh;
        private Mesh boundaryMesh;

        public KnotStateBase(KnotData data)
        {
            this.data = data;
            this.pullableMesh = this.data.GetPullableMesh(this.data.chosenPoints.first, this.data.chosenPoints.second);
            this.fixedMesh = this.data.GetFixedMesh(this.data.chosenPoints.first, this.data.chosenPoints.second);
            this.boundaryMesh = this.data.GetBoundaryMesh(this.data.chosenPoints.first, this.data.chosenPoints.second);
        }

        public IKnotState Update()
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

        public List<Vector3> GetPoints()
        {
            return this.data.points;
        }
    }

    public class KnotStatePull : IKnotState
    {
        private KnotData data;
        private PullableCurve pullableCurve;

        public KnotStatePull(KnotData data)
        {
            this.data = data;
            var (pullablePoints, fixedPoints) = this.data.GetPullableAndFixedPoints(this.data.chosenPoints.first, this.data.chosenPoints.second);
            this.pullableCurve = new PullableCurve(pullablePoints, new List<Vector3>(), fixedPoints, this.data.oculusTouch, closed: true,
                meridian: this.data.meridian, radius: this.data.radius, distanceThreshold: this.data.distanceThreshold, collisionCurves: this.data.collisionCurves);
        }

        public IKnotState Update()
        {
            // List<Vector3> collisionPoints = this.collisionPoints;
            // List<Vector3> collisionPoints = this.GetCompliment(this.chosenPoints.first, this.chosenPoints.second);
            // collisionPoints = collisionPoints.Concat(this.collisionPoints).ToList();
            this.pullableCurve.Update();
            Mesh knotMesh = this.pullableCurve.GetMesh();
            Graphics.DrawMesh(knotMesh, Vector3.zero, Quaternion.identity, this.data.curveMaterial, 0);
            // this.pointMesh = MakeMesh.GetMeshAtPoints(collisionPoints, this.radius * 2);

            if (this.data.oculusTouch.GetButtonDown(this.data.buttonA))
            {
                var confirmedPoints = this.pullableCurve.GetConfirmedPoints();
                this.data.points = confirmedPoints.points;
                this.data.chosenPoints = (0, confirmedPoints.count - 1);
                return new KnotStateBase(this.data);
            }
            else if (this.data.oculusTouch.GetButton(this.data.buttonB))
            {
                return new KnotStateBase(this.data);
            }

            return null;
        }

        public List<Vector3> GetPoints()
        {
            return this.data.points;
        }

        private List<Vector3> GetCompliment(int start, int end)
        {
            int numPoints = this.data.points.Count;
            int margin = 2;
            if (start <= end)
            {
                List<Vector3> range1 = this.data.points.GetRange(end + margin, numPoints - end - margin);
                List<Vector3> range2 = this.data.points.GetRange(0, start - margin);
                return range1.Concat(range2).ToList();
            }
            else
            {
                return this.data.points.GetRange(end + margin, start - end - margin);
            }
        }
    }

    public class KnotStateChoose1 : IKnotState
    {
        private KnotData data;
        private Mesh knotMesh;

        public KnotStateChoose1(KnotData data)
        {
            this.data = data;
            this.knotMesh = MakeMesh.GetMesh(this.data.points, this.data.meridian, this.data.radius, true);
        }

        public IKnotState Update()
        {
            int ind1 = KnotStateChoose1.FindClosestPoint(this.data.oculusTouch, this.data.points);
            var chosenPoints = new List<Vector3>() { this.data.points[ind1] };
            Mesh pointMesh = MakeMesh.GetMeshAtPoints(chosenPoints, this.data.radius * 3);

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

        public List<Vector3> GetPoints()
        {
            return this.data.points;
        }

        public static int FindClosestPoint(OculusTouch oculusTouch, List<Vector3> points)
        {
            // KnotStateChoose2 からも呼び出せるように static メソッドにした
            Vector3 controllerPosition = oculusTouch.GetPositionR();
            int closestIndex = 0;
            float closestDistance = Vector3.Distance(points[closestIndex], controllerPosition);
            for (int i = 1; i < points.Count; i++)
            {
                float distance = Vector3.Distance(points[i], controllerPosition);
                if (distance < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance;
                }
            }
            return closestIndex;
        }
    }

    public class KnotStateChoose2 : IKnotState
    {
        private KnotData data;
        private Mesh knotMesh;
        private int ind1;

        public KnotStateChoose2(KnotData data, int ind1)
        {
            this.data = data;
            this.knotMesh = MakeMesh.GetMesh(this.data.points, this.data.meridian, this.data.radius, true);
            this.ind1 = ind1;
        }

        public IKnotState Update()
        {
            int ind2 = KnotStateChoose1.FindClosestPoint(this.data.oculusTouch, this.data.points);
            var (first, second) = KnotStateChoose2.ChooseShorterPath((this.ind1, ind2), this.data.points.Count);

            Mesh pullableMesh = this.data.GetPullableMesh(first, second);
            Mesh fixedMesh = this.data.GetFixedMesh(first, second);
            Mesh boundaryMesh = this.data.GetBoundaryMesh(first, second);

            Graphics.DrawMesh(pullableMesh, Vector3.zero, Quaternion.identity, this.data.pullableCurveMaterial, 0);
            Graphics.DrawMesh(fixedMesh, Vector3.zero, Quaternion.identity, this.data.curveMaterial, 0);
            Graphics.DrawMesh(boundaryMesh, Vector3.zero, Quaternion.identity, this.data.pointMaterial, 0);

            if (this.data.oculusTouch.GetButtonDown(this.data.buttonA) && this.ind1 != ind2)
            {
                this.data.chosenPoints = (first, second);
                return new KnotStateBase(this.data);
            }
            else if (this.data.oculusTouch.GetButtonDown(this.data.buttonB))
            {
                return new KnotStateBase(this.data);
            }

            return null;
        }

        public List<Vector3> GetPoints()
        {
            return this.data.points;
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

    public class KnotStateDraw : IKnotState
    {
        private KnotData data;
        private Curve drawnCurve;

        public KnotStateDraw(KnotData data)
        {
            this.data = data;
            this.drawnCurve = new Curve(
                points: this.data.points,
                closed: true,
                segment: this.data.distanceThreshold / 2,
                radius: this.data.radius
                );
        }

        public IKnotState Update()
        {
            if (this.data.oculusTouch.GetButtonDown(this.data.buttonC))
            {
                // start drawing new curve
                this.drawnCurve.points = new List<Vector3>();
                this.drawnCurve.closed = false;
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
                this.data.points = this.drawnCurve.points;
                return new KnotStateBase(this.data);
            }
            else if (this.data.oculusTouch.GetButtonDown(this.data.buttonB))
            {
                return new KnotStateBase(this.data);
            }

            return null;
        }

        public List<Vector3> GetPoints()
        {
            return this.data.points;
        }
    }
}