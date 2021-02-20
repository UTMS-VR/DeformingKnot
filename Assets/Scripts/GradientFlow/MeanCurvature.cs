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

    protected override List<List<Vector3>> Gradient()
    {
        List<List<Vector3>> gradientList = new List<List<Vector3>>();

        for (int i1 = 0; i1 < this.curveList.Count; i1++)
        {
            gradientList.Add(new List<Vector3>());
            for (int j = 0; j < this.curveList[i1].points.Count; j++)
            {
                Vector3 gradient = new Vector3(0f,0f,0f);
                int jn = (j + 1) % this.curveList[i1].points.Count;
                int jp = (j + this.curveList[i1].points.Count - 1) % this.curveList[i1].points.Count;
                gradient += 0.5f*(this.curveList[i1].points[jn]+this.curveList[i1].points[jp]);                
                gradient -=this.curveList[i1].points[j];
                gradientList[i1].Add(gradient);
            }
        }

        return gradientList;
    }
}