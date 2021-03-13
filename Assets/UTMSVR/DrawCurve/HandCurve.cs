using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputManager;

namespace DrawCurve
{
    public class HandCurve
    {
        public List<Vector3> points;
        public Mesh mesh;
        public Mesh meshAtPoints;
        public bool closed;
        public bool selected;
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public float segment;
        public int meridian;
        public float radius;
        public static float collision = 0.05f;
        public static OculusTouch oculusTouch;
        public static LogicalButton drawButton;
        public static LogicalButton moveButton;

        public HandCurve(List<Vector3> points, bool closed, bool selected = false, float segment = 0.03f, int meridian = 10, float radius = 0.005f)
        {
            this.points = points;
            this.closed = closed;
            this.selected = selected;
            this.segment = segment;
            this.meridian = meridian;
            this.radius = radius;
            this.mesh = MakeMesh.GetMesh(this.points, this.meridian, this.radius, this.closed);
        }

        public static void SetUp(OculusTouch oculusTouch, LogicalButton drawButton, LogicalButton moveButton)
        {
            HandCurve.oculusTouch = oculusTouch;
            HandCurve.drawButton = drawButton;
            HandCurve.moveButton = moveButton;
        }

        public void MeshUpdate()
        {
            this.mesh = MakeMesh.GetMesh(this.points, this.meridian, this.radius, this.closed);
        }

        public void MeshUpdatePos()
        {
            List<Vector3> pointsCopy = new List<Vector3>(this.points);
            if (this.closed)
            {
                pointsCopy.Add(pointsCopy[0]);
                pointsCopy.Add(pointsCopy[1]);
            }
            List<Vector3> tangents = MakeMesh.Tangents(pointsCopy, this.closed);
            List<Vector3> principalNormals = MakeMesh.PrincipalNormals(tangents);
            Vector3[] vertices = this.mesh.vertices;
            Vector3[] normals = this.mesh.normals;
            int k=0;
            for (int j = 0; j < pointsCopy.Count; j++)
            {
                Vector3 binormal = Vector3.Cross(tangents[j], principalNormals[j]);
                for (int i = 0; i <= this.meridian; i++)
                {
                    float theta = i * 2 * Mathf.PI / this.meridian;
                    Vector3 direction = Mathf.Cos(theta) * principalNormals[j] + Mathf.Sin(theta) * binormal;
                    vertices[k] = pointsCopy[j] + this.radius * direction;
                    normals[k] = direction;
                    k++;
                }
            }
            this.mesh.vertices = vertices;
            this.mesh.normals = normals;
        }

        public void MeshAtPointsUpdate()
        {
            this.meshAtPoints = MakeMesh.GetMeshAtPoints(this.points, this.radius * 2.0f);
        }

        public void MeshAtEndPointUpdate()
        {
            this.meshAtPoints = MakeMesh.GetMeshAtEndPoint(this.points, this.radius * 2.0f);
        }

        private int Length()
        {
            return this.points.Count;
        }

        public void Draw()
        {
            if (oculusTouch.GetButton(drawButton))
            {
                Vector3 nowPosition = oculusTouch.GetPositionR();

                if (Length() == 0)
                {
                    this.points.Add(nowPosition);
                }
                else if (Vector3.Distance(this.points.Last(), nowPosition) >= this.segment)
                {
                    this.points.Add(nowPosition);
                    this.MeshUpdate();
                }
            }
        }

        public void Move()
        {
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
            this.points = this.points.Select(v => v - position).Select(v => Quaternion.Inverse(rotation) * v).ToList();
            MeshUpdate();
        }

        public void MoveUpdate(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public void MoveCleanUp()
        {
            this.points = this.points.Select(v => this.rotation * v).Select(v => v + this.position).ToList();
            MeshUpdate();
            this.position = Vector3.zero;
            this.rotation = Quaternion.identity;
        }

        public void Select()
        {
            Vector3 nowPosition = oculusTouch.GetPositionR();

            if (Distance(this.points, nowPosition).Item2 < collision)
            {
                this.selected = !this.selected;
            }
        }

        public void Close()
        {
            if (Vector3.Distance(this.points.First(), this.points.Last()) < collision)
            {
                this.closed = !this.closed;
                MeshUpdate();
            }
        }

        public List<HandCurve> Cut()
        {
            List<HandCurve> newCurves = new List<HandCurve>();
            Vector3 nowPosition = oculusTouch.GetPositionR();
            (int, float) distance = Distance(this.points, nowPosition);
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
            List<Vector3> newPoints = new List<Vector3>();

            for (int i = num + 1; i < Length(); i++)
            {
                newPoints.Add(this.points[i]);
            }

            for (int i = 0; i < num; i++)
            {
                newPoints.Add(this.points[i]);
            }

            HandCurve cutKnot = new HandCurve(newPoints, false, selected: true);

            return cutKnot;
        }

        private (HandCurve, HandCurve) CutCurve(int num)
        {
            List<Vector3> newPoints1 = new List<Vector3>();
            List<Vector3> newPoints2 = new List<Vector3>();

            for (int i = 0; i < num; i++)
            {
                newPoints1.Add(this.points[i]);
            }

            for (int i = num + 1; i < Length(); i++)
            {
                newPoints2.Add(this.points[i]);
            }

            HandCurve newCurve1 = new HandCurve(newPoints1, false, selected: true);
            HandCurve newCurve2 = new HandCurve(newPoints2, false, selected: true);

            return (newCurve1, newCurve2);
        }

        public static List<HandCurve> Combine(HandCurve curve1, HandCurve curve2)
        {
            List<HandCurve> newCurves = new List<HandCurve>();
            List<Vector3> points1 = curve1.points;
            List<Vector3> points2 = curve2.points;
            AdjustOrientation(ref points1, ref points2);

            if (Vector3.Distance(points1.Last(), points2.First()) < collision)
            {
                foreach (Vector3 v in points2)
                {
                    points1.Add(v);
                }

                bool closed = Vector3.Distance(points1.First(), points2.Last()) < collision ? true : false;
                newCurves.Add(new HandCurve(points1, closed, selected: true));
            }

            return newCurves;
        }

        private static void AdjustOrientation(ref List<Vector3> points1, ref List<Vector3> points2)
        {
            if (Vector3.Distance(points1.Last(), points2.Last()) < collision)
            {
                points2.Reverse();
            }
            else if (Vector3.Distance(points1.First(), points2.First()) < collision)
            {
                points1.Reverse();
            }
            else if (Vector3.Distance(points1.First(), points2.Last()) < collision)
            {
                points1.Reverse();
                points2.Reverse();
            }
        }

        public static (int, float) Distance(List<Vector3> points, Vector3 position)
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
            List<Vector3> seq = this.points;
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

        public float CurveDistance(HandCurve curve)
        {
            float min = float.PositiveInfinity;
            int n1 = this.Length();
            int n2 = curve.Length();
            int end1 = this.closed ? n1 - 1 : n1 - 2;
            int end2 = curve.closed ? n2 - 1 : n2 - 2;

            for (int i = 0; i <= end1; i++)
            {
                for (int j = 0; j <= end2; j++)
                {
                    float dist = SegmentDist.SSDist(this.points[i], this.points[(i + 1) % n1], curve.points[j], curve.points[(j + 1) % n2]);
                    if (dist < min) min = dist;
                }
            }

            return min;
        }

        public HandCurve DeepCopy()
        {
            List<Vector3> points = ListVector3Copy(this.points);
            HandCurve curve = new HandCurve(points, this.closed, segment: this.segment);
            curve.mesh = this.mesh;
            curve.meshAtPoints = this.meshAtPoints;
            curve.closed = this.closed;
            curve.selected = this.selected;
            curve.position = Vector3Copy(this.position);
            curve.rotation = QuaternionCopy(this.rotation);
            curve.meridian = this.meridian;
            curve.radius = this.radius;
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

        private List<Vector3> ListVector3Copy(List<Vector3> l)
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