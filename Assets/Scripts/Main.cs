using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DrawCurve;
using InputManager;

public class Main : MonoBehaviour
{
    private OculusTouch oculusTouch;
    private State state;
    private ButtonConfig button;
    private List<Curve> curves;
    private List<Curve> preCurves;
    private Curve drawingCurve;
    private List<int> movingCurves;
    private Knot deformingCurve;
    private string text;

    // Start is called before the first frame update
    void Start()
    {
        MyController.SetUp(ref oculusTouch);
        state = State.BasicDeform;
        button = new ButtonConfig(oculusTouch);
        Player.SetUp(oculusTouch, button);
        Curve.SetUp(oculusTouch, button.draw, button.move);

        curves = new List<Curve>();
        preCurves = new List<Curve>();
        drawingCurve = new Curve(new List<Vector3>(), false);
        movingCurves = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        this.oculusTouch.UpdateFirst();

        text = state.ToString();

        if (state == State.BasicDeform && button.ValidBaseButtonInput())
        {
            Player.DeepCopy(curves, ref preCurves);
            Player.ChangeState(ref curves, ref state, ref deformingCurve);
            Player.Draw(ref drawingCurve, ref curves);
            Player.Move(curves, ref movingCurves);
            Player.Select(curves);
            Player.Cut(ref curves);
            Player.Combine(ref curves);
            Player.Remove(ref curves);
            Player.Undo(ref curves, preCurves);
        }
        else if (state == State.ContiDeform)
        {
            Player.ChangeState(ref curves, ref state, ref deformingCurve);
            deformingCurve.Update();
        }

        Player.Display(curves);
        
        this.oculusTouch.UpdateLast();
    }

    public void UpdateFixedInterface(FixedInterface.FixedInterfaceSetting setting)
    {
        setting.text = text;
        if (state == State.ContiDeform)
        {
            deformingCurve.UpdateFixedInterface(setting);
        }
    }
}