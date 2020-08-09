using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;

public static class MyController
{
    public static void SetUp(ref Controller controller)
    {
        controller = new Controller(
            buttonMap: LiteralKeysPlus,
            rightHandMover: Stick3DMap.OKLSemiIComma,
            handScale: 0.03f,
            handSpeed: 0.01f
        );
    }

    public static void Update(Controller controller)
    {
        controller.Update();
    }

    private static ButtonMap LiteralKeysPlus
        = new ButtonMap(new List<(OVRInput.RawButton, KeyCode)>{
            ( OVRInput.RawButton.A, KeyCode.A ),
            ( OVRInput.RawButton.B, KeyCode.B ),
            ( OVRInput.RawButton.X, KeyCode.X ),
            ( OVRInput.RawButton.Y, KeyCode.Y ),
            ( OVRInput.RawButton.RIndexTrigger, KeyCode.R ),
            ( OVRInput.RawButton.RHandTrigger, KeyCode.E ),
            ( OVRInput.RawButton.LIndexTrigger, KeyCode.Q ),
            ( OVRInput.RawButton.LHandTrigger, KeyCode.W )
        });
}
