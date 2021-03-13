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
        private LogicalButton button1;
        private LogicalButton button2;
        private float epsilon;
        private Flow curveFlow;
        private Elasticity elasticity;
        private float minSeg = 1e+20f;

        public Optimize(OculusTouch oculusTouch,
                        List<HandCurve> deformableCurves,
                        List<HandCurve> collisionCurves,
                        float epsilon,
                        string flowClass,
                        LogicalButton button1,
                        LogicalButton button2)
        {
            this.deformableCurves = deformableCurves;
            this.collisionCurves = collisionCurves;
            this.oculusTouch = oculusTouch;

            for (int i = 0; i < this.deformableCurves.Count; i++)
            {
                this.deformableCurves[i].points = AdjustParameter.Equalize(this.deformableCurves[i].points, this.deformableCurves[i].segment, true);
                this.deformableCurves[i].MeshUpdate();
                this.minSeg = Mathf.Min(this.minSeg,this.deformableCurves[i].segment);
            }

            if (flowClass == "Moebius")
            {
                curveFlow = new Moebius(ref this.deformableCurves, 1e-04f);
            }
            else if (flowClass == "MeanCurvature")
            {
                curveFlow = new MeanCurvature(ref this.deformableCurves, 0.05f);
            }

            elasticity = new Elasticity(ref this.deformableCurves, 1e-01f);

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
                        curveFlow.Update(0.0f);
                    }
                    else if (this.oculusTouch.GetButton(this.button2))
                    {
                        curveFlow.Update(0.95f);
                    }

                    while (elasticity.MaxError() > this.minSeg * 0.2f)
                    {
                        elasticity.Update(0.0f);
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
                Graphics.DrawMesh(curve.mesh, Vector3.zero, Quaternion.identity, MakeMesh.SelectedCurveMaterial, 0);
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