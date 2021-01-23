using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputManager;

public class Controller
{
    public OculusTouch oculusTouch;
    public LogicalButton changeState;
    public LogicalButton draw;
    public LogicalButton move;
    public LogicalButton select;
    public LogicalButton cut;
    public LogicalButton combine;
    public LogicalButton remove;
    public LogicalButton undo;
    public Controller()
    {
        this.oculusTouch = new OculusTouch
        (
            buttonMap: LiteralKeysPlus,
            rightStickKey: PredefinedMaps.WASD,
            rightHandKey: PredefinedMaps.OKLSemiIComma,
            handScale: 0.03f,
            handSpeed: 0.01f
        );
        this.changeState = LogicalOVRInput.RawButton.LIndexTrigger;
        this.draw = LogicalOVRInput.RawButton.RIndexTrigger;
        this.move = LogicalOVRInput.RawButton.RHandTrigger;
        this.select = LogicalOVRInput.RawButton.A;
        this.cut = LogicalOVRInput.RawButton.B;
        this.combine = LogicalOVRInput.RawButton.X;
        this.remove = LogicalOVRInput.RawButton.Y;
        this.undo = LogicalOVRInput.RawButton.LHandTrigger;
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

    public bool ValidBaseButtonInput()
    {
        int valid = 0;

        if (this.oculusTouch.GetButtonDown(this.changeState)) valid++;
        if (this.oculusTouch.GetButton(this.draw) || this.oculusTouch.GetButtonUp(this.draw)) valid++;
        if (this.oculusTouch.GetButton(this.move) || this.oculusTouch.GetButtonUp(this.move)) valid++;
        if (this.oculusTouch.GetButtonDown(this.select)) valid++;
        if (this.oculusTouch.GetButtonDown(this.cut)) valid++;
        if (this.oculusTouch.GetButtonDown(this.combine)) valid++;
        if (this.oculusTouch.GetButtonDown(this.remove)) valid++;
        if (this.oculusTouch.GetButtonDown(this.undo)) valid++;

        return (valid == 1) ? true : false;
    }
}
