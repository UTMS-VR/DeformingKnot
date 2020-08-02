using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;

public static class Player
{
    public static Controller controller;

    public static void Draw(ref Curve drawingCurve, ref List<Curve> curves, OVRInput.RawButton button)
    {
        if (controller.GetButton(button))
        {
            drawingCurve.Draw();

            if (drawingCurve.positions.Count >= 2)
            {
                Graphics.DrawMesh(drawingCurve.mesh, drawingCurve.position, drawingCurve.rotation, defaultMaterial, 0);
            }
        }

        if (controller.GetButtonUp(button))
        {
            if (drawingCurve.positions.Count >= 2)
            {
                curves.Add(drawingCurve);
            }

            drawingCurve = new Curve(new List<Vector3>(), false);
        }
    }

    public static void Move(List<Curve> curves, List<int> movingCurves, OVRInput.RawButton button)
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();
        Quaternion nowRotation = controller.rightHand.GetRotation();

        if (controller.GetButtonDown(button))
        {
            for (int i = 0; i < curves.Count; i++)
            {
                if (Curve.Distance(curves[i].positions, nowPosition).Item2 < Curve.collision)
                {
                    movingCurves.Add(i);
                    curves[i].MoveSetUp();
                }
            }
        }

        if (controller.GetButton(button))
        {
            foreach (int i in movingCurves)
            {
                curves[i].Move();
            }
        }
        
        if (controller.GetButtonUp(button))
        {
            foreach (int i in movingCurves)
            {
                curves[i].MoveCleanUp();
            }

            movingCurves = new List<int>();
        }
    }

    public static void Select(List<Curve> curves, OVRInput.RawButton button)
    {
        if (controller.GetButtonDown(button))
        {
            for (int i = 0; i < curves.Count; i++)
            {
                curves[i].Select();
            }
        }
    }

    public static void Cut(ref List<Curve> curves, OVRInput.RawButton button)
    {
        if (controller.GetButtonDown(button))
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();
            List<Curve> removeCurves = new List<Curve>();
            List<Curve> addCurves = new List<Curve>();

            foreach (Curve curve in selection)
            {
                List<Curve> newCurves = curve.Cut();

                if (newCurves.Count != 0)
                {
                    removeCurves.Add(curve);
                    foreach (Curve newCurve in newCurves)
                    {
                        addCurves.Add(newCurve);
                    }
                }
            }

            RemoveAddCurve(ref curves, removeCurves, addCurves);
        }
    }

    public static void Combine(ref List<Curve> curves, OVRInput.RawButton button)
    {
        if (controller.GetButtonDown(button))
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();

            if (selection.Count == 1)
            {
                selection[0].Close();
            }
            else if (selection.Count == 2 && !selection[0].close && !selection[1].close)
            {
                List<Curve> newCurves = Curve.Combine(selection[0], selection[1]);

                if (newCurves.Count != 0)
                {
                    List<Curve> removeCurves = new List<Curve>();
                    removeCurves.Add(selection[0]);
                    removeCurves.Add(selection[1]);
                    RemoveAddCurve(ref curves, removeCurves, newCurves);
                }
            }
        }
    }

    public static void Remove(ref List<Curve> curves, OVRInput.RawButton button)
    {
        if (controller.GetButtonDown(button))
        {
            curves = curves.Where(curve => !curve.selected).ToList();
        }
    }

    public static void Display(List<Curve> curves)
    {
        foreach (Curve curve in curves)
        {
            Material material = curve.selected ? selectedMaterial : defaultMaterial;
            Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, material, 0);
        }
    }

    private static void RemoveAddCurve(ref List<Curve> curves, List<Curve> removeCurves, List<Curve> addCurves)
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
}
