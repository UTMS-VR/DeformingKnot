using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DrawCurve;
using DebugUtil;

public class Main : MonoBehaviour
{
    private Controller controller;
    private ButtonConfig button = new ButtonConfig();
    private List<Curve> curves = new List<Curve>();
    private Curve drawingCurve = new Curve(new List<Vector3>(), false);
    private List<int> movingCurves = new List<int>();
    private string text;

    // Start is called before the first frame update
    void Start()
    {
        MyController.SetUp(ref controller);
        Player.SetUp(controller, button);
        Curve.SetUp(controller, button);
    }

    // Update is called once per frame
    void Update()
    {
        MyController.Update(this.controller);

        Player.Draw(ref drawingCurve, ref curves);
        Player.Move(curves, movingCurves);
        Player.Select(curves);
        Player.Cut(ref curves);
        Player.Combine(ref curves);
        Player.Remove(ref curves);
        Player.Display(curves);
    }

    public void UpdateFixedInterface(FixedInterface.FixedInterfaceSetting setting)
    {
        setting.text = text;
    }
}