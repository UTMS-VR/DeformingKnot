using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

namespace EnergyOptimizer
{
    // 暗黙の仮定：隣接する2点の間隔は一定
    public abstract class Flow
    {
        protected List<HandCurve> curveList;
        protected float lr; // longitude 64, segment 0.03f -> 1e-05f;
        protected abstract void SetGradient();
        protected int[] countList;
        protected List<Vector3[]> gradientList = new List<Vector3[]>();
        protected List<Vector3[]> momentum = new List<Vector3[]>();

        public Flow(ref List<HandCurve> curveList, float lr= 1e-04f)
        {
            this.curveList = curveList;
            this.lr = lr;
            this.countList = this.curveList.Select(curve => curve.points.Count).ToArray();
            for(int i =0; i < this.curveList.Count; i++)
            {
                this.gradientList.Add(new Vector3[this.countList[i]]);
                this.momentum.Add(new Vector3[this.countList[i]]);
            }
            this.ClearGradient();
            this.ClearMomentum();
        }

        public void Update(float alpha)
        {
            this.SetGradient();
            for(int i =0; i < this.curveList.Count; i++)
            {
                for (int j = 0; j < this.countList[i]; j++)
                {
                    this.momentum[i][j] = alpha * this.momentum[i][j] + this.gradientList[i][j];
                    this.curveList[i].points[j] -= this.lr * this.momentum[i][j];
                    // if (this.gradientList[i][j].magnitude > 0.001f) Debug.Log(this.gradientList[i][j].magnitude);
                }
            }
        }

        public void ClearGradient()
        {
            for(int i =0; i < this.curveList.Count; i++)
            {
                for (int j = 0; j < this.countList[i]; j++)
                {
                    this.gradientList[i][j] = Vector3.zero;
                }
            }
        }
        public void ClearMomentum()
        {
            for(int i =0; i < this.curveList.Count; i++)
            {
                for (int j = 0; j < this.countList[i]; j++)
                {
                    this.momentum[i][j] = Vector3.zero;
                }
            }
        }

    }
}