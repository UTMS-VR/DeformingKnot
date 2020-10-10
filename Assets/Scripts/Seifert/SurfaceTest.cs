using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using DebugUtil;

public class SurfaceTest : MonoBehaviour
{
    private Curve curve;
    private Surface surface;
    private Controller controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = new Controller(
            buttonMap: LiteralKeysPlus,
            rightHandMover: Stick3DMap.OKLSemiIComma,
            handScale: 0.03f,
            handSpeed: 0.01f
        );

        Curve.SetUp(controller, drawButton: OVRInput.RawButton.RIndexTrigger, moveButton: OVRInput.RawButton.RHandTrigger);
        curve = new Curve(new List<Vector3>(), false);
    }

    // Update is called once per frame
    void Update()
    {
        controller.Update();
        curve.Draw();
        curve.Move();
        if(controller.GetButtonDown(OVRInput.RawButton.X)) curve.Close();
        Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, MakeMesh.CurveMaterial, 0);

        if (controller.GetButtonDown(OVRInput.RawButton.A))
        {
            surface = new Surface(curve.positions, 20);
            surface.MeshUpdate();
        }

        Graphics.DrawMesh(surface.mesh, Vector3.zero, Quaternion.identity, MakeMesh.CurveMaterial, 0);
    }

    private ButtonMap LiteralKeysPlus
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