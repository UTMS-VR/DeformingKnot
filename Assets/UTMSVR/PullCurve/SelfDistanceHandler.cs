using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PullCurve
{
    using DistFunc = Func<int, int, float>;

    abstract class SelfDistanceHandler
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

    class TrivialSelfDistanceHandler : SelfDistanceHandler
    {
        int length;
        bool closed;

        public TrivialSelfDistanceHandler(int length, bool closed)
        {
            this.length = length;
            this.closed = closed;
        }

        protected override IEnumerable<(int i, int j)> CollidablePairs()
        {
            int n = this.length;
            int endi = closed ? n - 3 : n - 4;

            for (int i = 0; i <= endi; i++)
            {
                int endj = (i == 0 || !closed) ? n - 2 : n - 1;
                for (int j = i + 2; j <= endj; j++)
                {
                    yield return (i, j);
                }
            }
        }

        public override void Update(DistFunc dist) { }
    }

    class SimpleSelfDistanceHandler : SelfDistanceHandler
    {
        int length;
        bool closed;
        private List<(int i, int j)> collidablePairs;
        private float epsilon;
        private int updateFrame = 10;
        private int frameCount = 0;

        public SimpleSelfDistanceHandler(
            int length, bool closed,
            float epsilon, DistFunc dist
            )
        {
            this.length = length;
            this.closed = closed;
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
            int n = this.length;
            int endi = closed ? n - 3 : n - 4;

            for (int i = 0; i <= endi; i++)
            {
                int endj = (i == 0 || !closed) ? n - 2 : n - 1;
                for (int j = i + 2; j <= endj; j++)
                {
                    float d = dist(i, j);
                    if (d < this.epsilon * this.updateFrame)
                    {
                        collidablePairs.Add((i, j));
                    }
                }
            }
            return collidablePairs;
        }
    }
}
