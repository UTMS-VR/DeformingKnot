using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputManager;

public static class MyController
{
    public static void SetUp(ref OculusTouch oculusTouch)
    {
        oculusTouch = new OculusTouch
        (
            buttonMap: LiteralKeysPlus,
            rightStickKey: PredefinedMaps.WASD,
            rightHandKey: PredefinedMaps.OKLSemiIComma,
            handScale: 0.03f,
            handSpeed: 0.01f
        );
    }

    private static ButtonMap LiteralKeysPlus
    = new ButtonMap(new List<(LogicalButton logicalButton, IPhysicalButton physicalButton)>
    {
        ( LogicalOVRInput.RawButton.A, new PhysicalKey(KeyCode.A) ),
        ( LogicalOVRInput.RawButton.B, new PhysicalKey(KeyCode.B) ),
        ( LogicalOVRInput.RawButton.X, new PhysicalKey(KeyCode.X) ),
        ( LogicalOVRInput.RawButton.Y, new PhysicalKey(KeyCode.Y) ),
        ( LogicalOVRInput.RawButton.RIndexTrigger, new PhysicalKey(KeyCode.R) ),
        ( LogicalOVRInput.RawButton.RHandTrigger, new PhysicalKey(KeyCode.E) ),
        ( LogicalOVRInput.RawButton.LIndexTrigger, new PhysicalKey(KeyCode.Q) ),
        ( LogicalOVRInput.RawButton.LHandTrigger, new PhysicalKey(KeyCode.W) )
    });
}
