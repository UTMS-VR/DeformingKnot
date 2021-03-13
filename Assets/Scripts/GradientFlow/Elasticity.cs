using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

public class Elasticity : Flow
{
    public float gtol = 0.0f;

    public Elasticity(ref List<HandCurve> curveList, float lr=1e-04f) : base(ref curveList, lr)
    {
    }

    protected override void SetGradient()
    {
        for (int i = 0; i < this.curveList.Count; i++)
        {
            for (int j = 0; j < this.countList[i]; j++)
            {
                int jn = (j + 1) % this.countList[i];
                int jp = (j + this.countList[i] - 1) % this.countList[i];
                Vector3 next = this.curveList[i].points[j] - this.curveList[i].points[jn];
                Vector3 prev = this.curveList[i].points[j] - this.curveList[i].points[jp];
                next = (next.magnitude - this.curveList[i].segment) * next.normalized;
                prev = (prev.magnitude - this.curveList[i].segment) * prev.normalized;
                gradientList[i][j] = next + prev;
            }
        }
    }

    public float MaxError()
    {
        float max = 0.0f;
        for (int i = 0; i < this.curveList.Count; i++)
        {
            for (int j = 0; j < this.countList[i]; j++)
            {
                int jn = (j + 1) % this.countList[i];
                max = Mathf.Max(Mathf.Abs(Vector3.Distance(this.curveList[i].points[j], this.curveList[i].points[jn]) - this.curveList[i].segment), max);
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