using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using InputManager;
using MinimizeSurface;

public class SurfaceTest : MonoBehaviour
{
    private Curve curve;
    private Surface surface;
    private OculusTouch oculusTouch;
    private string text;

    // Start is called before the first frame update
    void Start()
    {
        oculusTouch = new OculusTouch
        (
            buttonMap: LiteralKeysPlus,
            rightStickKey: PredefinedMaps.WASD,
            rightHandKey: PredefinedMaps.OKLSemiIComma,
            handScale: 0.03f,
            handSpeed: 0.01f
        );

        Curve.SetUp(oculusTouch, drawButton: LogicalOVRInput.RawButton.RIndexTrigger, moveButton: LogicalOVRInput.RawButton.RHandTrigger);
        curve = new Curve(new List<Vector3>(), false);

        //List<Vector3> vec = new List<Vector3> {new Vector3(0.0f, 0.0f, 1.0f), new Vector3(1.0f, 0.0f, 1.0f), new Vector3(0.5f, 0.86f, 1.0f)};
        //List<Vector3> vec = new List<Vector3> {new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.5f, 0.86f, 1.0f), new Vector3(1.0f, 0.0f, 1.0f)};
        //curve = new Curve(vec, true);
    }

    // Update is called once per frame
    void Update()
    {
        oculusTouch.UpdateFirst();

        curve.Draw();
        curve.Move();
        if (oculusTouch.GetButtonDown(LogicalOVRInput.RawButton.A)) curve.Close();
        Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, MakeMesh.CurveMaterial, 0);

        if (oculusTouch.GetButtonDown(LogicalOVRInput.RawButton.B))
        {
            curve = new Curve(new List<Vector3>(), false);
            surface = null;
        }

        if (oculusTouch.GetButtonDown(LogicalOVRInput.RawButton.X))
        {
            surface = new Surface(curve.positions, 5);
            surface.MeshUpdate();
        }

        if (oculusTouch.GetButtonDown(LogicalOVRInput.RawButton.LHandTrigger))
        {
            surface.GetMinimal();
            surface.MeshUpdate();
        }

        if (oculusTouch.GetButtonDown(LogicalOVRInput.RawButton.LIndexTrigger))
        {
            surface.LaplacianFairing();
            surface.MeshUpdate();
        }

        if (oculusTouch.GetButtonDown(LogicalOVRInput.RawButton.Y))
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

        oculusTouch.UpdateLast();
    }

    public void UpdateFixedInterface(FixedInterface.FixedInterfaceSetting setting)
    {
        setting.text = text;
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