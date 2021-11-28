using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace DrawCurve {
    public abstract partial class Curve
    {
        public const int defaultMeridianCount = 10;
        public const float defaultRadius = 0.005f;

        protected List<Vector3> points;
        protected List<Vector3> preVirtualPoints = new List<Vector3>();
        protected List<Vector3> postVirtualPoints = new List<Vector3>();
        protected List<float> vCoordinates;
        protected List<float> preVirtualVCoordinates = new List<float>();
        protected List<float> postVirtualVCoordinates = new List<float>();

        private Mesh mesh = new Mesh();
        public readonly int meridianCount;
        public readonly float radius;
        public abstract bool closed { get; }
        public abstract Vector3 this[int i] { get; }
        private VisiblePoints? visiblePoints = null;
        public int Count { get { return this.points.Count; } }

        public Curve(IReadOnlyList<Vector3> points, IReadOnlyList<float> vCoordinates, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius)
        {
            this.points = points.ToList();
            this.meridianCount = meridianCount;
            this.radius = radius;
            if (vCoordinates != null) {
                if (vCoordinates.Count != this.points.Count) {
                    throw new Exception("Length must be the same: points and vCoordinates");
                }
                this.vCoordinates = vCoordinates.ToList();
            } else {
                this.vCoordinates = Curve.GenerateVCoordinates(this.points.Count, 0.0f, 1.0f);
            }
        }

        public Curve(IReadOnlyList<Vector3> points,
                     (float start, float end) vRange,
                     int meridianCount = Curve.defaultMeridianCount,
                     float radius = Curve.defaultRadius) :
        this(points,
             Curve.GenerateVCoordinates(points.Count, vRange.start, vRange.end),
             meridianCount, radius) {}

        public Curve(IReadOnlyList<Vector3> points,
                     int meridianCount = Curve.defaultMeridianCount,
                     float radius = Curve.defaultRadius) :
        this(points, (0.0f, 1.0f), meridianCount, radius) {}

        private static List<float> GenerateVCoordinates(int count, float vStart, float vEnd) {
            // 返り値のリストの最後の値は必ず vEnd よりも少し小さい値になる
            return Enumerable.Range(0, count)
                .Select(i => vStart + (vEnd - vStart) * ((float)i) / count)
                .ToList();
        }

        public static Curve Create(bool closed, IReadOnlyList<Vector3> points, List<float> vCoordinates, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius)
        {
            if (closed) {
                return new ClosedCurve(points, vCoordinates, meridianCount, radius);
            } else {
                return new OpenCurve(points, vCoordinates, meridianCount, radius);
            }
        }
        public static Curve Create(bool closed, IReadOnlyList<Vector3> points, (float start, float end) vRange, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius)
        {
            if (closed) {
                return new ClosedCurve(points, vRange, meridianCount, radius);
            } else {
                return new OpenCurve(points, vRange, meridianCount, radius);
            }
        }
        public static Curve Create(bool closed, IReadOnlyList<Vector3> points, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius)
        {
            if (closed) {
                return new ClosedCurve(points, meridianCount, radius);
            } else {
                return new OpenCurve(points, meridianCount, radius);
            }
        }

        public IReadOnlyList<Vector3> GetPoints() {
            return this.points;
        }

        public IReadOnlyList<float> GetVCoordinates() {
            return this.vCoordinates;
        }

        public IReadOnlyList<Vector3> GetPreVirtualPoints() {
            return this.preVirtualPoints;
        }

        public IReadOnlyList<float> GetPreVirtualVCoordinates() {
            return this.preVirtualVCoordinates;
        }

        public IReadOnlyList<Vector3> GetPostVirtualPoints() {
            return this.postVirtualPoints;
        }

        public IReadOnlyList<float> GetPostVirtualVCoordinates() {
            return this.postVirtualVCoordinates;
        }

        protected int GetVLength() {
            if (this.vCoordinates.Count == 0) {
                return 0;
                // throw new Exception("Cannot compute vLength because the list vCoordinates is empty");
            }
            return (int)Math.Round(this.vCoordinates.Last() - this.vCoordinates.First());
        }

        public void Add(Vector3 vector)
        {
            this.points.Add(vector);
            float vSegment = 0.05f;
            if (this.vCoordinates.Count == 0) {
                this.vCoordinates.Add(0f);
            } else {
                float previousV = this.vCoordinates.Last();
                this.vCoordinates.Add(previousV + vSegment);
            }
        }

        public void UpdateVCoordinates((float start, float end)? vRange = null) {
            (float start, float end) vRangeNotNull = vRange ?? (0.0f, 1.0f);
            this.vCoordinates = Curve.GenerateVCoordinates(this.points.Count, vRangeNotNull.start, vRangeNotNull.end);
        }

        public virtual void SetPoints(IReadOnlyList<Vector3> points)
        {
            bool pointsCountChanged = (this.points.Count != points.Count);
            this.points = points.ToList();
            if (pointsCountChanged) {
                this.UpdateVCoordinates(); // 内部で this.points にアクセスするので，先に this.points を更新しておく必要がある．
            }
        }

        public Mesh GetMesh()
        {
            if (this.mesh.vertices.Length == 0) {
                this.UpdateMesh();
            }
            return this.mesh;
        }

        public void UpdateMesh()
        {
            var (vertices, normals, uv, triangles) = MakeMesh.GetMeshInfo(this, this.meridianCount, this.radius, this.closed);
            this.mesh.vertices = vertices.ToArray();
            this.mesh.normals = normals.ToArray();
            this.mesh.uv = uv.ToArray();
            this.mesh.triangles = triangles.ToArray();
        }

        public Mesh GetMeshAtPoints()
        {
            if (this.visiblePoints == null) {
                this.visiblePoints = new VisiblePoints(this.points, this.radius * 2.0f);
            }
            this.visiblePoints.UpdateMesh();
            return this.visiblePoints.GetMesh();
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

        public OpenCurve Take(int count) {
            // virtual points を取りたい場合は OpenCurve, ClosedCurve のメソッドを用いる
            List<Vector3> points = this.points.Take(count).ToList();
            List<float> vCoordinates = this.vCoordinates.Take(count).ToList();
            return new OpenCurve(points, vCoordinates, this.meridianCount, this.radius);
        }

        public OpenCurve Skip(int count) {
            // virtual points を取りたい場合は OpenCurve, ClosedCurve のメソッドを用いる
            List<Vector3> points = this.points.Skip(count).ToList();
            List<float> vCoordinates = this.vCoordinates.Skip(count).ToList();
            return new OpenCurve(points, vCoordinates, this.meridianCount, this.radius);
        }

        public Curve Equalize(float segment) {
            return this.EqualizeInternal(segment);
        }
        protected abstract Curve EqualizeInternal(float segment);
        protected (List<Vector3>, List<float>) GetEqualizedPoints(float segment)
        {
            if (segment <= 0) {
                throw new System.Exception("segment must be positive");
            }

            int length = this.points.Count;
            List<Vector3> newPoints = new List<Vector3>();
            newPoints.Add(this.points[0]);
            List<float> newVCoordinates = new List<float>();
            newVCoordinates.Add(this.vCoordinates[0]);
            float remainder = 0.0f;
            float temporarySegment = this.TemporarySegment(segment);

            for (int i = 1; i < length; i++)
            {
                Completion(ref newPoints, this.points[i - 1], this.points[i],
                           ref newVCoordinates, this.vCoordinates[i - 1], this.vCoordinates[i],
                           ref remainder, temporarySegment);
            }

            if (this.closed)
            {
                int vLength = this.GetVLength();
                Completion(ref newPoints, this.points[length - 1], this.points[0],
                           ref newVCoordinates, this.vCoordinates[length - 1], this.vCoordinates[0] + vLength,
                           ref remainder, temporarySegment);

                // 始点と終点が重複して追加されるのを回避する
                if (newPoints.Count > this.DivisionNumber(segment))
                {
                    newPoints.RemoveAt(newPoints.Count - 1);
                    newVCoordinates.RemoveAt(newVCoordinates.Count - 1);
                }
            }
            else
            {
                // 終点がちゃんと追加されることを保証する
                if (newPoints.Count < this.DivisionNumber(segment) + 1)
                {
                    newPoints.Add(this.points[length - 1]);
                    newVCoordinates.Add(this.vCoordinates[length - 1]);
                }
            }

            return (newPoints, newVCoordinates);
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

        private static void Completion(ref List<Vector3> newPoints, Vector3 start, Vector3 end,
                                       ref List<float> newVCoordinates, float vStart, float vEnd,
                                       ref float remainder, float segment)
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
                    float rate = (distance - remainder) / distance;
                    newPoints.Add(start + (end - start) * rate);
                    newVCoordinates.Add(vStart + (vEnd - vStart) * rate);
                }
        }
    }
}
