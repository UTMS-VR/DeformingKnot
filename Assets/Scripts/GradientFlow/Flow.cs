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
    protected abstract void setGradient();
    protected int[] countList;
    protected List<Vector3[]> gradientList = new List<Vector3[]>();

    public Flow(ref List<Curve> curveList, float lr= 1e-04f)
    {
        this.curveList = curveList;
        this.lr = lr;
        this.countList = this.curveList.Select(curve => curve.points.Count).ToArray();
        for(int i =0; i < this.curveList.Count; i++)
        {
            this.gradientList.Add(new Vector3[this.countList[i]]);
        }
        this.clearGradient();
    }

    public void update(float alpha)
    {
        this.setGradient();
        for(int i =0; i < this.curveList.Count; i++)
        {
            for (int j = 0; j < this.countList[i]; j++)
            {
                this.curveList[i].momentum[j] = alpha * curveList[i].momentum[j] + this.gradientList[i][j];
                this.curveList[i].points[j] -= this.lr * this.curveList[i].momentum[j];
                // if (this.gradientList[i][j].magnitude > 0.001f) Debug.Log(this.gradientList[i][j].magnitude);
            }
        }
    }

    public void clearGradient()
    {
        for(int i =0; i < this.curveList.Count; i++)
        {
            for (int j = 0; j < this.countList[i]; j++)
            {
                this.gradientList[i][j] = Vector3.zero;
            }
        }
    }
}

