using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;

public class Player
{
    private Controller controller;
    private float collision = 0.05f;

    private List<Vector3> Positions;
    private List<Vector3> NewPositions;
    private BezierCurve PartialCurve;

    public Player(Controller controller)
    {
        this.controller = controller;
    }

    public void Draw(List<Curve> curves)
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();

        if (controller.GetButtonDown(OVRInput.RawButton.RIndexTrigger))
        {
            curves.Add(new Curve(false, true, false, false, new List<Vector3>(), Vector3.zero, Quaternion.identity));
        }

        foreach (Curve curve in curves)
        {
            if (controller.GetButton(OVRInput.RawButton.RIndexTrigger) && curve.isBeingDrawn)
            {
                if (curve.positions.Count == 0)
                {
                    curve.positions.Add(nowPosition);
                }
                else if (Vector3.Distance(nowPosition, curve.positions.Last()) >= curve.segment)
                {
                    curve.positions.Add(nowPosition);
                    curve.MeshUpdate();
                }
            }
            else if (controller.GetButtonUp(OVRInput.RawButton.RIndexTrigger))
            {
                curve.isBeingDrawn = false;
            }
        }
    }

    public void Move(List<Curve> curves)
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();
        Quaternion nowRotation = controller.rightHand.GetRotation();

        foreach (Curve curve in curves)
        {
            if (controller.GetButtonDown(OVRInput.RawButton.RHandTrigger) && Dist(curve.positions, nowPosition).Item2 < collision)
            {
                curve.isBeingMoved = true;
                curve.positions = curve.positions.Select(v => v - nowPosition).Select(v => Quaternion.Inverse(nowRotation) * v).ToList();
                curve.MeshUpdate();
            }

            if (controller.GetButton(OVRInput.RawButton.RHandTrigger) && curve.isBeingMoved)
            {
                curve.position = nowPosition;
                curve.rotation = nowRotation;
            }
            else if (controller.GetButtonUp(OVRInput.RawButton.RHandTrigger))
            {
                curve.isBeingMoved = false;
                curve.positions = curve.positions.Select(v => curve.rotation * v).Select(v => v + curve.position).ToList();
                curve.MeshUpdate();
                curve.position = Vector3.zero;
                curve.rotation = Quaternion.identity;
            }
        }
    }

    public void Select(List<Curve> curves)
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();

        foreach (Curve curve in curves)
        {
            if (Dist(curve.positions, nowPosition).Item2 < collision)
            {
                curve.isSelected = !curve.isSelected;
            }
        }
    }

    public void Cut(ref List<Curve> curves)
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();
        List<Curve> removeCurves = new List<Curve>();
        List<Curve> addCurves = new List<Curve>();

        foreach (Curve curve in curves)
        {
            Tuple<int, float> Distance = Dist(curve.positions, nowPosition);
            if (curve.isSelected && Distance.Item2 < collision)
            {
                int num = Distance.Item1;
                if (curve.isClosed)
                {
                    Curve newCurve = CutKnot(curve, num);
                    removeCurves.Add(curve);
                    addCurves.Add(newCurve);
                }
                else if (2 <= num && num <= curve.positions.Count - 3)
                {
                    List<Curve> newCurves = CutCurve(curve, num);
                    removeCurves.Add(curve);
                    addCurves.Add(newCurves[0]);
                    addCurves.Add(newCurves[1]);
                }
            }
        }

        RemoveAddCurve(ref curves, removeCurves, addCurves);
    }

    public void Close(List<Curve> curves)
    {
        foreach (Curve curve in curves)
        {
            if (curve.isSelected && Vector3.Distance(curve.positions.First(), curve.positions.Last()) < collision)
            {
                curve.isClosed = !curve.isClosed;
                curve.MeshUpdate();
            }
        }
    }

    public void Combine(ref List<Curve> curves)
    {
        List<Curve> removeCurves = new List<Curve>();
        List<Curve> addCurves = new List<Curve>();

        List<Curve> selectedCurves = curves.Where(curve => curve.isSelected).ToList();
        List<Vector3> positions0 = selectedCurves[0].positions;
        List<Vector3> positions1 = selectedCurves[1].positions;
        AdjustOrientation(ref positions0, ref positions1);

        if (Vector3.Distance(positions0.Last(), positions1.First()) < collision)
        {
            foreach (Vector3 v in positions1)
            {
                positions0.Add(v);
            }

            bool isClosed = Vector3.Distance(positions0.First(), positions1.Last()) < collision ? true : false;
            Curve newCurve = new Curve(false, false, false, isClosed, positions0, Vector3.zero, Quaternion.identity);

            removeCurves.Add(selectedCurves[0]);
            removeCurves.Add(selectedCurves[1]);
            addCurves.Add(newCurve);
        }

        RemoveAddCurve(ref curves, removeCurves, addCurves);
    }

    public void Optimize(List<Curve> curves)
    {
        foreach(Curve curve in curves)
        {
            if (curve.isSelected && curve.isClosed)
            {
                /*if (curve.momentum == null)
                {
                    curve.momentum = new List<Vector3>();

                    for (int i = 0; i < curve.positions.Count; i++)
                    {
                        curve.momentum.Add(Vector3.zero);
                    }
                }*/

                // SGD.Step(curve);
                curve.positions = KnotEnergy.Flow(curve.positions);
                DrawCurve.AdjustParameter.Equalize(ref curve.positions, curve.segment, curve.isClosed);
                curve.MeshUpdate();
            }
        }
    }

    public void Remove(ref List<Curve> curves)
    {
        curves = curves.Where(curve => !curve.isSelected).ToList();
    }

    private Tuple<int, float> Dist(List<Vector3> positions, Vector3 position)
    {
        List<Vector3> relPositions = positions.Select(v => v - position).ToList();

        int num = 0;
        float min = relPositions[0].magnitude;

        for (int i = 0; i < relPositions.Count - 1; i++)
        {
            if (relPositions[i + 1].magnitude < min)
            {
                num = i + 1;
                min = relPositions[i + 1].magnitude;
            }
        }

        return new Tuple<int, float>(num, min);
    }

    public void MakeBezierCurve(ref List<Curve> curves, int n_interval)
    {
        List<Curve> selectedCurves = curves.Where(curve => curve.isSelected).ToList();

        foreach (Curve curve in selectedCurves)
        {
            NewPositions = new List<Vector3>();
            Positions = curve.GetPositions();
            int N = Positions.Count;
            int q = N / n_interval;
            int r = N % n_interval;

            for (int i = 0; i < q; i++)
            {
                int s = i * n_interval;
                PartialCurve = new BezierCurve(Positions.GetRange(s, n_interval));
                for (int j = 0; j < n_interval; j++)
                {
                    NewPositions.Add(PartialCurve.GetPosition((float) j / n_interval));
                }
            }

            // remain part
            PartialCurve = new BezierCurve(Positions.GetRange(N - r, r));
            for (int j = 0; j < r; j++)
            {
                NewPositions.Add(PartialCurve.GetPosition((float) j / r));
            }

            curve.UpdatePositions(NewPositions);
            curve.MeshUpdate();
            Debug.Log("Updated Knot");

            // unselect
            curve.isSelected = !curve.isSelected;
        }
    }

    private void RemoveAddCurve(ref List<Curve> curves, List<Curve> removeCurves, List<Curve> addCurves)
    {
        foreach (Curve curve in removeCurves)
        {
            curves.Remove(curve);
        }

        foreach (Curve curve in addCurves)
        {
            curves.Add(curve);
        }
    }

    private Curve CutKnot(Curve curve, int num)
    {
        List<Vector3> newPositions = new List<Vector3>();

        for (int i = num + 1; i < curve.positions.Count; i++)
        {
            newPositions.Add(curve.positions[i]);
        }

        for (int i = 0; i < num; i++)
        {
            newPositions.Add(curve.positions[i]);
        }

        Curve cutKnot = new Curve(false, false, false, false, newPositions, curve.position, curve.rotation);

        return cutKnot;
    }

    private List<Curve> CutCurve(Curve curve, int num)
    {
        List<Vector3> newPositions1 = new List<Vector3>();
        List<Vector3> newPositions2 = new List<Vector3>();
        List<Curve> cutCurve = new List<Curve>();

        for (int i = 0; i < num; i++)
        {
            newPositions1.Add(curve.positions[i]);
        }

        for (int i = num + 1; i < curve.positions.Count; i++)
        {
            newPositions2.Add(curve.positions[i]);
        }

        Curve newCurve1 = new Curve(false, false, false, false, newPositions1, curve.position, curve.rotation);
        Curve newCurve2 = new Curve(false, false, false, false, newPositions2, curve.position, curve.rotation);

        cutCurve.Add(newCurve1);
        cutCurve.Add(newCurve2);

        return cutCurve;
    }

    private void AdjustOrientation(ref List<Vector3> positions0, ref List<Vector3> positions1)
    {
        if (Vector3.Distance(positions0.Last(), positions1.Last()) < collision)
        {
            positions1.Reverse();
        }
        else if (Vector3.Distance(positions0.First(), positions1.First()) < collision)
        {
            positions0.Reverse();
        }
        else if (Vector3.Distance(positions0.First(), positions1.Last()) < collision)
        {
            positions0.Reverse();
            positions1.Reverse();
        }
    }
}
