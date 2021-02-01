using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using InputManager;
using ContextMenu;

public abstract class State
{
    protected OculusTouch oculusTouch;
    protected ContextMenu.ContextMenu contextMenu;
    protected List<Curve> curves;

    public State newState;

    public State(OculusTouch oculusTouch, ContextMenu.ContextMenu contextMenu, List<Curve> curves)
    {
        this.oculusTouch = oculusTouch;
        this.contextMenu = contextMenu;
        this.SetupMenu();
        this.curves = curves;
        this.newState = null;
    }

    protected abstract void SetupMenu();
    protected void ResetMenu()
    {
        List<MenuItem> removedItems = this.contextMenu.FindAllItems((item) => this.contextMenu.items.IndexOf(item) >= 3);
        foreach (MenuItem item in removedItems)
        {
            this.contextMenu.RemoveItem(item);
        }
    }

    public abstract void Update();

    public void Display()
    {
        foreach (Curve curve in this.curves)
        {
            Material material = curve.selected ? MakeMesh.SelectedCurveMaterial : MakeMesh.CurveMaterial;
            Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, material, 0);
        }
    }
}

public class BasicDeformation : State
{
    private List<Curve> preCurves;
    private Curve drawingCurve;
    private List<int> movingCurves;

    private LogicalButton changeState;
    private LogicalButton draw;
    private LogicalButton move;
    private LogicalButton select;
    private LogicalButton cut;
    private LogicalButton combine;
    private LogicalButton remove;
    private LogicalButton undo;

    public BasicDeformation(OculusTouch oculusTouch,
                            ContextMenu.ContextMenu contextMenu,
                            List<Curve> curves,
                            LogicalButton changeState = null,
                            LogicalButton draw = null,
                            LogicalButton move = null,
                            LogicalButton select = null,
                            LogicalButton cut = null,
                            LogicalButton combine = null,
                            LogicalButton remove = null,
                            LogicalButton undo = null)
        : base(oculusTouch, contextMenu, curves)
    {
        this.preCurves = new List<Curve>();
        this.drawingCurve = new Curve(new List<Vector3>(), false);
        this.movingCurves = new List<int>();

        if (changeState != null) this.changeState = changeState;
        else this.changeState = LogicalOVRInput.RawButton.LIndexTrigger;

        if (draw != null) this.draw = draw;
        else this.draw = LogicalOVRInput.RawButton.RIndexTrigger;

        if (move != null) this.move = move;
        else this.move = LogicalOVRInput.RawButton.RHandTrigger;

        if (select != null) this.select = select;
        else this.select = LogicalOVRInput.RawButton.A;

        if (cut != null) this.cut = cut;
        else this.cut = LogicalOVRInput.RawButton.B;

        if (combine != null) this.combine = combine;
        else this.combine = LogicalOVRInput.RawButton.X;

        if (remove != null) this.remove = remove;
        else this.remove = LogicalOVRInput.RawButton.Y;

        if (undo != null) this.undo = undo;
        else this.undo = LogicalOVRInput.RawButton.LHandTrigger;

        Curve.SetUp(base.oculusTouch, this.draw, this.move);
    }

    protected override void SetupMenu()
    {
        this.contextMenu.AddItem(new MenuItem("右人差し指 : 描画", () => {}));
        this.contextMenu.AddItem(new MenuItem("右中指 : 平行移動, 回転", () => {}));
        this.contextMenu.AddItem(new MenuItem("A : 選択", () => {}));
        this.contextMenu.AddItem(new MenuItem("B : 切断", () => {}));
        this.contextMenu.AddItem(new MenuItem("結合", () => {
            this.Combine();
        }));
        this.contextMenu.AddItem(new MenuItem("削除", () => {
            this.Remove();
        }));
        this.contextMenu.AddItem(new MenuItem("元に戻す", () => {
            this.Undo();
        }));
        this.contextMenu.AddItem(new MenuItem("連続変形", () => {
            this.ChangeState();
        }));
    }

    public override void Update()
    {
        if (this.ValidButtonInput())
        {
            this.DeepCopy();
            this.Draw();
            this.Move();
            this.Select();
            this.Cut();
            // this.Combine();
            //this.Remove();
            //this.Undo();
            
            /*if (base.oculusTouch.GetButtonDown(this.changeState))
            {
                bool closed = true;
                foreach (Curve curve in base.curves)
                {
                    if (!curve.closed) closed = false;
                }
                if (closed) return new SelectAutoOrManual(base.oculusTouch, base.contextMenu, base.curves);
            }*/
        }

        //return null;
    }

    private bool ValidButtonInput()
    {
        int valid = 0;

        //if (base.oculusTouch.GetButtonDown(this.changeState)) valid++;
        if (base.oculusTouch.GetButton(this.draw) || this.oculusTouch.GetButtonUp(this.draw)) valid++;
        if (base.oculusTouch.GetButton(this.move) || this.oculusTouch.GetButtonUp(this.move)) valid++;
        if (base.oculusTouch.GetButtonDown(this.select)) valid++;
        if (base.oculusTouch.GetButtonDown(this.cut)) valid++;
        //if (base.oculusTouch.GetButtonDown(this.combine)) valid++;
        //if (base.oculusTouch.GetButtonDown(this.remove)) valid++;
        //if (base.oculusTouch.GetButtonDown(this.undo)) valid++;

        return (valid == 1) ? true : false;
    }

    private void DeepCopy()
    {
        if (base.oculusTouch.GetButtonDown(this.changeState)
            || base.oculusTouch.GetButtonDown(this.draw)
            || base.oculusTouch.GetButtonDown(this.move)
            || base.oculusTouch.GetButtonDown(this.select)
            || base.oculusTouch.GetButtonDown(this.cut)
            || base.oculusTouch.GetButtonDown(this.combine)
            || base.oculusTouch.GetButtonDown(this.remove))
        {
            this.preCurves = new List<Curve>();

            foreach (Curve curve in this.curves)
            {
                this.preCurves.Add(curve.DeepCopy());
            }
        }
    }

    private void Draw()
    {
        Debug.Log("draw");
        this.drawingCurve.Draw();

        if (base.oculusTouch.GetButtonUp(this.draw))
        {
            if (this.drawingCurve.points.Count >= 2)
            {
                base.curves.Add(this.drawingCurve);
            }

            this.drawingCurve = new Curve(new List<Vector3>(), false);
        }
        
        if (this.drawingCurve.points.Count >= 2)
        {
            Graphics.DrawMesh(this.drawingCurve.mesh, this.drawingCurve.position, this.drawingCurve.rotation, MakeMesh.CurveMaterial, 0);
        }
    }

    private void Move()
    {
        Vector3 nowPosition = base.oculusTouch.GetPositionR();

        if (base.oculusTouch.GetButtonDown(this.move))
        {
            for (int i = 0; i < base.curves.Count; i++)
            {
                if (Curve.Distance(base.curves[i].points, nowPosition).Item2 < Curve.collision)
                {
                    this.movingCurves.Add(i);
                }
            }
        }

        foreach (int i in this.movingCurves)
        {
            base.curves[i].Move();
        }
        
        if (base.oculusTouch.GetButtonUp(this.move))
        {
            this.movingCurves = new List<int>();
        }
    }

    private void Select()
    {
        if (base.oculusTouch.GetButtonDown(this.select))
        {
            for (int i = 0; i < base.curves.Count; i++)
            {
                base.curves[i].Select();
            }
        }
    }

    private void Cut()
    {
        if (base.oculusTouch.GetButtonDown(this.cut))
        {
            List<Curve> selection = base.curves.Where(curve => curve.selected).ToList();

            if (selection.Count == 1)
            {
                List<Curve> newCurves = selection[0].Cut();

                if (newCurves.Count != 0)
                {
                    base.curves.Remove(selection[0]);
                    
                    foreach (Curve curve in newCurves)
                    {
                        base.curves.Add(curve);
                    }
                }
            }
        }
    }

    public void Combine()
    {
        //if (base.oculusTouch.GetButtonDown(this.combine))
        //{
            List<Curve> selection = base.curves.Where(curve => curve.selected).ToList();

            if (selection.Count == 1)
            {
                selection[0].Close();
            }
            else if (selection.Count == 2 && !selection[0].closed && !selection[1].closed)
            {
                List<Curve> newCurves = Curve.Combine(selection[0], selection[1]);

                if (newCurves.Count != 0)
                {
                    base.curves.Remove(selection[0]);
                    base.curves.Remove(selection[1]);
                    base.curves.Add(newCurves[0]);
                }
            }
        //}
    }

    private void Remove()
    {
        //if (base.oculusTouch.GetButtonDown(this.remove))
        //{
            base.curves = base.curves.Where(curve => !curve.selected).ToList();
        //}
    }

    private void Undo()
    {
        //if (base.oculusTouch.GetButtonDown(this.undo))
        //{
            base.curves = this.preCurves;
        //}
    }

    private void ChangeState()
    {
        bool closed = true;
        foreach (Curve curve in base.curves)
        {
            if (!curve.closed) closed = false;
        }
        if (closed)
        {
            base.ResetMenu();
            this.newState = new SelectAutoOrManual(base.oculusTouch, base.contextMenu, base.curves);
        }
    }
}

public class SelectAutoOrManual : State
{
    private LogicalButton changeState;
    private LogicalButton select;
    private LogicalButton energy;
    private LogicalButton pull;

    public SelectAutoOrManual(OculusTouch oculusTouch,
                              ContextMenu.ContextMenu contextMenu,
                              List<Curve> curves,
                              LogicalButton changeState = null,
                              LogicalButton select = null,
                              LogicalButton energy = null,
                              LogicalButton pull = null)
        : base(oculusTouch, contextMenu, curves)
    {
        if (changeState != null) this.changeState = changeState;
        else this.changeState = LogicalOVRInput.RawButton.LIndexTrigger;

        if (select != null) this.select = select;
        else this.select = LogicalOVRInput.RawButton.A;

        if (energy != null) this.energy = energy;
        else this.energy = LogicalOVRInput.RawButton.X;

        if (pull != null) this.pull = pull;
        else this.pull = LogicalOVRInput.RawButton.Y;
    }

    protected override void SetupMenu()
    {
        
    }

    public override void Update()
    {
        this.Select();

        List<Curve> selection = base.curves.Where(curve => curve.selected).ToList();

        if (base.oculusTouch.GetButtonDown(this.changeState))
        {
            // return new BasicDeformation(base.oculusTouch, base.contextMenu, base.curves);
        }
        if (base.oculusTouch.GetButtonDown(this.energy))
        {
            // return new AutomaticDeformation(base.oculusTouch, base.contextMenu, base.curves);
        }
        else if (base.oculusTouch.GetButtonDown(this.pull) && selection.Count == 1)
        {
            base.curves.Remove(selection[0]);
            // return new ManualDeformation(base.oculusTouch, base.contextMenu, base.curves, selection[0]);
        }

        // return null;
    }

    private void Select()
    {
        if (base.oculusTouch.GetButtonDown(this.select))
        {
            for (int i = 0; i < base.curves.Count; i++)
            {
                base.curves[i].Select();
            }
        }
    }
}

public class AutomaticDeformation : State
{
    private Optimize optimizer;
    private LogicalButton changeState;
    private LogicalButton button1;
    private LogicalButton button2;
    
    public AutomaticDeformation(OculusTouch oculusTouch,
                                ContextMenu.ContextMenu contextMenu,
                                List<Curve> curves,
                                LogicalButton changeState = null,
                                LogicalButton button1 = null,
                                LogicalButton button2 = null)
        : base(oculusTouch, contextMenu, curves.Where(curve => !curve.selected).ToList())
    {
        if (changeState != null) this.changeState = changeState;
        else this.changeState = LogicalOVRInput.RawButton.LIndexTrigger;

        if (button1 != null) this.button1 = button1;
        else this.button1 = LogicalOVRInput.RawButton.X;

        if (button2 != null) this.button2 = button2;
        else this.button2 = LogicalOVRInput.RawButton.Y;

        List<Curve> newCurves = curves.Where(curve => curve.selected).ToList();
        this.optimizer = new Optimize(oculusTouch, newCurves, this.button1, this.button2);
    }

    protected override void SetupMenu()
    {

    }

    public override void Update()
    {
        Debug.Log("Update");
        this.optimizer.Update(base.curves);

        if (base.oculusTouch.GetButtonDown(this.changeState))
        {
            base.curves = base.curves.Concat(this.optimizer.GetCurves()).ToList();
            // return new SelectAutoOrManual(base.oculusTouch, base.contextMenu, base.curves);
        }

        // return null;
    }
}

public class ManualDeformation : State
{
    private Knot deformingCurve;
    private LogicalButton changeState;
    
    public ManualDeformation(OculusTouch oculusTouch,
                             ContextMenu.ContextMenu contextMenu,
                             List<Curve> curves,
                             Curve curve,
                             LogicalButton changeState = null)
        : base(oculusTouch, contextMenu, curves)
    {
        this.deformingCurve = new Knot(curve.points,
                                       oculusTouch,
                                       meridian: curve.meridian,
                                       radius: curve.radius,
                                       distanceThreshold: curve.segment,
                                       collisionCurves: curves);
    
        if (changeState != null) this.changeState = changeState;
        else this.changeState = LogicalOVRInput.RawButton.LIndexTrigger;
    }

    protected override void SetupMenu()
    {
        
    }

    public override void Update()
    {
        this.deformingCurve.Update();

        if (base.oculusTouch.GetButtonDown(this.changeState))
        {
            base.curves.Add(new Curve(this.deformingCurve.GetPoints(), true, selected: true));
            // return new SelectAutoOrManual(base.oculusTouch, base.contextMenu, base.curves);
        }

        // return null;
    }
}