using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace DrawCurve {
    using UvTransformer = Func<Vector2, Vector2>;

    public abstract partial class Curve
    {
        public const int defaultMeridianCount = 10;
        public const float defaultRadius = 0.005f;

        protected List<Vector3> points;
        private Mesh mesh = new Mesh();
        public readonly int meridianCount;
        public readonly float radius;
        public abstract bool closed { get; }

        public Curve(List<Vector3> points, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius)
        {
            this.points = points;
            this.meridianCount = meridianCount;
            this.radius = radius;
        }

        public static Curve create(List<Vector3> points, bool closed, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius)
        {
            if (closed) {
                return new ClosedCurve(points, meridianCount, radius);
            } else {
                return new OpenCurve(points, meridianCount, radius);
            }
        }

        public List<Vector3> GetPoints()
        {
            return this.points;
        }

        public void Add(Vector3 vector)
        {
            this.points.Add(vector);
        }

        public void SetPoints(List<Vector3> points)
        {
            this.points = points;
        }

        public Mesh GetMesh(UvTransformer? uvTransformer = null)
        {
            if (this.mesh.vertices.Length == 0) {
                this.UpdateMesh(uvTransformer);
            }
            return this.mesh;
        }

        public void UpdateMesh(UvTransformer? uvTransformer = null)
        {
            var (vertices, normals, uv, triangles) = MakeMesh.GetMeshInfo(this, this.meridianCount, this.radius, this.closed, uvTransformer);
            this.mesh.vertices = vertices.ToArray();
            this.mesh.normals = normals.ToArray();
            this.mesh.uv = uv.ToArray();
            this.mesh.triangles = triangles.ToArray();
        }

        public Mesh GetMeshAtPoints()
        {
            return MakeMesh.GetMeshAtPoints(this, this.radius * 2.0f);
        }

        public Mesh GetMeshAtEndPoint()
        {
            return MakeMesh.GetMeshAtEndPoint(this, radius * 2.0f);
        }

        public Curve ToggleClosed() {
            if (this is ClosedCurve closedThis) {
                return closedThis.Open();
            } else if (this is OpenCurve openThis) {
                return openThis.Close();
            } else {
                throw new Exception("This can't happen!");
            }
        }
        public Curve ChangeClosed(bool closed) {
            if (closed) {
                return this.Close();
            } else {
                return this.Open();
            }
        }

        abstract public ClosedCurve Close();
        abstract public OpenCurve Open();

        public Curve Reversed() {
            return this.ReversedInternal();
        }
        protected abstract Curve ReversedInternal();

        public Curve Equalize(float segment)
        {
            if (segment <= 0) {
                throw new System.Exception("segment must be positive");
            }

            int length = this.points.Count;
            List<Vector3> newPoints = new List<Vector3>();
            newPoints.Add(this.points[0]);
            float remainder = 0.0f;
            float temporarySegment = this.TemporarySegment(segment);

            for (int i = 1; i < length; i++)
            {
                Completion(ref newPoints, this.points[i - 1], this.points[i], ref remainder, temporarySegment);
            }

            if (this.closed)
            {
                Completion(ref newPoints, this.points[length - 1], this.points[0], ref remainder, temporarySegment);

                // 始点と終点が重複して追加されるのを回避する
                if (newPoints.Count > this.DivisionNumber(segment))
                {
                    newPoints.Remove(newPoints[newPoints.Count - 1]);
                }
            }
            else
            {
                // 終点がちゃんと追加されることを保証する
                if (newPoints.Count < this.DivisionNumber(segment) + 1)
                {
                    newPoints.Add(this.points[length - 1]);
                }
            }

            return Curve.create(newPoints, this.closed, this.meridianCount, this.radius);
        }

        public float ArcLength()
        {
            int length = this.points.Count;
            float arclength = 0.0f;

            for (int i = 1; i < length; i++)
                {
                    arclength += Vector3.Distance(this.points[i - 1], this.points[i]);
                }

            if (this.closed)
                {
                    arclength += Vector3.Distance(this.points[length - 1], this.points[0]);
                }

            return arclength;
        }

        private float TemporarySegment(float segment)
        {
            return this.ArcLength() / this.DivisionNumber(segment);
        }

        private int DivisionNumber(float segment)
        {
            return Mathf.FloorToInt(this.ArcLength() / segment + 0.5f);
        }

        private static void Completion(ref List<Vector3> newPoints, Vector3 start, Vector3 end, ref float remainder, float segment)
        {
            // start から (弧長で測って) remainder だけ前の点を始点にして，
            // そこから segment ごとに newPoints に点を追加していく
            // . --- start ---- . ------- . ------- . --- end
            //    r         s-r     2s-r      3s-r     r'
            // (r = remainder, s = segment)
            float distance = Vector3.Distance(start, end);
            remainder += distance;

            while (segment < remainder)
                {
                    remainder -= segment;
                    newPoints.Add(start + (end - start) * (distance - remainder) / distance);
                }
        }
    }

    public class OpenCurve : Curve {
        public OpenCurve(List<Vector3> points, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius) : base(points, meridianCount, radius) {}

        public override bool closed {
            get {
                return false;
            }
        }

        override public ClosedCurve Close() {
            return new ClosedCurve(this.points, this.meridianCount, this.radius);
        }

        override public OpenCurve Open() {
            return this;
        }

        public OpenCurve Combine(OpenCurve other) {
            List<Vector3> newPoints = this.points.Concat(other.points).ToList();
            return new OpenCurve(newPoints, this.meridianCount, this.radius);
        }

        public float DistanceOfFirstAndLast() {
            return Vector3.Distance(this.points.First(), this.points.Last());
        }

        public new OpenCurve Reversed()
        {
            // this.points.Reverse() だと in-place に反転される
            List<Vector3> newPoints = this.points.AsEnumerable().Reverse().ToList();
            return new OpenCurve(newPoints, this.meridianCount, this.radius);
        }

        protected override Curve ReversedInternal() {
            return this.Reversed();
        }
    }

    public class ClosedCurve : Curve {
        public ClosedCurve(List<Vector3> points, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius) : base(points, meridianCount, radius) {}

        public override bool closed {
            get {
                return true;
            }
        }

        override public OpenCurve Open() {
            return new OpenCurve(this.points, this.meridianCount, this.radius);
        }

        override public ClosedCurve Close() {
            return this;
        }

        public Curve Shift(int n) // 0 <= n < this.points.Count
        {
            List<Vector3> newPoints = new List<Vector3>();

            for (int i = n; i < this.points.Count; i++)
                {
                    newPoints.Add(this.points[i]);
                }

            for (int i = 0; i < n; i++)
                {
                    newPoints.Add(this.points[i]);
                }

            return Curve.create(newPoints, this.closed, this.meridianCount, this.radius);
        }

        public new ClosedCurve Reversed()
        {
            // this.points.Reverse() だと in-place に反転される
            List<Vector3> newPoints = this.points.AsEnumerable().Reverse().ToList();
            return new ClosedCurve(newPoints, this.meridianCount, this.radius);
        }

        protected override Curve ReversedInternal() {
            return this.Reversed();
        }
    }
}
