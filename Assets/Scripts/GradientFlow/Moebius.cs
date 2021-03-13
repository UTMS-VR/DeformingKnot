using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

// 暗黙の仮定：隣接する2点の間隔は一定
public class Moebius : Flow
{
    public Moebius(ref List<HandCurve> curveList, float lr=1e-04f):base(ref curveList, lr)
    {
    }

    protected override void SetGradient()
    {
        for (int i1 = 0; i1 < this.curveList.Count; i1++)
        {
            for (int j1 = 0; j1 < this.countList[i1]; j1++)
            {
                Vector3 gradient = Vector3.zero;
                for (int j2 = 1; j2 < this.countList[i1]; j2++)
                {
                    int j3 = (j1 + j2) % this.countList[i1];
                    Vector3 first = this.CoulombDiff(this.curveList[i1].points[j1], this.curveList[i1].points[j3])
                                    * Mathf.Pow(this.curveList[i1].segment, 2);
                    Vector3 second = this.Coulomb(this.curveList[i1].points[j1], this.curveList[i1].points[j3])
                                     * 2 * this.curveList[i1].segment * this.SegmentDiff(i1, j1);
                    gradient += 2 * (first + second);
                }
                for (int i2 = 1; i2 < this.curveList.Count; i2++)
                {
                    int i3 = (i1 + i2) % this.curveList.Count;
                    for (int j2 = 0; j2 < this.countList[i3]; j2++)
                    {
                        Vector3 first = this.CoulombDiff(this.curveList[i1].points[j1], this.curveList[i3].points[j2])
                                        * this.curveList[i1].segment * this.curveList[i3].segment;
                        Vector3 second = this.Coulomb(this.curveList[i1].points[j1], this.curveList[i3].points[j2])
                                         * this.SegmentDiff(i1, j1) * this.curveList[i3].segment;
                        gradient += 2 * (first + second);
                    }
                }
                this.gradientList[i1][j1] = gradient;
            }
        }
    }

    // private float Energy()
    // {
    //     float energy = 0.0f;
    //
    //     for (int i1 = 0; i1 < this.count; i1++)
    //     {
    //         for (int j1 = 0; j1 < this.countList[i1]; j1++)
    //         {
    //             for (int j2 = 1; j2 < this.countList[i1]; j2++)
    //             {
    //                 int j3 = (j1 + j2) % this.countList[i1];
    //                 energy += this.Coulomb(this.curveList[i1].points[i1][j1], this.curveList[i1].points[i1][j3])
    //                           * Mathf.Pow(this.segmentList[i1], 2);
    //             }
    //
    //             for (int i2 = 1; i2 < this.count; i2++)
    //             {
    //                 int i3 = (i1 + i2) % this.count;
    //                 for (int j2 = 0; j2 < this.countList[i3]; j2++)
    //                 {
    //                     energy += this.Coulomb(this.curveList[i1].points[i1][j1], this.curveList[i1].points[i3][j2])
    //                               * this.segmentList[i1] * this.segmentList[i3];
    //                 }
    //             }
    //         }
    //     }
    //
    //     return energy;
    // }

    private float Coulomb(Vector3 v, Vector3 w)
    {
        return 1 / Mathf.Pow(Vector3.Distance(v, w), 2);
    }

    private Vector3 CoulombDiff(Vector3 v, Vector3 w)
    {
        return - 2 * (v - w) / Mathf.Pow(Vector3.Distance(v, w), 4);
    }

    private float ArcLength(List<Vector3> points)
    {
        float arc = 0.0f;
        int count = points.Count;

        for (int i = 0; i < count; i++)
        {
            arc += Vector3.Distance(points[i], points[(i + 1) % count]);
        }

        return arc;
    }

    private Vector3 SegmentDiff(int i, int j)
    {
        int jp = (j + 1) % this.countList[i];
        int jn = (j + this.countList[i] - 1) % this.countList[i];
        Vector3 tangentp = (this.curveList[i].points[j] - this.curveList[i].points[jp]).normalized;
        Vector3 tangentn = (this.curveList[i].points[j] - this.curveList[i].points[jn]).normalized;
        return (tangentp + tangentn) / this.countList[i];
    }
}