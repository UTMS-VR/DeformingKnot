using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

namespace EnergyOptimizer
{
    public class Elasticity : Flow
    {
        public float gtol = 0.0f;

        public Elasticity(ref List<Vector3[]> pointsList, float segment, float lr = 1e-04f) : base(ref pointsList, segment, lr)
        {
        }

        protected override void SetGradient()
        {
            for (int i = 0; i < this.pointsList.Count; i++)
            {
                for (int j = 0; j < this.countList[i]; j++)
                {
                    int jn = (j + 1) % this.countList[i];
                    int jp = (j + this.countList[i] - 1) % this.countList[i];
                    Vector3 next = this.pointsList[i][j] - this.pointsList[i][jn];
                    Vector3 prev = this.pointsList[i][j] - this.pointsList[i][jp];
                    next = (next.magnitude - this.segment) * next.normalized;
                    prev = (prev.magnitude - this.segment) * prev.normalized;
                    gradientList[i][j] = next + prev;
                }
            }
        }

        public float MaxError()
        {
            float max = 0.0f;
            for (int i = 0; i < this.pointsList.Count; i++)
            {
                for (int j = 0; j < this.countList[i]; j++)
                {
                    int jn = (j + 1) % this.countList[i];
                    max = Mathf.Max(Mathf.Abs(Vector3.Distance(this.pointsList[i][j], this.pointsList[i][jn]) - this.segment), max);
                }
            }
            return max;
        }

        // public float Energy()
        // {
        //     float energy = 0.0f;
        //
        //     for (int i = 0; i < this.len; i++)
        //     {
        //         energy += Mathf.Pow(Vector3.Distance(this.pos[i], this.pos[Succ(i)]) - this.seg, 2) / 2;
        //     }
        //
        //     return energy;
        // }
    }
}