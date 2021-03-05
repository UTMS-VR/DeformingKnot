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

    public class PredefinedMaps
    {
        public static ButtonMap LiteralKeys = new ButtonMap(new ButtonMapData
        {
            (LogicalOVRInput.RawButton.A, new PhysicalKey(KeyCode.A)),
            (LogicalOVRInput.RawButton.B, new PhysicalKey(KeyCode.B)),
            (LogicalOVRInput.RawButton.X, new PhysicalKey(KeyCode.X)),
            (LogicalOVRInput.RawButton.Y, new PhysicalKey(KeyCode.Y)),
            (LogicalOVRInput.RawButton.RIndexTrigger, new PhysicalKey(KeyCode.R)),
            (LogicalOVRInput.RawButton.LIndexTrigger, new PhysicalKey(KeyCode.L)),
        });

        public static ButtonMap PositionalKeys = new ButtonMap(new ButtonMapData
        {
            (LogicalOVRInput.RawButton.A, new PhysicalKey(KeyCode.Period)),
            (LogicalOVRInput.RawButton.B, new PhysicalKey(KeyCode.Slash)),
            (LogicalOVRInput.RawButton.X, new PhysicalKey(KeyCode.X)),
            (LogicalOVRInput.RawButton.Y, new PhysicalKey(KeyCode.Z)),
            (LogicalOVRInput.RawButton.RIndexTrigger, new PhysicalKey(KeyCode.P)),
            (LogicalOVRInput.RawButton.LIndexTrigger, new PhysicalKey(KeyCode.Q)),
        });

        public static KeyTo2D Arrows = new KeyTo2D
        {
            up = KeyCode.UpArrow,
            down = KeyCode.DownArrow,
            right = KeyCode.RightArrow,
            left = KeyCode.LeftArrow
        };

        public static KeyTo2D WASD = new KeyTo2D
        {
            up = KeyCode.W,
            down = KeyCode.S,
            right = KeyCode.D,
            left = KeyCode.A
        };

        public static KeyTo2D OKLSemi = new KeyTo2D
        {
            up = KeyCode.O,
            down = KeyCode.L,
            right = KeyCode.Semicolon,
            left = KeyCode.K
        };


        public static KeyTo3D WASDEC = new KeyTo3D
        {
            up = KeyCode.W,
            down = KeyCode.S,
            right = KeyCode.D,
            left = KeyCode.A,
            above = KeyCode.E,
            below = KeyCode.C
        };

        public static KeyTo3D OKLSemiIComma = new KeyTo3D
        {
            up = KeyCode.O,
            down = KeyCode.L,
            right = KeyCode.Semicolon,
            left = KeyCode.K,
            above = KeyCode.I,
            below = KeyCode.Comma
        };
    }
}