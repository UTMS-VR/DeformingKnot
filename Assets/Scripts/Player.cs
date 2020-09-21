using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using DebugUtil;

public enum State
{
    BasicDeform,
    ContiDeform
}

public static class Player
{
    public static Controller controller;
    public static ButtonConfig button;

    public static void SetUp(Controller argController, ButtonConfig argButton)
    {
        controller = argController;
        button = argButton;
    }

    public static void DeepCopy(List<Curve> curves, ref List<Curve> preCurves)
    {
        if (controller.GetButtonDown(button.changeState)
            || controller.GetButtonDown(button.draw)
            || controller.GetButtonDown(button.move)
            || controller.GetButtonDown(button.select)
            || controller.GetButtonDown(button.cut)
            || controller.GetButtonDown(button.combine)
            || controller.GetButtonDown(button.remove))
        {
            preCurves = new List<Curve>();

            foreach (Curve curve in curves)
            {
                preCurves.Add(curve.DeepCopy());
            }
        }
    }

    public static void ChangeState(ref List<Curve> curves, ref State state, ref Knot deformingCurve)
    {
        if (state == State.BasicDeform)
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();

            if (controller.GetButtonDown(button.changeState) && selection.Count == 1 && selection[0].close)
            {
                state = State.ContiDeform;
                deformingCurve = new Knot(selection[0].positions, controller,
                                          meridian: selection[0].meridian,
                                          radius: selection[0].radius,
                                          distanceThreshold: selection[0].segment,
                                          collisionCurves: new List<Curve>());
                curves.Remove(selection[0]);
            }
        }
        else if (state == State.ContiDeform)
        {
            if (controller.GetButtonDown(button.changeState))
            {
                state = State.BasicDeform;
                curves.Add(new Curve(deformingCurve.GetPoints(), true, selected: true));
            }
        }
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

    public static void Move(List<Curve> curves, ref List<int> movingCurves)
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
        if (controller.GetButtonDown(button.select))
        {
            for (int i = 0; i < curves.Count; i++)
            {
                curves[i].Select();
            }
        }
    }

    public static void Cut(ref List<Curve> curves)
    {
        if (controller.GetButtonDown(button.cut))
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();

            if (selection.Count == 1)
            {
                List<Curve> newCurves = selection[0].Cut();

                if (newCurves.Count != 0)
                {
                    curves.Remove(selection[0]);
                    
                    foreach (Curve curve in newCurves)
                    {
                        curves.Add(curve);
                    }
                }
            }
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
                    curves.Remove(selection[0]);
                    curves.Remove(selection[1]);
                    curves.Add(newCurves[0]);
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

    /*public static void Optimize(List<Curve> curves)
    {
        if (controller.GetButton(button.optimize))
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();

            foreach (Curve curve in selection)
            {
                if (controller.GetButtonDown(button.optimize))
                {
                    float tempSegment = AdjustParameter.TemporarySegment(curve.positions, curve.segment, curve.close);
                    AdjustParameter.Equalize(ref curve.positions, curve.segment, curve.close);
                    curve.segment = tempSegment;
                    curve.MomentumInitialize();
                }

                curve.Optimize();
            }
        }

        if (controller.GetButtonDown(button.remove))
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();

            foreach (Curve curve in selection)
            {
                float tempSegment = AdjustParameter.TemporarySegment(curve.positions, curve.segment, curve.close);
                AdjustParameter.Equalize(ref curve.positions, curve.segment, curve.close);
                curve.segment = tempSegment;
                curve.MomentumInitialize();
                curve.MeshUpdate();
                curve.MeshAtPositionsUpdate();
            }
        }

        if (controller.GetButtonDown(button.optimize))
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();

            foreach (Curve curve in selection)
            {
                DiscreteMoebius optimizer = new DiscreteMoebius(curve);
                optimizer.MomentumFlow();
                curve.MeshUpdate();
                curve.MeshAtPositionsUpdate();
            }
        }

        if (controller.GetButtonDown(button.undo))
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();

            foreach (Curve curve in selection)
            {
                while (true)
                {
                    Elasticity optimizer2 = new Elasticity(curve);
                    if (optimizer2.MaxError() < curve.segment * 1.0f) break;
                    optimizer2.Flow();
                }

                curve.MeshUpdate();
                curve.MeshAtPositionsUpdate();
            }
        }
    }*/

    public static void Undo(ref List<Curve> curves, List<Curve> preCurves)
    {
        if (controller.GetButtonDown(button.undo))
        {
            curves = preCurves;
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
}
