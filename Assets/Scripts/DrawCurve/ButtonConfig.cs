using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;

namespace DrawCurve
{
    public class ButtonConfig
    {
        public Controller controller;
        public OVRInput.RawButton draw;
        public OVRInput.RawButton move;
        public OVRInput.RawButton select;
        public OVRInput.RawButton cut;
        public OVRInput.RawButton combine;
        public OVRInput.RawButton remove;
        public OVRInput.RawButton undo;

        public ButtonConfig(
            Controller controller,
            OVRInput.RawButton draw = OVRInput.RawButton.RIndexTrigger,
            OVRInput.RawButton move = OVRInput.RawButton.RHandTrigger,
            OVRInput.RawButton select = OVRInput.RawButton.A,
            OVRInput.RawButton cut = OVRInput.RawButton.B,
            OVRInput.RawButton combine = OVRInput.RawButton.X,
            OVRInput.RawButton remove = OVRInput.RawButton.Y,
            OVRInput.RawButton undo = OVRInput.RawButton.LHandTrigger
        )
        {
            this.controller = controller;
            this.draw = draw;
            this.move = move;
            this.select = select;
            this.cut = cut;
            this.combine = combine;
            this.remove = remove;
            this.undo = undo;
        }

        public bool ValidButtonInput()
        {
            int valid = 0;

            if (controller.GetButton(this.draw) || controller.GetButtonUp(this.draw)) valid++;
            if (controller.GetButton(this.move) || controller.GetButtonUp(this.move)) valid++;
            if (controller.GetButtonDown(this.select)) valid++;
            if (controller.GetButtonDown(this.cut)) valid++;
            if (controller.GetButtonDown(this.combine)) valid++;
            if (controller.GetButtonDown(this.remove)) valid++;
            if (controller.GetButtonDown(this.undo)) valid++;

            return (valid == 1) ? true : false;
        }
    }
}