using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DrawCurve;
using InputManager;
using FileManager;
using ContextMenu;

public class Main : MonoBehaviour
{
    private OculusTouch oculusTouch;
    private ContextMenu.ContextMenu contextMenu;
    private State state;

    void Start()
    {
        this.oculusTouch = new OculusTouch
        (
            buttonMap: LiteralKeysPlus,
            leftStickKey: PredefinedMaps.Arrows,
            rightHandKey: PredefinedMaps.OKLSemiIComma,
            handScale: 0.03f,
            handSpeed: 0.01f
        );

        this.contextMenu = new ContextMenu.ContextMenu(
            this.oculusTouch,
            upButton: LogicalOVRInput.RawButton.LStickUp,
            downButton: LogicalOVRInput.RawButton.LStickDown,
            confirmButton: LogicalOVRInput.RawButton.X,
            toggleMenuButton: LogicalOVRInput.RawButton.LIndexTrigger,
            lockLevel: null
        );
        this.contextMenu.AddItem(new MenuItem("左人差し指 : メニューウィンドウの開閉", () => {}));
        this.contextMenu.AddItem(new MenuItem("左スティック : カーソル操作", () => {}));
        this.contextMenu.AddItem(new MenuItem("X : 決定", () => {}));
        this.contextMenu.Open();

        this.state = new BasicDeformation(this.oculusTouch, this.contextMenu, new List<Curve>());
    }

    void Update()
    {
        this.oculusTouch.UpdateFirst();
        this.contextMenu.Update();
        this.state.Update();
        if (this.state.newState != null) this.state = this.state.newState;
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