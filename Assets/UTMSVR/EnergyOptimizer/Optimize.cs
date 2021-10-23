using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using InputManager;

namespace EnergyOptimizer
{
    public class Optimize
    {
        private OculusTouch oculusTouch;
        private List<HandCurve> deformableCurves;
        private List<HandCurve> collisionCurves;
        private float segment;
        private LogicalButton button1;
        private LogicalButton button2;
        private float epsilon;
        private Flow curveFlow;
        private Elasticity elasticity;
        private float minSeg = 1e+20f;

        public Optimize(OculusTouch oculusTouch,
                        List<HandCurve> deformableCurves,
                        List<HandCurve> collisionCurves,
                        float segment,
                        float epsilon,
                        string flowClass,
                        LogicalButton button1,
                        LogicalButton button2)
        {
            this.deformableCurves = deformableCurves;
            List<Vector3[]> pointsList = new List<Vector3[]>();
            this.collisionCurves = collisionCurves;
            if (segment <= 0) {
                throw new System.Exception("segment must be positive");
            }
            this.segment = segment;
            this.oculusTouch = oculusTouch;

            for (int i = 0; i < this.deformableCurves.Count; i++)
            {
                if (!(this.deformableCurves[i].curve is ClosedCurve)) {
                    throw new System.Exception("curves must be closed");
                }
                this.deformableCurves[i].curve = this.deformableCurves[i].curve.Equalize(this.deformableCurves[i].segment);
                this.deformableCurves[i].MeshUpdate();
                pointsList.Add(this.deformableCurves[i].curve.GetPoints().ToArray());
                this.minSeg = Mathf.Min(this.minSeg,this.deformableCurves[i].segment);
            }

            if (flowClass == "Moebius")
            {
                curveFlow = new Moebius(pointsList, this.segment, 1e-04f);
            }
            else if (flowClass == "MeanCurvature")
            {
                curveFlow = new MeanCurvature(pointsList, this.segment, 0.05f);
            }

            elasticity = new Elasticity(pointsList, this.segment, 1e-01f);

            this.button1 = button1;
            this.button2 = button2;
            this.epsilon = epsilon;
        }

        public void Update()
        {
            if (this.oculusTouch.GetButton(this.button1) || this.oculusTouch.GetButton(this.button2))
            {
                if (!this.HaveInterSections())
                {
                    if (this.oculusTouch.GetButton(this.button1))
                    {
                        curveFlow.SetPoints(this.UpdatePoints, 0.0f);
                    }
                    else if (this.oculusTouch.GetButton(this.button2))
                    {
                        curveFlow.SetPoints(this.UpdatePoints, 0.95f);
                    }

                    while (elasticity.MaxError() > this.minSeg * 0.2f)
                    {
                        elasticity.SetPoints(this.UpdatePoints, 0.0f);
                    }
                    elasticity.ClearMomentum();
                }
            }

            if (this.oculusTouch.GetButtonUp(this.button2))
            {
                curveFlow.ClearMomentum();
            }

            foreach (HandCurve curve in this.deformableCurves)
            {
                curve.MeshUpdatePos();
                Graphics.DrawMesh(curve.mesh, Vector3.zero, Quaternion.identity, Curve.RainbowCurveMaterial, 0);
            }
        }

        private void UpdatePoints(List<Vector3[]> pointsList)
        {
            for (int i = 0; i < this.deformableCurves.Count; i++)
            {
                this.deformableCurves[i].curve.SetPoints(pointsList[i].ToList());
            }
        }

        private bool HaveInterSections()
        {
            int deformableCurvesCount = deformableCurves.Count;
            int collisionCurvesCount = collisionCurves.Count;

            for (int i = 0; i < deformableCurvesCount; i++)
            {
                if (deformableCurves[i].MinSegmentDist() < this.epsilon) return true;
                for (int j = i + 1; j < deformableCurvesCount; j++)
                {
                    if (deformableCurves[i].CurveDistance(deformableCurves[j]) < this.epsilon) return true;
                }
                for (int k = 0; k < collisionCurvesCount; k++)
                {
                    if (deformableCurves[i].CurveDistance(collisionCurves[k]) < this.epsilon) return true;
                }
            }

            return false;
        }

        public List<HandCurve> GetCurves()
        {
            return this.deformableCurves;
        }
    }
}
