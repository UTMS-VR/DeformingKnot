using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputManager;
using DrawCurve;

public class Knot
{
    private IKnotState state;

    public Knot(
        List<Vector3> points,
        OculusTouch oculusTouch,
        int meridian = 20,
        float radius = 0.1f,
        float distanceThreshold = -1,
        List<Curve> collisionCurves = null
        )
    {
        int count = points.Count;
        (int first, int second) chosenPoints = (count / 3, 2 * count / 3);
        KnotData data = new KnotData(points, chosenPoints, oculusTouch, radius, meridian, distanceThreshold, collisionCurves,
                        LogicalOVRInput.RawButton.A, LogicalOVRInput.RawButton.B, LogicalOVRInput.RawButton.RIndexTrigger);
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
