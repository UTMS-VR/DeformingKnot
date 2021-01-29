using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DrawCurve;
using InputManager;
using FileManager;

public class Main : MonoBehaviour
{
    private OculusTouch oculusTouch;
    private State state;

    void Start()
    {
        this.oculusTouch = new OculusTouch
        (
            buttonMap: LiteralKeysPlus,
            rightStickKey: PredefinedMaps.WASD,
            rightHandKey: PredefinedMaps.OKLSemiIComma,
            handScale: 0.03f,
            handSpeed: 0.01f
        );
        this.state = new BasicDeformation(oculusTouch, new List<Curve>());
    }

    void Update()
    {
        this.oculusTouch.UpdateFirst();
        State newState = this.state.Update();
        if (newState != null) this.state = newState;
        this.state.Display();
        this.oculusTouch.UpdateFirst();
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