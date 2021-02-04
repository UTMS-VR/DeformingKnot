using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PullCurve;
using DrawCurve;

public class IntersectionManager
{
    private List<Curve> deformableCurves;
    private List<Curve> fixedCurves;
    private List<CurveDistanceHandler> selfHandlers;
    private List<List<CurveDistanceHandler>> deformableHandlers;
    private List<List<CurveDistanceHandler>> fixedHandlers;
    private float epsilon;

    public IntersectionManager(List<Curve> deformableCurves, List<Curve> fixedCurves, float epsilon)
    {
        this.deformableCurves = deformableCurves;
        this.fixedCurves = fixedCurves;
        this.epsilon = epsilon;

        int count = this.deformableCurves.Count;
        this.selfHandlers = new List<CurveDistanceHandler>();
        foreach (Curve curve in this.deformableCurves)
        {
            this.selfHandlers.Add((CurveDistanceHandler)new SelfCurveDistanceHandler(
                curve.points.Count,
                curve.closed,
                this.epsilon,
                (k, l) => this.CurveSegmentDistance(curve, curve, k, l)
            ));
        }

        this.deformableHandlers = new List<List<CurveDistanceHandler>>();
        for (int i = 0; i < count; i++)
        {
            this.deformableHandlers.Add(new List<CurveDistanceHandler>());
            for (int j = 0; j < count; j++)
            {
                if (j <= i)
                {
                    this.deformableHandlers[i].Add(null);
                }
                else
                {
                    this.deformableHandlers[i].Add((CurveDistanceHandler)new TwoCurvesDistanceHandler(
                        this.deformableCurves[i].points.Count,
                        this.deformableCurves[j].points.Count,
                        this.deformableCurves[i].closed,
                        this.deformableCurves[j].closed,
                        this.epsilon,
                        (k, l) => this.CurveSegmentDistance(this.deformableCurves[i], this.deformableCurves[j], k, l)
                    ));
                }
            }
        }

        this.fixedHandlers = new List<List<CurveDistanceHandler>>();
        for (int i = 0; i < count; i++)
        {
            this.fixedHandlers.Add(new List<CurveDistanceHandler>());
            foreach (Curve curve in this.fixedCurves)
            {
                this.fixedHandlers[i].Add((CurveDistanceHandler)new TwoCurvesDistanceHandler(
                    this.deformableCurves[i].points.Count,
                    curve.points.Count,
                    this.deformableCurves[i].closed,
                    curve.closed,
                    this.epsilon,
                    (k, l) => this.CurveSegmentDistance(this.deformableCurves[i], curve, k, l)
                ));
            }
        }
    }

    public void Update()
    {
        int pullableCount = this.deformableCurves.Count;
        int fixedCount = this.fixedCurves.Count;

        for (int i = 0; i < pullableCount; i++)
        {
            this.selfHandlers[i].Update((k, l) => this.CurveSegmentDistance(
                this.deformableCurves[i], this.deformableCurves[i], k, l
            ));
        }

        for (int i = 0; i < pullableCount; i++)
        {
            for (int j = 0; j < pullableCount; j++)
            {
                if (this.deformableHandlers[i][j] != null)
                {
                    this.deformableHandlers[i][j].Update((k, l) => this.CurveSegmentDistance(
                        this.deformableCurves[i], this.deformableCurves[j], k, l
                    ));
                }
            }
        }

        for (int i = 0; i < pullableCount; i++)
        {
            for (int j = 0; j < fixedCount; j++)
            {
                this.fixedHandlers[i][j].Update((k, l) => this.CurveSegmentDistance(
                    this.deformableCurves[i], this.fixedCurves[j], k, l
                ));
            }
        }
    }

    public bool HaveInterSections()
    {
        int pullableCount = this.deformableCurves.Count;
        int fixedCount = this.fixedCurves.Count;

        for (int i = 0; i < pullableCount; i++)
        {
            float d = this.selfHandlers[i].Distance((k, l) => this.CurveSegmentDistance(
                this.deformableCurves[i], this.deformableCurves[i], k, l
            ));
            if (d < this.epsilon) return true;
        }

        for (int i = 0; i < pullableCount; i++)
        {
            for (int j = 0; j < pullableCount; j++)
            {
                if (this.deformableHandlers[i][j] != null)
                {
                    float d = this.deformableHandlers[i][j].Distance((k, l) => this.CurveSegmentDistance(
                        this.deformableCurves[i], this.deformableCurves[j], k, l
                    ));
                    if (d < this.epsilon) return true;
                }
            }
        }

        for (int i = 0; i < pullableCount; i++)
        {
            for (int j = 0; j < fixedCount; j++)
            {
                float d = this.fixedHandlers[i][j].Distance((k, l) => this.CurveSegmentDistance(
                    this.deformableCurves[i], this.fixedCurves[j], k, l
                ));
                if (d < this.epsilon) return true;
            }
        }

        return false;
    }

    private float CurveSegmentDistance(Curve curve1, Curve curve2, int index1, int index2)
    {
        int count1 = curve1.points.Count;
        int count2 = curve2.points.Count;
        return SegmentDist.SSDist(
            curve1.points[index1], curve1.points[(index1 + 1) % count1],
            curve2.points[index2], curve2.points[(index2 + 1) % count2]
        );
    }
}
