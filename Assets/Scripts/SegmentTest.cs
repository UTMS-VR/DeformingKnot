using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DrawCurve;
using DebugUtil;

public class SegmentTest : MonoBehaviour
{
    private Controller controller;
    private Curve curve1;
    private Curve curve2;
    private int count = 0;
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        MyController.SetUp(ref controller);
        text = GameObject.Find("Text").GetComponent<Text>();
        curve1 = new Curve(new List<Vector3>(), false);
        curve2 = new Curve(new List<Vector3>(), false);
    }

    // Update is called once per frame
    void Update()
    {
        MyController.Update(this.controller);

        if (controller.GetButtonDown(OVRInput.RawButton.A))
        {
            if (count == 0)
            {
                curve1.positions.Add(controller.rightHand.GetPosition());
                curve1.MeshUpdate();
                curve1.MeshAtPositionsUpdate();
            }
            else if (count == 1)
            {
                curve1.positions.Add(controller.rightHand.GetPosition());
                curve1.MeshUpdate();
                curve1.MeshAtPositionsUpdate();
            }
            else if (count == 2)
            {
                curve2.positions.Add(controller.rightHand.GetPosition());
                curve2.MeshUpdate();
                curve2.MeshAtPositionsUpdate();
            }
            else if (count == 3)
            {
                curve2.positions.Add(controller.rightHand.GetPosition());
                curve2.MeshUpdate();
                curve2.MeshAtPositionsUpdate();
            }
            
            count++;
        }

        if (count >= 1)
        {
            Graphics.DrawMesh(curve1.mesh, Vector3.zero, Quaternion.identity, MakeMesh.CurveMaterial, 0);
            Graphics.DrawMesh(curve1.meshAtPositions, Vector3.zero, Quaternion.identity, MakeMesh.PositionMaterial, 0);
        }

        if (count >= 3)
        {
            Graphics.DrawMesh(curve2.mesh, Vector3.zero, Quaternion.identity, MakeMesh.CurveMaterial, 0);
            Graphics.DrawMesh(curve2.meshAtPositions, Vector3.zero, Quaternion.identity, MakeMesh.PositionMaterial, 0);
        }

        if (count >= 4)
        {
            float dist = SegmentDist.SSDist(curve1.positions[0], curve1.positions[1], curve2.positions[0], curve2.positions[1]);
            float len1 = Vector3.Distance(curve1.positions[0], curve1.positions[1]);
            float len2 = Vector3.Distance(curve2.positions[0], curve2.positions[1]);
            text.text = "dist : " + (Mathf.Floor(dist * 1000) / 1000).ToString();
            text.text += ", len1 : " + (Mathf.Floor(len1 * 1000) / 1000).ToString();
            text.text += ", len2 : " + (Mathf.Floor(len2 * 1000) / 1000).ToString();
        }

        if (controller.GetButtonDown(OVRInput.RawButton.B))
        {
            curve1 = new Curve(new List<Vector3>(), false);
            curve2 = new Curve(new List<Vector3>(), false);
            count = 0;
            text.text = "";
        }
    }
}
