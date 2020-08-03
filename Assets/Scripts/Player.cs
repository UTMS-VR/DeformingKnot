using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using DebugUtil;

public static class Player
{
    public static Controller controller;
    public static ButtonConfig button;

    public static void SetUp(Controller argController, ButtonConfig argButton)
    {
        controller = argController;
        button = argButton;
    }

    public static void Draw(ref Curve curve, ref List<Curve> curves)
    {
        curve.Draw();

        if (controller.GetButtonUp(button.draw))
        {
            if (curve.positions.Count >= 2)
            {
                curves.Add(curve);
            }

            curve = new Curve(new List<Vector3>(), false);
        }

        if (curve.positions.Count >= 2)
        {
            Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, MakeMesh.CurveMaterial, 0);
        }
    }

    public static void Move(List<Curve> curves, List<int> movingCurves)
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();

        if (controller.GetButtonDown(button.move))
        {
            for (int i = 0; i < curves.Count; i++)
            {
                if (Curve.Distance(curves[i].positions, nowPosition).Item2 < Curve.collision)
                {
                    movingCurves.Add(i);
                }
            }
        }

        foreach (int i in movingCurves)
        {
            curves[i].Move();
        }
        
        if (controller.GetButtonUp(button.move))
        {
            movingCurves = new List<int>();
        }
    }

    public static void Select(List<Curve> curves)
    {
        for (int i = 0; i < curves.Count; i++)
        {
            curves[i].Select();
        }
    }

    public static void Cut(ref List<Curve> curves)
    {
        if (controller.GetButtonDown(button.cut))
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

    public static void Combine(ref List<Curve> curves)
    {
        if (controller.GetButtonDown(button.combine))
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

    public static void Remove(ref List<Curve> curves)
    {
        if (controller.GetButtonDown(button.remove))
        {
            curves = curves.Where(curve => !curve.selected).ToList();
        }
    }

    public static void Display(List<Curve> curves)
    {
        foreach (Curve curve in curves)
        {
            Material material = curve.selected ? MakeMesh.SelectedCurveMaterial : MakeMesh.CurveMaterial;
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
