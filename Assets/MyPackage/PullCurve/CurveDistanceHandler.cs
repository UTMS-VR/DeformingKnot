using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PullCurve
{
    using DistFunc = Func<int, int, float>;

    public abstract class CurveDistanceHandler
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

        protected abstract List<(int i, int j)> CollidablePairs();

        public abstract void Update(DistFunc dist);
    }

    public class TrivialCurveDistanceHandler : CurveDistanceHandler
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

        protected override List<(int i, int j)> CollidablePairs()
        {
            List<(int i, int j)> pairs = new List<(int i, int j)>();
            int end1 = this.closed1 ? this.length1 - 1 : this.length1 - 2;
            int end2 = this.closed2 ? this.length2 - 1 : this.length2 - 2;

            for (int i = 0; i <= end1; i++)
            {
                for (int j = 0; j <= end2; j++)
                {
                    pairs.Add((i, j));
                }
            }

            return pairs;
        }

        public override void Update(DistFunc dist) { }
    }

    public class TwoCurvesDistanceHandler : CurveDistanceHandler
    {
        private int length1;
        private int length2;
        private bool closed1;
        private bool closed2;
        private List<(int i, int j)> collidablePairs;
        private float epsilon;
        private int updateFrame = 10;
        private int frameCount = 0;

        public TwoCurvesDistanceHandler(
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

        protected override List<(int i, int j)> CollidablePairs()
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
            List<(int i, int j)> pairs = new List<(int i, int j)>();
            int end1 = this.closed1 ? this.length1 - 1 : this.length1 - 2;
            int end2 = this.closed2 ? this.length2 - 1 : this.length2 - 2;

            for (int i = 0; i <= end1; i++)
            {
                for (int j = 0; j <= end2; j++)
                {
                    if (this.Collidable(i, j, dist)) pairs.Add((i, j));
                }
            }

            return pairs;
        }

        private bool Collidable(int i, int j, DistFunc dist)
        {
            return dist(i, j) < this.epsilon * (this.updateFrame + 1);
        }
    }

    public class OneCurveDistanceHandler : CurveDistanceHandler
    {
        private int length;
        private bool closed;
        private List<(int i, int j)> collidablePairs;
        private float epsilon;
        private int updateFrame = 10;
        private int frameCount = 0;

        public OneCurveDistanceHandler(
            int length,
            bool closed,
            float epsilon,
            DistFunc dist
        )
        {
            this.length = length;
            this.closed = closed;
            this.epsilon = epsilon;
            this.collidablePairs = this.FindCollidablePairs(dist);
        }

        protected override List<(int i, int j)> CollidablePairs()
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
            List<(int i, int j)> pairs = new List<(int i, int j)>();
            int endi = this.closed ? this.length - 1 : this.length - 2;
            for (int i = 0; i <= endi; i++)
            {
                int endj = (i != 0 && this.closed) ? this.length - 1 : this.length - 2;
                for (int j = i + 2; j <= endj; j++)
                {
                    if (this.Collidable(i, j, dist)) pairs.Add((i, j));
                }
            }

            return pairs;
        }

        private bool Collidable(int i, int j, DistFunc dist)
        {
            return dist(i, j) < this.epsilon * (this.updateFrame + 1);
        }
    }

    public class EfficientCurveDistanceHandler : CurveDistanceHandler
    {
        private int length;
        private int initial;
        private int terminal;
        private bool closed;
        private List<(int i, int j)> collidablePairs;
        private float epsilon;
        private int updateFrame = 10;
        private int frameCount = 0;

        public EfficientCurveDistanceHandler(
            int length,
            int initial,
            int terminal,
            bool closed,
            float epsilon,
            DistFunc dist
        )
        {
            this.length = length;
            this.initial = initial;
            this.terminal = terminal;
            this.closed = closed;
            this.epsilon = epsilon;
            this.collidablePairs = this.FindCollidablePairs(dist);
        }

        protected override List<(int i, int j)> CollidablePairs()
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
            List<(int i, int j)> pairs = new List<(int i, int j)>();
            for (int i = initial; i < terminal; i++)
            {
                int end1 = (i == initial) ? initial - 2 : initial - 1;
                for (int j = 0; j <= end1; j++)
                {
                    if (this.Collidable(i, j, dist)) pairs.Add((i, j));
                }

                for (int j = i + 2; j < terminal; j++)
                {
                    if (this.Collidable(i, j, dist)) pairs.Add((i, j));
                }

                int end2 = (i == terminal - 1) ? terminal + 1 : terminal;
                int end3 = (i != 0 && this.closed) ? this.length - 1 : this.length - 2;
                for (int j = end2; j <= end3; j++)
                {
                    if (this.Collidable(i, j, dist)) pairs.Add((i, j));
                }
            }

            return pairs;
        }

        private bool Collidable(int i, int j, DistFunc dist)
        {
            return dist(i, j) < this.epsilon * (this.updateFrame + 1);
        }
    }
}