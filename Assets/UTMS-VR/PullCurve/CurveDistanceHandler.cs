using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PullCurve
{
    using DistFunc = Func<int, int, float>;

    abstract class CurveDistanceHandler
    {
        public float Distance(DistFunc dist)
        {
            float min = float.PositiveInfinity;

            foreach (var (i, j) in this.CollidablePairs())
            {
                float d = dist(i, j);
                if (d < min) min = d;
            }

            return min;
        }

        protected abstract IEnumerable<(int i, int j)> CollidablePairs();

        public abstract void Update(DistFunc dist);
    }

    class TrivialCurveDistanceHandler : CurveDistanceHandler
    {
        int length1;
        int length2;
        bool closed1;
        bool closed2;

        public TrivialCurveDistanceHandler(int length1, int length2, bool closed1, bool closed2)
        {
            this.length1 = length1;
            this.length2 = length2;
            this.closed1 = closed1;
            this.closed2 = closed2;
        }

        protected override IEnumerable<(int i, int j)> CollidablePairs()
        {
            int end1 = this.closed1 ? this.length1 - 1 : this.length1 - 2;
            int end2 = this.closed2 ? this.length2 - 1 : this.length2 - 2;

            for (int i = 0; i <= end1; i++)
            {
                for (int j = 0; j <= end2; j++)
                {
                    yield return (i, j);
                }
            }
        }

        public override void Update(DistFunc dist) { }
    }

    class SimpleCurveDistanceHandler : CurveDistanceHandler
    {
        private int length1;
        private int length2;
        private bool closed1;
        private bool closed2;
        private List<(int i, int j)> collidablePairs;
        private float epsilon;
        private int updateFrame = 10;
        private int frameCount = 0;

        public SimpleCurveDistanceHandler(
            int length1, int length2,
            bool closed1, bool closed2,
            float epsilon, DistFunc dist
            )
        {
            this.length1 = length1;
            this.length2 = length2;
            this.closed1 = closed1;
            this.closed2 = closed2;
            this.epsilon = epsilon;
            this.collidablePairs = this.FindCollidablePairs(dist);
        }

        protected override IEnumerable<(int i, int j)> CollidablePairs()
        {
            return this.collidablePairs;
        }

        public override void Update(DistFunc dist)
        {
            this.frameCount += 1;
            if (this.frameCount % this.updateFrame == 0)
            {
                this.collidablePairs = this.FindCollidablePairs(dist);
            }
        }

        private List<(int i, int j)> FindCollidablePairs(DistFunc dist)
        {
            var collidablePairs = new List<(int i, int j)>();
            int end1 = this.closed1 ? this.length1 - 1 : this.length1 - 2;
            int end2 = this.closed2 ? this.length2 - 1 : this.length2 - 2;

            for (int i = 0; i <= end1; i++)
            {
                for (int j = 0; j <= end2; j++)
                {
                    float d = dist(i, j);
                    if (d < this.epsilon * (this.updateFrame + 1))
                    {
                        collidablePairs.Add((i, j));
                    }
                }
            }
            return collidablePairs;
        }
    }
}