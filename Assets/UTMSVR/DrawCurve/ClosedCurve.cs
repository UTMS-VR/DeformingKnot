using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace DrawCurve {
    public class ClosedCurve : Curve {
        public ClosedCurve(List<Vector3> points, List<float> vCoordinates, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius) : base(points, vCoordinates, meridianCount, radius) {}
        public ClosedCurve(List<Vector3> points, (float start, float end) vRange, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius) : base(points, vRange, meridianCount, radius) {}
        public ClosedCurve(List<Vector3> points, int meridianCount = Curve.defaultMeridianCount, float radius = Curve.defaultRadius) : base(points, meridianCount, radius) {}

        public override bool closed {
            get {
                return true;
            }
        }

        public override Vector3 this[int i] {
            get {
                int iModCount = i % this.points.Count;
                if (iModCount < 0) {
                    iModCount += this.points.Count;
                }
                return this.points[iModCount];
            }
        }

        override public OpenCurve Open() {
            return new OpenCurve(this.points, this.vCoordinates, this.meridianCount, this.radius);
        }

        override public ClosedCurve Close() {
            return this;
        }

        public ClosedCurve Shift(int n) // 0 <= n < this.points.Count
        {
            if (!(0 <= n && n < this.points.Count)) {
                throw new Exception($"Required 0 <= n < this.points.Count, but n={n} was given");
            }

            List<Vector3> newPoints = new List<Vector3>();
            List<float> newVCoordinates = new List<float>();

            for (int i = n; i < this.points.Count; i++)
                {
                    newPoints.Add(this.points[i]);
                    newVCoordinates.Add(this.vCoordinates[i]);
                }

            int vLength = this.GetVLength();
            for (int i = 0; i < n; i++)
                {
                    newPoints.Add(this.points[i]);
                    newVCoordinates.Add(this.vCoordinates[i] + vLength);
                    // vCoordinates.Last - vCoordinates.First がおよそ 1.0 であることを仮定している
                }

            return new ClosedCurve(newPoints, newVCoordinates, this.meridianCount, this.radius);
        }

        public new ClosedCurve Reversed()
        {
            // this.points.Reverse() だと in-place に反転される
            List<Vector3> newPoints = this.points.AsEnumerable().Reverse().ToList();
            List<float> newVCoordinates = this.vCoordinates.AsEnumerable().Reverse().ToList();
            return new ClosedCurve(newPoints, newVCoordinates, this.meridianCount, this.radius);
        }

        protected override Curve ReversedInternal() {
            return this.Reversed();
        }

        public new ClosedCurve Equalize(float segment) {
            var (equalizedPoints, equalizedVCoordinates) = this.GetEqualizedPoints(segment);
            return new ClosedCurve(equalizedPoints, equalizedVCoordinates, this.meridianCount, this.radius);
        }

        protected override Curve EqualizeInternal(float segment)
        {
            return this.Equalize(segment);
        }
    }
}
