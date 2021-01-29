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
    private LogicalButton button1;
    private LogicalButton button2;

    public Optimize(OculusTouch oculusTouch,
                    List<Curve> newCurves,
                    LogicalButton button1,
                    LogicalButton button2)
    {
        this.newCurves = newCurves;
        this.oculusTouch = oculusTouch;

        for (int i = 0; i < this.newCurves.Count; i++)
        {
            this.newCurves[i].points = AdjustParameter.Equalize(this.newCurves[i].points, this.newCurves[i].segment, true);
            this.newCurves[i].MomentumInitialize();
        }

        this.button1 = button1;
        this.button2 = button2;
    }

    public void Update(List<Curve> collisionCurves)
    {
        float epsilon = 0.2f;
        int count = this.newCurves.Count;

        bool intersection = false;
        for (int i = 0; i < count; i++)
        {
            float segment = this.newCurves[i].segment;
            if (this.newCurves[i].points.Count >= 4
                && PullableCurve.MinSegmentDist(this.newCurves[i].points, true) <= segment * epsilon)
            {
                intersection = true;
                break;
            }

            for (int j = i + 1; j < count; j++)
            {
                if (PullableCurve.CurveDistance(this.newCurves[i].points, this.newCurves[j].points, true, true) <= segment * epsilon)
                intersection = true;
                break;
            }

            foreach (Curve curve in collisionCurves)
            {
                if (PullableCurve.CurveDistance(this.newCurves[i].points, curve.points, true, curve.closed) <= segment * epsilon)
                intersection = true;
                break;
            }
        }

        if (!intersection)
        {
            List<List<Vector3>> pointsList = this.newCurves.Select(curve => curve.points).ToList();
            List<List<Vector3>> momentumList = this.newCurves.Select(curve => curve.momentum).ToList();
            Moebius moebius = new Moebius(pointsList, momentumList);
            if (this.oculusTouch.GetButton(this.button1))
            {
                moebius.Flow();
            }
            else if (this.oculusTouch.GetButton(this.button2))
            {
                moebius.MomentumFlow();
            }

            foreach (Curve curve in this.newCurves)
            {
                while (true)
                {
                    Elasticity elasticity = new Elasticity(curve.points, curve.momentum, curve.segment);
                    if (elasticity.MaxError() < curve.segment * 0.1f) break;
                    elasticity.Flow();
                }
            }
        }

        if (this.oculusTouch.GetButtonUp(this.button2))
        {
            foreach (Curve curve in this.newCurves)
            {
                curve.MomentumInitialize();
            }
        }

        foreach (Curve curve in this.newCurves)
        {
            curve.MeshUpdate();
            Graphics.DrawMesh(curve.mesh, Vector3.zero, Quaternion.identity, MakeMesh.SelectedCurveMaterial, 0);
        }
    }

    public List<Curve> GetCurves()
    {
        return this.newCurves;
    }
}
