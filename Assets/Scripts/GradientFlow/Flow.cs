using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

// 暗黙の仮定：隣接する2点の間隔は一定
public abstract class Flow
{
    protected List<Curve> curveList;
    protected float lr; // longitude 64, segment 0.03f -> 1e-05f;
    protected abstract List<List<Vector3>> Gradient();

    public Flow(ref List<Curve> curveList, float lr= 1e-04f)
    {
        this.curveList = curveList;
        this.lr = lr;
    }

    public void update(float alpha)
    {
        List<List<Vector3>> gradientList = Gradient();        
        for(int i =0; i < this.curveList.Count; i++)
        {
            for (int j = 0; j < this.curveList[i].points.Count; j++)
            {
                this.curveList[i].momentum[j] = alpha * this.curveList[i].momentum[j] + gradientList[i][j];
                this.curveList[i].points[j] -= this.lr * this.curveList[i].momentum[j];
                // if (this.gradientList[i][j].magnitude > 0.001f) Debug.Log(this.gradientList[i][j].magnitude);
            }
        }
    }
}

