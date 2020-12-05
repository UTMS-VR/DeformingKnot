using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using DebugUtil;
using MinimizeSurface;

public class SurfaceTest : MonoBehaviour
{
    private Curve curve;
    private Surface surface;
    private Controller controller;
    private string text;

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

        //List<Vector3> vec = new List<Vector3> {new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 1.0f), new Vector3(0.5f, 0.86f, 1.0f)};
        //List<Vector3> vec = new List<Vector3> {new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.5f, 0.86f, 1.0f), new Vector3(1.0f, 0.0f, 1.0f)};
        //curve = new Curve(vec, true);
    }

    // Update is called once per frame
    void Update()
    {
        controller.Update();
        curve.Draw();
        curve.Move();
        if (controller.GetButtonDown(OVRInput.RawButton.A)) curve.Close();
        Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, MakeMesh.CurveMaterial, 0);

        if (controller.GetButtonDown(OVRInput.RawButton.B))
        {
            curve = new Curve(new List<Vector3>(), false);
            surface = null;
        }

        if (controller.GetButtonDown(OVRInput.RawButton.X))
        {
            surface = new Surface(curve.positions, 5);
            surface.MeshUpdate();
        }

        if (controller.GetButtonDown(OVRInput.RawButton.LHandTrigger))
        {
            surface.GetMinimal();
            surface.MeshUpdate();
        }

        if (controller.GetButtonDown(OVRInput.RawButton.LIndexTrigger))
        {
            surface.LaplacianFairing();
            surface.MeshUpdate();
        }

        if (controller.GetButtonDown(OVRInput.RawButton.Y))
        {
            // Debug.Log(surface.Valid().ToString());
            // surface.DebugLog();
            surface.EdgeSwapping();
            surface.MeshUpdate();
        }

        if (surface != null)
        {
            Graphics.DrawMesh(surface.mesh1, Vector3.zero, Quaternion.identity, MakeMesh.CurveMaterial, 0);
            Graphics.DrawMesh(surface.mesh2, Vector3.zero, Quaternion.identity, MakeMesh.CurveMaterial, 0);
            text = surface.SurfaceArea().ToString();
        }
    }

    public void UpdateFixedInterface(FixedInterface.FixedInterfaceSetting setting)
    {
        setting.text = text;
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