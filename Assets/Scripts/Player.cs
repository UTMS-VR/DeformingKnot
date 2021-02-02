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
    protected int NumberOfDefaultItems = 4;
    protected int NumberOfUnselectableItems;
    protected List<Curve> curves;
    protected float segment = 0.03f;
    protected float epsilon;
    public State newState;

    public State(OculusTouch oculusTouch, ContextMenu.ContextMenu contextMenu, List<Curve> curves)
    {
        this.oculusTouch = oculusTouch;
        this.contextMenu = contextMenu;
        this.SetupMenu();
        this.curves = curves;
        this.epsilon = this.segment * 0.2f;
        this.newState = null;
    }

    protected abstract void SetupMenu();
    protected void ResetMenu()
    {
        List<MenuItem> removedItems = this.contextMenu.FindAllItems((item) => this.contextMenu.items.IndexOf(item) >= this.NumberOfDefaultItems);
        foreach (MenuItem item in removedItems)
        {
            this.contextMenu.RemoveItem(item);
        }
    }

    public void RestrictCursorPosition()
    {
        while (this.contextMenu.SelectedIndex() < this.NumberOfUnselectableItems)
        {
            this.contextMenu.IncreaseSelectedIndex();
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

    private LogicalButton draw;
    private LogicalButton move;
    private LogicalButton select;
    private LogicalButton cut;
    private LogicalButton comfirm;

    public BasicDeformation(OculusTouch oculusTouch,
                            ContextMenu.ContextMenu contextMenu,
                            List<Curve> curves,
                            LogicalButton changeState = null,
                            LogicalButton draw = null,
                            LogicalButton move = null,
                            LogicalButton select = null,
                            LogicalButton cut = null,
                            LogicalButton comfirm = null)
        : base(oculusTouch, contextMenu, curves)
    {
        base.NumberOfUnselectableItems = 9;
        this.preCurves = base.curves;
        this.drawingCurve = new Curve(new List<Vector3>(), false);
        this.movingCurves = new List<int>();

        if (draw != null) this.draw = draw;
        else this.draw = LogicalOVRInput.RawButton.RIndexTrigger;

        if (move != null) this.move = move;
        else this.move = LogicalOVRInput.RawButton.RHandTrigger;

        if (select != null) this.select = select;
        else this.select = LogicalOVRInput.RawButton.A;

        if (cut != null) this.cut = cut;
        else this.cut = LogicalOVRInput.RawButton.B;

        if (comfirm != null) this.comfirm = comfirm;
        else this.comfirm = LogicalOVRInput.RawButton.X;

        Curve.SetUp(base.oculusTouch, this.draw, this.move);
    }

    protected override void SetupMenu()
    {
        this.contextMenu.AddItem(new MenuItem("右人差し指 : 描画", () => {}));
        this.contextMenu.AddItem(new MenuItem("右中指 : 平行移動, 回転", () => {}));
        this.contextMenu.AddItem(new MenuItem("A : 選択", () => {}));
        this.contextMenu.AddItem(new MenuItem("B : 選択中の曲線を切断 (曲線は1つ選択)", () => {}));
        this.contextMenu.AddItem(new MenuItem("", () => {}));
        this.contextMenu.AddItem(new MenuItem("選択中の曲線を結合 (曲線は1つか2つ選択)", () => {
            this.Combine();
        }));
        this.contextMenu.AddItem(new MenuItem("選択中の曲線を削除", () => {
            this.Remove();
        }));
        this.contextMenu.AddItem(new MenuItem("元に戻す", () => {
            this.Undo();
        }));
        this.contextMenu.AddItem(new MenuItem("連続変形 (全て閉曲線にしておく)", () => {
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
        }
    }

    private bool ValidButtonInput()
    {
        int valid = 0;

        if (base.oculusTouch.GetButton(this.draw) || this.oculusTouch.GetButtonUp(this.draw)) valid++;
        if (base.oculusTouch.GetButton(this.move) || this.oculusTouch.GetButtonUp(this.move)) valid++;
        if (base.oculusTouch.GetButtonDown(this.select)) valid++;
        if (base.oculusTouch.GetButtonDown(this.cut)) valid++;
        if (base.oculusTouch.GetButtonDown(this.comfirm)) valid++; 

        return (valid == 1) ? true : false;
    }

    private void DeepCopy()
    {
        if (base.oculusTouch.GetButtonDown(this.draw)
            || base.oculusTouch.GetButtonDown(this.move)
            || base.oculusTouch.GetButtonDown(this.select)
            || base.oculusTouch.GetButtonDown(this.cut)
            || (base.oculusTouch.GetButtonDown(this.comfirm) && (this.contextMenu.SelectedIndex() != 11)))
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
        this.drawingCurve.Draw();

        if (base.oculusTouch.GetButtonUp(this.draw))
        {
            if (this.drawingCurve.points.Count >= 2)
            {
                base.curves.Add(this.drawingCurve);
            }

            this.drawingCurve = new Curve(new List<Vector3>(), false, segment: base.segment);
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
    }

    private void Remove()
    {
        base.curves = base.curves.Where(curve => !curve.selected).ToList();
    }

    private void Undo()
    {
        base.curves = this.preCurves;
    }

    private void ChangeState()
    {
        bool closed = true;
        foreach (Curve curve in base.curves)
        {
            if (!curve.closed) closed = false;
        }

        bool intersection = false;
        for (int i = 0; i < base.curves.Count; i++)
        {
            for (int j = i + 1; j < base.curves.Count; j++)
            {
                if (base.curves[i].CurveDistance(base.curves[j]) < base.epsilon)
                {
                    intersection = true;
                    break;
                }
            }
            if (intersection) break;
        }

        if (closed && !intersection)
        {
            base.ResetMenu();
            this.newState = new SelectAutoOrManual(base.oculusTouch, base.contextMenu, base.curves);
        }
    }
}

public class SelectAutoOrManual : State
{
    private LogicalButton select;

    public SelectAutoOrManual(OculusTouch oculusTouch,
                              ContextMenu.ContextMenu contextMenu,
                              List<Curve> curves,
                              LogicalButton select = null)
        : base(oculusTouch, contextMenu, curves)
    {
        base.NumberOfUnselectableItems = 6;
        if (select != null) this.select = select;
        else this.select = LogicalOVRInput.RawButton.A;
    }

    protected override void SetupMenu()
    {
        this.contextMenu.AddItem(new MenuItem("A : 選択", () => {}));
        this.contextMenu.AddItem(new MenuItem("", () => {}));

        this.contextMenu.AddItem(new MenuItem("自動変形", () => {
            base.ResetMenu();
            this.newState = new AutomaticDeformation(base.oculusTouch, base.contextMenu, base.curves);
        }));

        this.contextMenu.AddItem(new MenuItem("手動変形 (曲線は1つ選択)", () => {
            List<Curve> selection = base.curves.Where(curve => curve.selected).ToList();
            if (selection.Count == 1)
            {
                base.ResetMenu();
                base.curves.Remove(selection[0]);
                this.newState = new ManualDeformation(base.oculusTouch, base.contextMenu, base.curves, selection[0]);
            }
        }));

        this.contextMenu.AddItem(new MenuItem("戻る", () => {
            base.ResetMenu();
            this.newState = new BasicDeformation(base.oculusTouch, base.contextMenu, base.curves);
        }));
    }

    public override void Update()
    {
        this.Select();
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
    private LogicalButton button1;
    private LogicalButton button2;
    
    public AutomaticDeformation(OculusTouch oculusTouch,
                                ContextMenu.ContextMenu contextMenu,
                                List<Curve> curves,
                                LogicalButton button1 = null,
                                LogicalButton button2 = null)
        : base(oculusTouch, contextMenu, curves.Where(curve => !curve.selected).ToList())
    {
        base.NumberOfUnselectableItems = 7;

        if (button1 != null) this.button1 = button1;
        else this.button1 = LogicalOVRInput.RawButton.A;

        if (button2 != null) this.button2 = button2;
        else this.button2 = LogicalOVRInput.RawButton.B;

        List<Curve> newCurves = curves.Where(curve => curve.selected).ToList();
        this.optimizer = new Optimize(oculusTouch, newCurves, this.button1, this.button2);
    }

    protected override void SetupMenu()
    {
        this.contextMenu.AddItem(new MenuItem("A : 自動変形 (遅い)", () => {}));
        this.contextMenu.AddItem(new MenuItem("B : 自動変形 (速い)", () => {}));
        this.contextMenu.AddItem(new MenuItem("", () => {}));
        this.contextMenu.AddItem(new MenuItem("戻る", () => {
            base.curves = base.curves.Concat(this.optimizer.GetCurves()).ToList();
            base.ResetMenu();
            this.newState = new SelectAutoOrManual(base.oculusTouch, base.contextMenu, base.curves);
        }));
    }

    public override void Update()
    {
        this.optimizer.Update(base.curves);
    }
}

public class ManualDeformation : State
{
    private Knot deformingCurve;
    
    public ManualDeformation(OculusTouch oculusTouch,
                             ContextMenu.ContextMenu contextMenu,
                             List<Curve> curves,
                             Curve curve)
        : base(oculusTouch, contextMenu, curves)
    {
        base.NumberOfUnselectableItems = 8;
        this.deformingCurve = new Knot(curve.points,
                                       oculusTouch,
                                       meridian: curve.meridian,
                                       radius: curve.radius,
                                       distanceThreshold: curve.segment,
                                       collisionCurves: curves);
    }

    protected override void SetupMenu()
    {
        this.contextMenu.AddItem(new MenuItem("", () => {}));
        this.contextMenu.AddItem(new MenuItem("A : 決定", () => {}));
        this.contextMenu.AddItem(new MenuItem("B : キャンセル", () => {}));
        this.contextMenu.AddItem(new MenuItem("", () => {}));
        this.contextMenu.AddItem(new MenuItem("戻る", () => {
            base.curves.Add(new Curve(this.deformingCurve.GetPoints(), true, selected: true, segment: base.segment));
            base.ResetMenu();
            this.newState = new SelectAutoOrManual(base.oculusTouch, base.contextMenu, base.curves);
        }));
    }

    public override void Update()
    {
        this.deformingCurve.Update();
        MenuItem renamedItem = this.contextMenu.FindItem((item) => this.contextMenu.items.IndexOf(item) == base.NumberOfDefaultItems);
        this.contextMenu.ChangeItemMessage(renamedItem, this.Message(this.deformingCurve.state));
    }

    private string Message(IKnotState knotState)
    {
        if (knotState.ToString() == "KnotStateBase")
        {
            return "可動域を確定";
        }
        else if (knotState.ToString() == "KnotStatePull")
        {
            return "変形";
        }
        else if (knotState.ToString() == "KnotStateChoose1")
        {
            return "可動域の始点を選択";
        }
        else if (knotState.ToString() == "KnotStateChoose2")
        {
            return "可動域の終点を選択";
        }

        return "";
    }
}