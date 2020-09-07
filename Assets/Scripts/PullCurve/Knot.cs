using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;
using DrawCurve;

public class Knot
{
    private IKnotState state;


    public Knot(
        List<Vector3> points,
        Controller controller,
        int meridian = 20,
        float radius = 0.1f,
        float distanceThreshold = -1,
        List<Curve> collisionCurves = null,
        OVRInput.RawButton selectButton = OVRInput.RawButton.A,
        OVRInput.RawButton cancelButton = OVRInput.RawButton.B,
        OVRInput.RawButton optimizeButton = OVRInput.RawButton.RIndexTrigger
        )
    {
        int count = points.Count;
        (int first, int second) chosenPoints = (count / 3, 2 * count / 3);
        KnotData data = new KnotData(points, chosenPoints, controller, radius, meridian, distanceThreshold, collisionCurves,
                        selectButton, cancelButton, optimizeButton);
        this.state = new KnotStateBase(data);
    }

    public void Update()
    {
        IKnotState newState = this.state.Update();
        if (newState != null)
        {
            Debug.Log($"Changed to {newState}");
            this.state = newState;
        }
    }

    public List<Vector3> GetPoints()
    {
        return this.state.GetPoints();
    }


    public void UpdateFixedInterface(FixedInterface.FixedInterfaceSetting setting)
    {
        setting.text += " " + this.state.ToString();
    }

}
