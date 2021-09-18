using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

namespace EnergyOptimizer
{
    // 暗黙の仮定：隣接する2点の間隔は一定
    public class MeanCurvature : Flow
    {
        public MeanCurvature(ref List<Vector3[]> pointsList, float segment, float lr = 0.01f) : base(ref pointsList, segment, lr)
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
                    Vector3 gradient = this.pointsList[i][j]-0.5f*(this.pointsList[i][jn]+this.pointsList[i][jp]);
                    gradientList[i][j] = gradient;
                }
            }
        }
    }
}