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

    private float segment = 0.02f;
    private int meridian = 100;
    private float radius = 0.01f;
    private bool closed = false;

    private List<Vector3> positions = new List<Vector3>();
    private Vector3 predPosition = new Vector3();

    private Vector3 position = Vector3.zero;
    private Quaternion rotation = Quaternion.identity;

    private bool moving = false;
    private Vector3 stdPosition = new Vector3();
    private Quaternion stdRotation = new Quaternion();

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

                    mesh = CurveFunction.Curve(positions, meridian, radius, closed);
                }
            }
            else if (controller.GetButtonDown(OVRInput.RawButton.B))
            {
                closed = true;
                mesh = CurveFunction.Curve(positions, meridian, radius, closed);
            }
        }

        if (controller.GetButtonDown(OVRInput.RawButton.RIndexTrigger))
        {
            if (Dist(positions, controller.rightHand.GetPosition()) < 0.1f)
            {
                moving = true;
                stdPosition = controller.rightHand.GetPosition();
                stdRotation = controller.rightHand.GetRotation();
                positions = MapPlus(positions, -stdPosition);
                mesh = CurveFunction.Curve(positions, meridian, radius, closed);
            }
        }
        else if (controller.GetButtonUp(OVRInput.RawButton.RIndexTrigger))
        {
            moving = false;
            positions = MapPlus(MapRotation(positions, rotation), position);
            mesh = CurveFunction.Curve(positions, meridian, radius, closed);
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }

        if (controller.GetButton(OVRInput.RawButton.RIndexTrigger) && moving)
        {
            position = controller.rightHand.GetPosition();
            rotation = Quaternion.Inverse(stdRotation) * controller.rightHand.GetRotation();
        }

        Graphics.DrawMesh(mesh, position, rotation, material, 0);

    }

    private List<Vector3> MapPlus(List<Vector3> positions, Vector3 position)
    {
        List<Vector3> newPositions = new List<Vector3>();

        foreach (Vector3 v in positions)
        {
            newPositions.Add(v + position);
        }

        return newPositions;
    }

    private float Dist(List<Vector3> positions, Vector3 position)
    {
        List<Vector3> relPositions = MapPlus(positions, -position);

        float min = relPositions[0].magnitude;

        for (int i = 0; i < relPositions.Count - 1; i++)
        {
            if (relPositions[i + 1].magnitude < min)
            {
                min = relPositions[i + 1].magnitude;
            }
        }

        return min;
    }

    private List<Vector3> MapRotation(List<Vector3> positions, Quaternion rotation)
    {
        List<Vector3> newPositions = new List<Vector3>();

        foreach (Vector3 v in positions)
        {
            newPositions.Add(rotation * v);
        }

        return newPositions;
    }
}
