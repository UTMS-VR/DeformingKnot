using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace DrawCurve {
    public class OpenCurve : Curve {
        public OpenCurve(List<Vector3> points, List<float> vCoordinates, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius, List<Vector3>? preVirtualPoints = null, List<Vector3>? postVirtualPoints = null) : base(points, vCoordinates, meridianCount, radius) {
            this.InitializeVirtual(preVirtualPoints, postVirtualPoints);
        }
        public OpenCurve(List<Vector3> points, (float start, float end) vRange, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius, List<Vector3>? preVirtualPoints = null, List<Vector3>? postVirtualPoints = null) : base(points, vRange, meridianCount, radius) {
            this.InitializeVirtual(preVirtualPoints, postVirtualPoints);
        }
        public OpenCurve(List<Vector3> points, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius, List<Vector3>? preVirtualPoints = null, List<Vector3>? postVirtualPoints = null) : base(points, meridianCount, radius) {
            this.InitializeVirtual(preVirtualPoints, postVirtualPoints);
        }

        private void InitializeVirtual(List<Vector3>? preVirtualPoints, List<Vector3>? postVirtualPoints) {
            // v座標をちゃんと設定するのはめんどいから，とりあえず端点の値を繰り返すことにした
            if (preVirtualPoints != null) {
                this.preVirtualPoints = preVirtualPoints;
                int count = preVirtualPoints.Count;
                float v = this.vCoordinates.First();
                this.preVirtualVCoordinates = new List<float>(Enumerable.Repeat(v, count));
            }
            if (postVirtualPoints != null) {
                this.postVirtualPoints = postVirtualPoints;
                int count = postVirtualPoints.Count;
                float v = this.vCoordinates.Last();
                this.postVirtualVCoordinates = new List<float>(Enumerable.Repeat(v, count));
            }
        }

        public override bool closed {
            get {
                return false;
            }
        }

        public override Vector3 this[int i] {
            get {
                return this.points[i];
            }
        }

        override public ClosedCurve Close() {
            return new ClosedCurve(this.points, this.vCoordinates, this.meridianCount, this.radius);
        }

        override public OpenCurve Open() {
            return this;
        }

        public OpenCurve Concat(OpenCurve other) {
            List<Vector3> newPoints = this.points.Concat(other.points).ToList();
            List<float> newVCoordinates = this.vCoordinates.Concat(other.vCoordinates).ToList();
            return new OpenCurve(newPoints, newVCoordinates, this.meridianCount, this.radius, preVirtualPoints: this.preVirtualPoints, postVirtualPoints: other.postVirtualPoints);
        }

        public float DistanceOfFirstAndLast() {
            return Vector3.Distance(this.points.First(), this.points.Last());
        }

        public OpenCurve Take(int count, int postVirtualCount) {
            // virtual points が不要な場合は Curve のメソッドを用いる
            List<Vector3> points = this.points.Take(count).ToList();
            List<Vector3> postVirtualPoints = this.points.Take(count + postVirtualCount).Skip(count).ToList();
            List<float> vCoordinates = this.vCoordinates.Take(count).ToList();
            return new OpenCurve(points, vCoordinates, this.meridianCount, this.radius, preVirtualPoints: this.preVirtualPoints, postVirtualPoints: postVirtualPoints);
        }

        public OpenCurve Skip(int count, int preVirtualCount) {
            // virtual points が不要な場合は Curve のメソッドを用いる
            List<Vector3> points = this.points.Skip(count).ToList();
            List<Vector3> preVirtualPoints = this.points.Skip(count - preVirtualCount).Take(preVirtualCount).ToList();
            List<float> vCoordinates = this.vCoordinates.Skip(count).ToList();
            return new OpenCurve(points, vCoordinates, this.meridianCount, this.radius, preVirtualPoints: preVirtualPoints, postVirtualPoints: this.postVirtualPoints);
        }


        public new OpenCurve Reversed()
        {
            // this.points.Reverse() だと in-place に反転される
            List<Vector3> newPoints = this.points.AsEnumerable().Reverse().ToList();
            List<float> newVCoordinates = this.vCoordinates.AsEnumerable().Reverse().ToList();
            return new OpenCurve(newPoints, newVCoordinates, this.meridianCount, this.radius, preVirtualPoints: this.postVirtualPoints, postVirtualPoints: this.preVirtualPoints);
        }

        protected override Curve ReversedInternal() {
            return this.Reversed();
        }

        public new OpenCurve Equalize(float segment) {
            var (equalizedPoints, equalizedVCoordinates) = this.GetEqualizedPoints(segment);
            return new OpenCurve(equalizedPoints, equalizedVCoordinates, this.meridianCount, this.radius, preVirtualPoints: this.preVirtualPoints, postVirtualPoints: this.postVirtualPoints);
        }

        protected override Curve EqualizeInternal(float segment)
        {
            return this.Equalize(segment);
        }
    }
}
