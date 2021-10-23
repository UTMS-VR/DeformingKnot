using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputManager;
using DrawCurve;

#nullable enable

namespace PullCurve
{
    public class Knot
    {
        private IKnotState state;


        public Knot(
            ClosedCurve curve,
            OculusTouch oculusTouch,
            float distanceThreshold = -1,
            List<Curve>? collisionCurves = null,
            LogicalButton? buttonA = null,
            LogicalButton? buttonB = null,
            LogicalButton? buttonC = null,
            LogicalButton? buttonD = null,
            Material? curveMaterial = null,
            Material? pullableCurveMaterial = null,
            Material? pointMaterial = null
        )
        {
            buttonA = buttonA ?? LogicalOVRInput.RawButton.A;
            buttonB = buttonB ?? LogicalOVRInput.RawButton.B;
            buttonC = buttonC ?? LogicalOVRInput.RawButton.RIndexTrigger;
            buttonD = buttonD ?? LogicalOVRInput.RawButton.RHandTrigger;
            int count = curve.GetPoints().Count;
            (int first, int second) chosenPoints = (count / 3, 2 * count / 3);
            KnotData data = new KnotData(curve, chosenPoints, oculusTouch, distanceThreshold, collisionCurves,
                buttonA, buttonB, buttonC, buttonD, curveMaterial, pullableCurveMaterial, pointMaterial);
            this.state = new KnotStateBase(data);

            HandCurve.SetUp(oculusTouch, drawButton: buttonC, moveButton: buttonD);
        }

        public void Update()
        {
            IKnotState? newState = this.state.Update();
            if (newState != null)
            {
                Debug.Log($"Changed to {newState}");
                this.state = newState;
            }
        }

        public Curve GetCurve()
        {
            return this.state.GetCurve();
        }


        public void UpdateFixedInterface(FixedInterface.FixedInterfaceSetting setting)
        {
            setting.text = this.state.ToString();
        }

        public IKnotState GetKnotState()
        {
            return this.state;
        }
    }

}
