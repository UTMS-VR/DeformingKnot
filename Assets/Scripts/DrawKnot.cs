using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DebugUtil;

// 曲線描画用のコンポーネント
public class DrawKnot : MonoBehaviour
{
    public int number;
    private GameObject knotMaster;
    private KnotMaster masterComponent;
    private Controller controller;

    [SerializeField] private Material material;
    private Mesh mesh;

    private float segment = 0.05f;
    private int meridian = 100;
    private float radius = 0.01f;

    private List<Vector3> positions = new List<Vector3>();
    private Vector3 predPosition = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        knotMaster = GameObject.Find("KnotMaster");
        masterComponent = knotMaster.GetComponent<KnotMaster>();
        controller = masterComponent.controller;
    }

    // Update is called once per frame
    void Update()
    {
        if (masterComponent.knotNumber == number + 1)
        {
            if (controller.GetButton(OVRInput.RawButton.A))
            {
                Vector3 nowPosition = controller.rightHand.GetPosition();

                if (positions.Count == 0)
                {
                    positions.Add(nowPosition);
                    predPosition = nowPosition;
                }
                else if (Vector3.Distance(nowPosition, predPosition) >= segment)
                {
                    positions.Add(nowPosition);
                    predPosition = nowPosition;

                    mesh = CurveFunction.Curve(positions, meridian, radius, false);
                }
            }
            else if (controller.GetButtonDown(OVRInput.RawButton.B))
            {
                mesh = CurveFunction.Curve(positions, meridian, radius, true);
            }
        }

        Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);

    }
}
