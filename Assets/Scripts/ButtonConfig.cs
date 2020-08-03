using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonConfig
{
    public OVRInput.RawButton draw;
    public OVRInput.RawButton move;
    public OVRInput.RawButton select;
    public OVRInput.RawButton cut;
    public OVRInput.RawButton combine;
    public OVRInput.RawButton remove;
    public OVRInput.RawButton undo;

    public ButtonConfig(
        OVRInput.RawButton draw = OVRInput.RawButton.RIndexTrigger,
        OVRInput.RawButton move = OVRInput.RawButton.RHandTrigger,
        OVRInput.RawButton select = OVRInput.RawButton.A,
        OVRInput.RawButton cut = OVRInput.RawButton.B,
        OVRInput.RawButton combine = OVRInput.RawButton.X,
        OVRInput.RawButton remove = OVRInput.RawButton.Y,
        OVRInput.RawButton undo = OVRInput.RawButton.RHandTrigger
    )
    {
        this.draw = draw;
        this.move = move;
        this.select = select;
        this.cut = cut;
        this.combine = combine;
        this.remove = remove;
        this.undo = undo;
    }
}
