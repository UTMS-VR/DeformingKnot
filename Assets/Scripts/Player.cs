using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using InputManager;

public enum State
{
    BasicDeform,
    ContiDeform
}

public class Player
{
    public Controller controller;
    public State state;
    public List<Curve> curves;
    private List<Curve> preCurves;
    private Curve drawingCurve;
    private List<int> movingCurves;
    public Knot deformingCurve;
    public Optimize optimizer;

    public Player(Controller controller)
    {
        this.controller = controller;
        this.state = State.BasicDeform;
        this.curves = new List<Curve>();
        this.preCurves = new List<Curve>();
        this.drawingCurve = new Curve(new List<Vector3>(), false);
        this.movingCurves = new List<int>();
    }

    public void DeepCopy()
    {
        if (this.controller.oculusTouch.GetButtonDown(this.controller.changeState)
            || this.controller.oculusTouch.GetButtonDown(this.controller.draw)
            || this.controller.oculusTouch.GetButtonDown(this.controller.move)
            || this.controller.oculusTouch.GetButtonDown(this.controller.select)
            || this.controller.oculusTouch.GetButtonDown(this.controller.cut)
            || this.controller.oculusTouch.GetButtonDown(this.controller.combine)
            || this.controller.oculusTouch.GetButtonDown(this.controller.remove))
        {
            this.preCurves = new List<Curve>();

            foreach (Curve curve in this.curves)
            {
                this.preCurves.Add(curve.DeepCopy());
            }
        }
    }

    public void ChangeState()
    {
        if (this.state == State.BasicDeform)
        {
            bool closed = true;
            foreach (Curve curve in this.curves)
            {
                if (!curve.closed) closed = false;
            }
            if (closed) this.state = State.ContiDeform;
        }
        else if (this.state == State.ContiDeform)
        {
            this.state = State.BasicDeform;
        }
    }

    public void SelectHowToDeform()
    {
        List<Curve> selection = this.curves.Where(curve => curve.selected).ToList();

        if (this.controller.oculusTouch.GetButtonDown(this.controller.pull) && selection.Count == 1)
        {
            this.deformingCurve = new Knot(selection[0].points, this.controller.oculusTouch,
                                        meridian: selection[0].meridian,
                                        radius: selection[0].radius,
                                        distanceThreshold: selection[0].segment,
                                        collisionCurves: new List<Curve>());
            this.curves.Remove(selection[0]);
        }
        else if (this.controller.oculusTouch.GetButtonDown(this.controller.energy))
        {
            this.optimizer = new Optimize(this.curves,
                                          this.controller.oculusTouch,
                                          LogicalOVRInput.RawButton.X,
                                          LogicalOVRInput.RawButton.Y);
        }
    }

    public void Draw()
    {
        this.drawingCurve.Draw();

        if (this.controller.oculusTouch.GetButtonUp(this.controller.draw))
        {
            if (this.drawingCurve.points.Count >= 2)
            {
                this.curves.Add(this.drawingCurve);
            }

            this.drawingCurve = new Curve(new List<Vector3>(), false);
        }
        
        if (this.drawingCurve.points.Count >= 2)
        {
            Graphics.DrawMesh(this.drawingCurve.mesh, this.drawingCurve.position, this.drawingCurve.rotation, MakeMesh.CurveMaterial, 0);
        }
    }

    public void Move()
    {
        Vector3 nowPosition = this.controller.oculusTouch.GetPositionR();

        if (this.controller.oculusTouch.GetButtonDown(this.controller.move))
        {
            for (int i = 0; i < this.curves.Count; i++)
            {
                if (Curve.Distance(this.curves[i].points, nowPosition).Item2 < Curve.collision)
                {
                    this.movingCurves.Add(i);
                }
            }
        }

        foreach (int i in this.movingCurves)
        {
            this.curves[i].Move();
        }
        
        if (this.controller.oculusTouch.GetButtonUp(this.controller.move))
        {
            this.movingCurves = new List<int>();
        }
    }

    public void Select()
    {
        if (this.controller.oculusTouch.GetButtonDown(this.controller.select))
        {
            for (int i = 0; i < this.curves.Count; i++)
            {
                this.curves[i].Select();
            }
        }
    }

    public void Cut()
    {
        if (this.controller.oculusTouch.GetButtonDown(this.controller.cut))
        {
            List<Curve> selection = this.curves.Where(curve => curve.selected).ToList();

            if (selection.Count == 1)
            {
                List<Curve> newCurves = selection[0].Cut();

                if (newCurves.Count != 0)
                {
                    this.curves.Remove(selection[0]);
                    
                    foreach (Curve curve in newCurves)
                    {
                        this.curves.Add(curve);
                    }
                }
            }
        }
    }

    public void Combine()
    {
        if (this.controller.oculusTouch.GetButtonDown(this.controller.combine))
        {
            List<Curve> selection = this.curves.Where(curve => curve.selected).ToList();

            if (selection.Count == 1)
            {
                selection[0].Close();
            }
            else if (selection.Count == 2 && !selection[0].closed && !selection[1].closed)
            {
                List<Curve> newCurves = Curve.Combine(selection[0], selection[1]);

                if (newCurves.Count != 0)
                {
                    this.curves.Remove(selection[0]);
                    this.curves.Remove(selection[1]);
                    this.curves.Add(newCurves[0]);
                }
            }
        }
    }

    public void Remove()
    {
        if (this.controller.oculusTouch.GetButtonDown(this.controller.remove))
        {
            this.curves = this.curves.Where(curve => !curve.selected).ToList();
        }
    }

    /*public static void Optimize(List<Curve> curves)
    {
        if (oculusTouch.GetButton(button.optimize))
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();

            foreach (Curve curve in selection)
            {
                if (oculusTouch.GetButtonDown(button.optimize))
                {
                    float tempSegment = AdjustParameter.TemporarySegment(curve.points, curve.segment, curve.closed);
                    AdjustParameter.Equalize(ref curve.points, curve.segment, curve.closed);
                    curve.segment = tempSegment;
                    curve.MomentumInitialize();
                }

                curve.Optimize();
            }
        }

        if (oculusTouch.GetButtonDown(button.remove))
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();

            foreach (Curve curve in selection)
            {
                float tempSegment = AdjustParameter.TemporarySegment(curve.points, curve.segment, curve.closed);
                AdjustParameter.Equalize(ref curve.points, curve.segment, curve.closed);
                curve.segment = tempSegment;
                curve.MomentumInitialize();
                curve.MeshUpdate();
                curve.MeshAtPointsUpdate();
            }
        }

        if (oculusTouch.GetButtonDown(button.optimize))
        {
            List<Curve> selection = curves.Where(curve => curve.selected).ToList();

            foreach (Curve curve in selection)
            {
                DiscreteMoebius optimizer = new DiscreteMoebius(curve);
                optimizer.MomentumFlow();
                curve.MeshUpdate();
                curve.MeshAtPointsUpdate();
            }
        }

        if (oculusTouch.GetButtonDown(button.undo))
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
                curve.MeshAtPointsUpdate();
            }
        }
    }*/

    public void Undo()
    {
        if (this.controller.oculusTouch.GetButtonDown(this.controller.undo))
        {
            this.curves = this.preCurves;
        }
    }

    public void Display()
    {
        foreach (Curve curve in this.curves)
        {
            Material material = curve.selected ? MakeMesh.SelectedCurveMaterial : MakeMesh.CurveMaterial;
            Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, material, 0);
        }
    }
}
