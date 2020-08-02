using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DebugUtil;

public class Main : MonoBehaviour
{
    private Controller controller;
    private List<Curve> curves = new List<Curve>();
    private Curve drawingCurve = new Curve(new List<Vector3>(), false);
    private List<int> movingCurves = new List<int>();
    private string text;

    // Start is called before the first frame update
    void Start()
    {
        MyController.SetUp(ref controller);
        Player.controller = this.controller;
        Curve.controller = this.controller;
    }

    // Update is called once per frame
    void Update()
    {
        MyController.Update(this.controller);

        Player.Draw(ref drawingCurve, ref curves, OVRInput.RawButton.RIndexTrigger);
        Player.Move(curves, movingCurves, OVRInput.RawButton.RHandTrigger);
        Player.Select(curves, OVRInput.RawButton.A);
        Player.Cut(ref curves, OVRInput.RawButton.B);
        Player.Combine(ref curves, OVRInput.RawButton.X);
        Player.Remove(ref curves, OVRInput.RawButton.Y);
        Player.Display(curves);
    }

    public void UpdateFixedInterface(FixedInterface.FixedInterfaceSetting setting)
    {
        setting.text = text;
    }
}