using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputManager
{
    class LogicalOVRInput
    {
        public class RawButton {
            public static LogicalButton A = LogicalButton.Create();
            public static LogicalButton B = LogicalButton.Create();
            public static LogicalButton X = LogicalButton.Create();
            public static LogicalButton Y = LogicalButton.Create();
            public static LogicalButton RHandTrigger = LogicalButton.Create();
            public static LogicalButton RIndexTrigger = LogicalButton.Create();
            public static LogicalButton LHandTrigger = LogicalButton.Create();
            public static LogicalButton LIndexTrigger = LogicalButton.Create();
            public static LogicalButton RStickUp = LogicalButton.Create();
            public static LogicalButton RStickDown = LogicalButton.Create();
            public static LogicalButton RStickRight = LogicalButton.Create();
            public static LogicalButton RStickLeft = LogicalButton.Create();
            public static LogicalButton LStickUp = LogicalButton.Create();
            public static LogicalButton LStickDown = LogicalButton.Create();
            public static LogicalButton LStickRight = LogicalButton.Create();
            public static LogicalButton LStickLeft = LogicalButton.Create();
        }

        public class RawAxis2D
        {
            public static LogicalStick RStick = LogicalStick.Create();
            public static LogicalStick LStick = LogicalStick.Create();
        }

        public class RawHand
        {
            public static LogicalPositionDevice RHand = LogicalPositionDevice.Create();
            public static LogicalPositionDevice LHand = LogicalPositionDevice.Create();
        }
    }
}