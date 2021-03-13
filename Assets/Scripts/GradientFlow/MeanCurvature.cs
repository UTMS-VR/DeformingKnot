using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

// 暗黙の仮定：隣接する2点の間隔は一定
public class MeanCurvature : Flow
{
    public MeanCurvature(ref List<Curve> curveList, float lr=0.01f):base(ref curveList, lr)
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
                Vector3 gradient = this.curveList[i].points[j]-0.5f*(this.curveList[i].points[jn]+this.curveList[i].points[jp]);
                gradientList[i][j] = gradient;
            }
        }
    }
}