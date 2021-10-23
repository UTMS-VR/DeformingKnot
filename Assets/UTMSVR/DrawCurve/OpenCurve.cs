using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace DrawCurve {
    public class OpenCurve : Curve {
        public OpenCurve(List<Vector3> points, List<float> vCoordinates, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius) : base(points, vCoordinates, meridianCount, radius) {}
        public OpenCurve(List<Vector3> points, (float start, float end) vRange, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius) : base(points, vRange, meridianCount, radius) {}
        public OpenCurve(List<Vector3> points, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius) : base(points, meridianCount, radius) {}

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
            return new OpenCurve(newPoints, newVCoordinates, this.meridianCount, this.radius);
        }

        public float DistanceOfFirstAndLast() {
            return Vector3.Distance(this.points.First(), this.points.Last());
        }

        public new OpenCurve Reversed()
        {
            // this.points.Reverse() だと in-place に反転される
            List<Vector3> newPoints = this.points.AsEnumerable().Reverse().ToList();
            List<float> newVCoordinates = this.vCoordinates.AsEnumerable().Reverse().ToList();
            return new OpenCurve(newPoints, newVCoordinates, this.meridianCount, this.radius);
        }

        protected override Curve ReversedInternal() {
            return this.Reversed();
        }

        public new OpenCurve Equalize(float segment) {
            var (equalizedPoints, equalizedVCoordinates) = this.GetEqualizedPoints(segment);
            return new OpenCurve(equalizedPoints, equalizedVCoordinates, this.meridianCount, this.radius);
        }

        protected override Curve EqualizeInternal(float segment)
        {
            return this.Equalize(segment);
        }
    }
}
