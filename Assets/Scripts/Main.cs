using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DrawCurve;
using DebugUtil;

public class Main : MonoBehaviour
{
    private Controller controller;
    private ButtonConfig button;
    private List<Curve> curves;
    private List<Curve> preCurves;
    private Curve drawingCurve;
    private List<int> movingCurves;
    private string text;
    private Text text1;

    // Start is called before the first frame update
    void Start()
    {
        MyController.SetUp(ref controller);
        button = new ButtonConfig(controller);
        Player.SetUp(controller, button);
        Curve.SetUp(controller, button.draw, button.move);

        curves = new List<Curve>();
        preCurves = new List<Curve>();
        drawingCurve = new Curve(new List<Vector3>(), false);
        movingCurves = new List<int>();

        text1 = GameObject.Find("Text").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        //text = "SegmentDist :";
        text1.text = "SegmentDist :";
        MyController.Update(this.controller);

        if (button.ValidButtonInput())
        {
            Player.DeepCopy(curves, ref preCurves);
            Player.Draw(ref drawingCurve, ref curves);
            Player.Move(curves, ref movingCurves);
            Player.Select(curves);
            Player.Cut(ref curves);
            Player.Combine(ref curves);
            Player.Remove(ref curves);
            Player.Optimize(curves);
            Player.Undo(ref curves, preCurves);
        }

        List<Curve> selection = curves.Where(curve => curve.selected).ToList();
        foreach (Curve curve in selection)
        {
            //text += " " + curve.MinSegmentDist();
            text1.text += " " + Mathf.Floor(curve.MinSegmentDist() * 100000) / 100000;
        }

        Player.Display(curves);
    }

    public void UpdateFixedInterface(FixedInterface.FixedInterfaceSetting setting)
    {
        setting.text = text;
    }
}