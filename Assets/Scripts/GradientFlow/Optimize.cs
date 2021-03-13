using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using InputManager;

public class Optimize
{
    private OculusTouch oculusTouch;
    private List<Curve> newCurves;
    private List<Curve> collisionCurves;
    // private IntersectionManager intersectionManager;
    private LogicalButton button1;
    private LogicalButton button2;
    private float epsilon;
    private Flow curveFlow;
    private Elasticity elasticity;
    private float minSeg = 1e+20f;

    public Optimize(OculusTouch oculusTouch,
                    List<Curve> newCurves,
                    List<Curve> collisionCurves,
                    float epsilon,
                    string flowClass,
                    LogicalButton button1,
                    LogicalButton button2)
    {
        this.newCurves = newCurves;
        this.collisionCurves = collisionCurves;
        this.oculusTouch = oculusTouch;
        Debug.Log("Opt");

        for (int i = 0; i < this.newCurves.Count; i++)
        {
            this.newCurves[i].points = AdjustParameter.Equalize(this.newCurves[i].points, this.newCurves[i].segment, true);
            this.newCurves[i].MeshUpdate();
            this.minSeg = Mathf.Min(this.minSeg,this.newCurves[i].segment);
        }
        // this.intersectionManager = new IntersectionManager(this.newCurves, this.collisionCurves, epsilon);

        if (flowClass == "Moebius")
        {
            curveFlow = new Moebius(ref this.newCurves, 1e-04f);
        }
        else if (flowClass == "MeanCurvature")
        {
            // curveFlow = new MeanCurvature(ref this.newCurves, 0.05f);
        }

        elasticity = new Elasticity(ref this.newCurves, 1e-01f);

        this.button1 = button1;
        this.button2 = button2;
        this.epsilon = epsilon;
    }

    public void Update()
    {
        if (this.oculusTouch.GetButton(this.button1) || this.oculusTouch.GetButton(this.button2))
        {
            //this.intersectionManager.Update();
            if (!this.HaveInterSections()) //this.intersectionManager.HaveInterSections())
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

        foreach (Curve curve in this.newCurves)
        {
            curve.MeshUpdate();
            Graphics.DrawMesh(curve.mesh, Vector3.zero, Quaternion.identity, MakeMesh.SelectedCurveMaterial, 0);
        }
    }

    private bool HaveInterSections()
    {
        int selectedCurvesCount = newCurves.Count;
        int unselectedCurvesCount = collisionCurves.Count;

        for (int i = 0; i < selectedCurvesCount; i++)
        {
            if (newCurves[i].MinSegmentDist() < this.epsilon) return true;
            for (int j = i + 1; j < selectedCurvesCount; j++)
            {
                if (newCurves[i].CurveDistance(newCurves[j]) < this.epsilon) return true;
            }
            for (int k = 0; k < unselectedCurvesCount; k++)
            {
                if (newCurves[i].CurveDistance(collisionCurves[k]) < this.epsilon) return true;
            }
        }

        return false;
    }

    public List<Curve> GetCurves()
    {
        return this.newCurves;
    }
}