using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputManager
{
    public class LogicalButton
    {
        private LogicalButton() { }

        public static LogicalButton Create()
        {
            return new LogicalButton();
        }
    }

    public class LogicalStick
    {
        private LogicalStick() { }

        public static LogicalStick Create()
        {
            return new LogicalStick();
        }
    }

    public class LogicalPositionDevice
    {
        private LogicalPositionDevice() { }

        public static LogicalPositionDevice Create()
        {
            return new LogicalPositionDevice();
        }
    }
}
