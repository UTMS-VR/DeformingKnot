using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugUtil
{


    public class Controller
    {
        private ButtonMap buttonMap;
        public VRController rightHand;
        public VRController leftHand;

        public Controller(
            ButtonMap buttonMap = null, 
            Stick2DMap rightStickMap = null,
            Stick3DMap rightHandMover = null,
            Stick2DMap leftStickMap = null,
            Stick3DMap leftHandMover = null
        )
        {
            this.buttonMap = buttonMap;
            this.rightHand = new VRController(
                handAnchorName: "RightHandAnchor",
                stick: OVRInput.RawAxis2D.RThumbstick,
                stickMap: rightStickMap,
                positionMover: rightHandMover,
                this.IsOnHeadset()
            );
            this.leftHand = new VRController(
                handAnchorName: "LeftHandAnchor",
                stick: OVRInput.RawAxis2D.LThumbstick,
                stickMap: leftStickMap,
                positionMover: leftHandMover,
                this.IsOnHeadset()
            );
        }

        private bool IsOnHeadset()
        {
            string productName = OVRPlugin.productName;
            return !(productName == null || productName == "");
        }

        public void Update()
        {
            this.rightHand.Update();
            this.leftHand.Update();
        }

        public bool GetButton(OVRInput.RawButton button)
        {
            if (this.IsOnHeadset())
            {
                return OVRInput.Get(button);
            }
            else
            {
                if (this.buttonMap == null)
                {
                    return false;
                }
                return this.buttonMap.Get(button);
            }
        }
        public bool GetButtonDown(OVRInput.RawButton button)
        {
            if (this.IsOnHeadset())
            {
                return OVRInput.GetDown(button);
            }
            else
            {
                if (this.buttonMap == null)
                {
                    return false;
                }
                return this.buttonMap.GetDown(button);
            }
        }
        public bool GetButtonUp(OVRInput.RawButton button)
        {
            if (this.IsOnHeadset())
            {
                return OVRInput.GetUp(button);
            }
            else
            {
                if (this.buttonMap == null)
                {
                    return false;
                }
                return this.buttonMap.GetUp(button);
            }
        }
    }
 }
