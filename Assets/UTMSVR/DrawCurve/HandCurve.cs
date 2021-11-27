using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputManager;

#nullable enable

namespace DrawCurve
{
    public class HandCurve
    {
        // public List<Vector3> points;
        public Curve curve;
        public Mesh mesh { get { return this.curve.GetMesh();} }
        public Mesh? meshAtPoints;
        public bool closed { get { return this.curve.closed; } }
        public bool selected;
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public float segment;
        public static float collision = 0.05f;
        public static OculusTouch? oculusTouch;
        public static LogicalButton? drawButton;
        public static LogicalButton? moveButton;

        public HandCurve(Curve curve, bool selected = false, float? segment = null)
        {
            this.curve = curve;
            this.selected = selected;
            this.segment = segment ?? this.MeanOfSegment();
            if (this.segment <= 0) {
                throw new System.Exception("segment must be positive");
            }
        }

        public static void SetUp(OculusTouch oculusTouch, LogicalButton drawButton, LogicalButton moveButton)
        {
            HandCurve.oculusTouch = oculusTouch;
            HandCurve.drawButton = drawButton;
            HandCurve.moveButton = moveButton;
        }

        public void MeshUpdate()
        {
            this.curve.UpdateMesh();
        }

        public void MeshUpdatePos()
        {
            this.curve.UpdateMesh();
        }

        public void MeshAtPointsUpdate()
        {
            this.meshAtPoints = this.curve.GetMeshAtPoints();
        }

        private int Length()
        {
            return this.curve.GetPoints().Count;
        }

        private float MeanOfSegment()
        {
            IReadOnlyList<Vector3> points = this.curve.GetPoints();
            int n = this.closed ? points.Count : points.Count - 1;
            return this.curve.ArcLength() / n;
        }

        public void Draw()
        {
            if (oculusTouch == null) {
                throw new System.Exception("Call HandCurve.SetUp()");
            }
            if (oculusTouch.GetButton(drawButton))
            {
                Vector3 nowPosition = oculusTouch.GetPositionR();

                if (Length() == 0)
                {
                    this.curve.Add(nowPosition);
                }
                else if (Vector3.Distance(this.curve.GetPoints().Last(), nowPosition) >= this.segment)
                {
                    this.curve.Add(nowPosition);
                    this.MeshUpdate();
                }
            }
        }

        public void Move()
        {
            if (oculusTouch == null) {
                throw new System.Exception("Call HandCurve.SetUp()");
            }
            Vector3 nowPosition = oculusTouch.GetPositionR();
            Quaternion nowRotation = oculusTouch.GetRotationR();

            if (oculusTouch.GetButtonDown(moveButton))
            {
                MoveSetUp(nowPosition, nowRotation);
            }

            if (oculusTouch.GetButton(moveButton))
            {
                MoveUpdate(nowPosition, nowRotation);
            }

            if (oculusTouch.GetButtonUp(moveButton))
            {
                MoveCleanUp();
            }
        }

        public void MoveSetUp(Vector3 position, Quaternion rotation)
        {
            this.curve.SetPoints(this.curve.GetPoints()
                .Select(v => v - position)
                .Select(v => Quaternion.Inverse(rotation) * v)
                .ToList());
            MeshUpdate();
        }

        public void MoveUpdate(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public void MoveCleanUp()
        {
            this.curve.SetPoints(this.curve.GetPoints()
                .Select(v => this.rotation * v)
                .Select(v => v + this.position)
                .ToList());
            MeshUpdate();
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
        }

        public void Select()
        {
            if (oculusTouch == null) {
                throw new System.Exception("Call HandCurve.SetUp()");
            }

            Vector3 nowPosition = oculusTouch.GetPositionR();

            if (Distance(this.curve.GetPoints(), nowPosition).Item2 < collision)
            {
                this.selected = !this.selected;
            }
        }

        public HandCurve? ToggleClosed()
        {
            if (Vector3.Distance(this.curve.GetPoints().First(), this.curve.GetPoints().Last()) >= collision)
            {
                return null;
            }
            Curve newCurve = this.curve.ToggleClosed();
            return new HandCurve(newCurve, this.selected, this.segment);
        }

        public List<HandCurve> Cut()
        {
            if (oculusTouch == null) {
                throw new System.Exception("Call HandCurve.SetUp()");
            }

            List<HandCurve> newCurves = new List<HandCurve>();
            Vector3 nowPosition = oculusTouch.GetPositionR();
            (int, float) distance = Distance(this.curve.GetPoints(), nowPosition);
            int num = distance.Item1;

            if (distance.Item2 < collision)
            {
                if (this.closed)
                {
                    newCurves.Add(CutKnot(num));
                }
                else if (2 <= num && num <= Length() - 3)
                {
                    (HandCurve, HandCurve) curves = CutCurve(num);
                    newCurves.Add(curves.Item1);
                    newCurves.Add(curves.Item2);
                }
            }

            return newCurves;
        }

        private HandCurve CutKnot(int num)
        {
            IReadOnlyList<Vector3> points = this.curve.GetPoints();
            List<Vector3> newPoints = new List<Vector3>();

            for (int i = num + 1; i < Length(); i++)
            {
                newPoints.Add(points[i]);
            }

            for (int i = 0; i < num; i++)
            {
                newPoints.Add(points[i]);
            }

            HandCurve cutKnot = new HandCurve(new OpenCurve(newPoints), selected: true);

            return cutKnot;
        }

        private (HandCurve, HandCurve) CutCurve(int num)
        {
            IReadOnlyList<Vector3> points = this.curve.GetPoints();
            List<Vector3> newPoints1 = new List<Vector3>();
            List<Vector3> newPoints2 = new List<Vector3>();

            for (int i = 0; i < num; i++)
            {
                newPoints1.Add(points[i]);
            }

            for (int i = num + 1; i < Length(); i++)
            {
                newPoints2.Add(points[i]);
            }

            HandCurve newCurve1 = new HandCurve(new OpenCurve(newPoints1), selected: true);
            HandCurve newCurve2 = new HandCurve(new OpenCurve(newPoints2), selected: true);

            return (newCurve1, newCurve2);
        }

        public static List<HandCurve> Combine(HandCurve handCurve1, HandCurve handCurve2)
        {
            if (!(handCurve1.curve is OpenCurve)) {
                throw new System.Exception("handCurve1 must be open");
            }
            if (!(handCurve2.curve is OpenCurve)) {
                throw new System.Exception("handCurve2 must be open");
            }

            (OpenCurve curve1, OpenCurve curve2) = AdjustOrientation((OpenCurve)handCurve1.curve, (OpenCurve)handCurve2.curve);

            List<HandCurve> newHandCurves = new List<HandCurve>();
            IReadOnlyList<Vector3> points1 = curve1.GetPoints();
            IReadOnlyList<Vector3> points2 = curve2.GetPoints();

            if (Vector3.Distance(points1.Last(), points2.First()) < collision)
            {
                OpenCurve newCurve = curve1.Concat(curve2);
                bool closed = newCurve.DistanceOfFirstAndLast() < collision;
                newHandCurves.Add(new HandCurve(newCurve.ChangeClosed(closed), selected: true));
            }

            return newHandCurves;
        }

        private static (OpenCurve, OpenCurve) AdjustOrientation(OpenCurve curve1, OpenCurve curve2) {
            var points1 = curve1.GetPoints();
            var points2 = curve2.GetPoints();
            if (Vector3.Distance(points1.Last(), points2.Last()) < collision) {
                    return (curve1, curve2.Reversed());
            } else if (Vector3.Distance(points1.First(), points2.First()) < collision) {
                return (curve1.Reversed(), curve2);
            } else if (Vector3.Distance(points1.First(), points2.Last()) < collision) {
                return (curve1.Reversed(), curve2.Reversed());
            } else {
                return (curve1, curve2);
            }
        }

        public static (int, float) Distance(IReadOnlyList<Vector3> points, Vector3 position)
        {
            List<Vector3> relPoints = points.Select(v => v - position).ToList();

            int num = 0;
            float min = relPoints[0].magnitude;

            for (int i = 0; i < points.Count - 1; i++)
            {
                if (relPoints[i + 1].magnitude < min)
                {
                    num = i + 1;
                    min = relPoints[i + 1].magnitude;
                }
            }

            return (num, min);
        }

        public float MinSegmentDist()
        {
            IReadOnlyList<Vector3> seq = this.curve.GetPoints();
            int n = this.Length();
            float min = float.PositiveInfinity;
            int endi = this.closed ? n - 3 : n - 4;

            for (int i = 0; i <= endi; i++)
            {
                int endj = (i == 0 || !this.closed) ? n - 2 : n - 1;
                for (int j = i + 2; j <= endj; j++)
                {
                    float dist = SegmentDist.SSDist(seq[i], seq[i + 1], seq[j], seq[(j + 1) % n]);
                    if (dist < min) min = dist;
                }
            }

            return min;
        }

        public float CurveDistance(HandCurve other)
        {
            IReadOnlyList<Vector3> thisPoints = this.curve.GetPoints();
            IReadOnlyList<Vector3> otherPoints = other.curve.GetPoints();
            float min = float.PositiveInfinity;
            int n1 = this.Length();
            int n2 = other.Length();
            int end1 = this.closed ? n1 - 1 : n1 - 2;
            int end2 = other.closed ? n2 - 1 : n2 - 2;

            for (int i = 0; i <= end1; i++)
            {
                for (int j = 0; j <= end2; j++)
                {
                    float dist = SegmentDist.SSDist(thisPoints[i], thisPoints[(i + 1) % n1], otherPoints[j], otherPoints[(j + 1) % n2]);
                    if (dist < min) min = dist;
                }
            }

            return min;
        }

        public HandCurve DeepCopy()
        {
            IReadOnlyList<Vector3> points = ListVector3Copy(this.curve.GetPoints());
            HandCurve curve = new HandCurve(Curve.Create(this.closed, points), segment: this.segment);
            curve.selected = this.selected;
            curve.position = Vector3Copy(this.position);
            curve.rotation = QuaternionCopy(this.rotation);
            return curve;
        }

        private Vector3 Vector3Copy(Vector3 v)
        {
            Vector3 w = new Vector3(v.x, v.y, v.z);
            return w;
        }

        private Quaternion QuaternionCopy(Quaternion v)
        {
            Quaternion w = new Quaternion(v.x, v.y, v.z, v.w);
            return w;
        }

        private List<Vector3> ListVector3Copy(IReadOnlyList<Vector3> l)
        {
            List<Vector3> m = new List<Vector3>();

            foreach (Vector3 v in l)
            {
                m.Add(Vector3Copy(v));
            }

            return m;
        }
    }
}
