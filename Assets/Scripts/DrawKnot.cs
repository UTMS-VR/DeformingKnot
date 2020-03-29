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
    private Controller controller;

    private CurveFunction curveFunction = new CurveFunction();

    [SerializeField] private Material material;
    private Mesh mesh;

    private float segment = 0.01f;
    private int meridian = 100;
    private float radius = 0.01f;

    private List<Vector3> positions = new List<Vector3>();
    private Vector3 pPosition = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        knotMaster = GameObject.Find("KnotMaster");
        controller = GameObject.Find("Setup").GetComponent<Setup>().controller;
    }

    // Update is called once per frame
    void Update()
    {
        if (knotMaster.GetComponent<GenerateKnots>().knotNumber == number + 1)
        {
            if (controller.GetButton(OVRInput.RawButton.A))
            {
                Vector3 nowPosition = controller.rightHand.GetPosition();

                if (positions.Count == 0)
                {
                    positions.Add(nowPosition);
                    pPosition = nowPosition;
                }
                else if (Vector3.Distance(nowPosition, pPosition) >= segment)
                {
                    positions.Add(nowPosition);
                    pPosition = nowPosition;

                    mesh = curveFunction.Curve(positions, meridian, radius, false);
                }
            }
            else if (controller.GetButtonDown(OVRInput.RawButton.B))
            {
                mesh = curveFunction.Curve(positions, meridian, radius, true);
            }
        }

        Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);

    }
}
