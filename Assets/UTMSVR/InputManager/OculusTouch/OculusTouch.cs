using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputManager
{
    using ButtonMapData = List<(LogicalButton logicalButton, IPhysicalButton physicalButton)>;
    using StickMapData = List<(LogicalStick logicalStick, IPhysicalStick physicalStick)>;
    using PositionDeviceMapData = List<(
        LogicalPositionDevice logicalPositionDevice,
        IPhysicalPositionDevice physicalPositionDevice)>;

    public class OculusTouch : Controller
    {
        private static ButtonMap defaultButtonMap = new ButtonMap(new ButtonMapData
        {
            (LogicalOVRInput.RawButton.A, new PhysicalOculusTouchButton(OVRInput.RawButton.A)),
            (LogicalOVRInput.RawButton.B, new PhysicalOculusTouchButton(OVRInput.RawButton.B)),
            (LogicalOVRInput.RawButton.X, new PhysicalOculusTouchButton(OVRInput.RawButton.X)),
            (LogicalOVRInput.RawButton.Y, new PhysicalOculusTouchButton(OVRInput.RawButton.Y)),
            (LogicalOVRInput.RawButton.RHandTrigger, new PhysicalOculusTouchButton(OVRInput.RawButton.RHandTrigger)),
            (LogicalOVRInput.RawButton.LHandTrigger, new PhysicalOculusTouchButton(OVRInput.RawButton.LHandTrigger)),
            (LogicalOVRInput.RawButton.RIndexTrigger, new PhysicalOculusTouchButton(OVRInput.RawButton.RIndexTrigger)),
            (LogicalOVRInput.RawButton.LIndexTrigger, new PhysicalOculusTouchButton(OVRInput.RawButton.LIndexTrigger))
        });

        private static StickMap defaultStickMap = new StickMap(new StickMapData
        {
            //(LogicalOVRInput.RawAxis2D.RStick, new PhysicalOculusTouchStick(OVRInput.RawAxis2D.RThumbstick)),
            //(LogicalOVRInput.RawAxis2D.LStick, new PhysicalOculusTouchStick(OVRInput.RawAxis2D.LThumbstick))
        });
        private static PositionDeviceMap defaultPositionDeviceMap = new PositionDeviceMap(new PositionDeviceMapData {
            (LogicalOVRInput.RawHand.RHand, new PhysicalOculusTouchPositionDevice(HandAnchorKey.RightHand)),
            (LogicalOVRInput.RawHand.LHand, new PhysicalOculusTouchPositionDevice(HandAnchorKey.LeftHand))
        });

        public OculusTouch(
            ButtonMap buttonMap = null,
            KeyTo2D rightStickKey = null,
            KeyTo2D leftStickKey = null,
            KeyTo3D rightHandKey = null,
            KeyTo3D leftHandKey = null,
            float handScale = 0.1f,
            float handSpeed = 0.3f,
            uint? repeatDelay = null,
            uint? repeatInterval = null
            ) : base(OculusTouch.defaultButtonMap, OculusTouch.defaultStickMap, OculusTouch.defaultPositionDeviceMap,
                repeatDelay: repeatDelay, repeatInterval: repeatInterval)
        {
            var buttonMapData = new ButtonMapData { };
            var stickMapData = new StickMapData { };
            var positionDeviceMapData = new PositionDeviceMapData { };
            // setup buttons
            if (buttonMap != null)
            {
                this.MergeButtonMap(buttonMap);
            }
            // setup sticks
            IPhysicalStick rStick = new PhysicalOculusTouchStick(OVRInput.RawAxis2D.RThumbstick);
            IPhysicalStick lStick = new PhysicalOculusTouchStick(OVRInput.RawAxis2D.LThumbstick);
            stickMapData.Add((LogicalOVRInput.RawAxis2D.RStick, rStick));
            stickMapData.Add((LogicalOVRInput.RawAxis2D.LStick, lStick));
            buttonMapData.Add((LogicalOVRInput.RawButton.RStickUp,
                new PhysicalButtonFromStick(rStick, PhysicalButtonFromStick.Direction.Up)));
            buttonMapData.Add((LogicalOVRInput.RawButton.RStickDown,
                new PhysicalButtonFromStick(rStick, PhysicalButtonFromStick.Direction.Down)));
            buttonMapData.Add((LogicalOVRInput.RawButton.RStickRight,
                new PhysicalButtonFromStick(rStick, PhysicalButtonFromStick.Direction.Right)));
            buttonMapData.Add((LogicalOVRInput.RawButton.RStickLeft,
                new PhysicalButtonFromStick(rStick, PhysicalButtonFromStick.Direction.Left)));
            buttonMapData.Add((LogicalOVRInput.RawButton.LStickUp,
                new PhysicalButtonFromStick(lStick, PhysicalButtonFromStick.Direction.Up)));
            buttonMapData.Add((LogicalOVRInput.RawButton.LStickDown,
                new PhysicalButtonFromStick(lStick, PhysicalButtonFromStick.Direction.Down)));
            buttonMapData.Add((LogicalOVRInput.RawButton.LStickRight,
                new PhysicalButtonFromStick(lStick, PhysicalButtonFromStick.Direction.Right)));
            buttonMapData.Add((LogicalOVRInput.RawButton.LStickLeft,
                new PhysicalButtonFromStick(lStick, PhysicalButtonFromStick.Direction.Left)));
#if UNITY_EDITOR || UNITY_WEBGL
            // setup sticks
            if (rightStickKey != null)
            {
                IPhysicalStick rStickFromKey = new PhysicalKeyStick(rightStickKey);
                stickMapData.Add((LogicalOVRInput.RawAxis2D.RStick, rStickFromKey));
                buttonMapData.Add((LogicalOVRInput.RawButton.RStickUp,
                    new PhysicalButtonFromStick(rStickFromKey, PhysicalButtonFromStick.Direction.Up)));
                buttonMapData.Add((LogicalOVRInput.RawButton.RStickDown,
                    new PhysicalButtonFromStick(rStickFromKey, PhysicalButtonFromStick.Direction.Down)));
                buttonMapData.Add((LogicalOVRInput.RawButton.RStickRight,
                    new PhysicalButtonFromStick(rStickFromKey, PhysicalButtonFromStick.Direction.Right)));
                buttonMapData.Add((LogicalOVRInput.RawButton.RStickLeft,
                    new PhysicalButtonFromStick(rStickFromKey, PhysicalButtonFromStick.Direction.Left)));
            }
            if (leftStickKey != null)
            {
                IPhysicalStick lStickFromKey = new PhysicalKeyStick(leftStickKey);
                stickMapData.Add((LogicalOVRInput.RawAxis2D.LStick, lStickFromKey));
                buttonMapData.Add((LogicalOVRInput.RawButton.LStickUp,
                    new PhysicalButtonFromStick(lStickFromKey, PhysicalButtonFromStick.Direction.Up)));
                buttonMapData.Add((LogicalOVRInput.RawButton.LStickDown,
                    new PhysicalButtonFromStick(lStickFromKey, PhysicalButtonFromStick.Direction.Down)));
                buttonMapData.Add((LogicalOVRInput.RawButton.LStickRight,
                    new PhysicalButtonFromStick(lStickFromKey, PhysicalButtonFromStick.Direction.Right)));
                buttonMapData.Add((LogicalOVRInput.RawButton.LStickLeft,
                    new PhysicalButtonFromStick(lStickFromKey, PhysicalButtonFromStick.Direction.Left)));
            }
            // setup position devices
            if (rightHandKey != null)
            {
                positionDeviceMapData.Add((LogicalOVRInput.RawHand.RHand, new PhysicalKeyPositionDevice(rightHandKey, handSpeed, handScale)));
            }
            if (leftHandKey != null)
            {
                positionDeviceMapData.Add((LogicalOVRInput.RawHand.LHand, new PhysicalKeyPositionDevice(leftHandKey, handSpeed, handScale)));
            }
#endif
            this.MergeButtonMap(new ButtonMap(buttonMapData));
            this.MergeStickMap(new StickMap(stickMapData));
            this.MergePositionDeviceMap(new PositionDeviceMap(positionDeviceMapData));
        }

        public Vector2 GetStickR(Lock lc = null)
        {
            return this.GetStick(LogicalOVRInput.RawAxis2D.RStick, lc);
        }

        public Vector2 GetStickL(Lock lc = null)
        {
            return this.GetStick(LogicalOVRInput.RawAxis2D.LStick, lc);
        }

        public Vector3 GetPositionR()
        {
            return this.GetPosition(LogicalOVRInput.RawHand.RHand);
        }

        public Vector3 GetPositionL()
        {
            return this.GetPosition(LogicalOVRInput.RawHand.LHand);
        }

        public Quaternion GetRotationR()
        {
            return this.GetRotation(LogicalOVRInput.RawHand.RHand);
        }

        public Quaternion GetRotationL()
        {
            return this.GetRotation(LogicalOVRInput.RawHand.LHand);
        }
    }
}
