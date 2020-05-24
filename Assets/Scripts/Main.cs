using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;

public class Main : MonoBehaviour
{
    private Controller controller;
    private Player player;

    private List<Curve> curves = new List<Curve>();
    private int n_interval = 10;
    private int k;
    private List<Vector3> Positions;
    private List<Vector3> NewPositions;
    private BezierCurve PartialCurve;

    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material selectedMaterial;

    // Start is called before the first frame update
    void Start()
    {
        MyController.SetUp(ref controller);
        player = new Player(controller);
    }

    // Update is called once per frame
    void Update()
    {
        MyController.Update(controller);

        player.Draw(curves);
        curves = curves.Where(curve => curve.isBeingDrawn || curve.positions.Count >= 2).ToList();

        player.Move(curves);

        if (controller.GetButtonDown(OVRInput.RawButton.A))
        {
            player.Select(curves);
        }
        else if (controller.GetButtonDown(OVRInput.RawButton.B))
        {
            player.Cut(ref curves);
        }
        else if (controller.GetButtonDown(OVRInput.RawButton.X))
        {
            List<Curve> selectedCurves = curves.Where(curve => curve.isSelected).ToList();

            if (selectedCurves.Count == 1)
            {
                player.Close(curves);
            }
            else if (selectedCurves.Count == 2 && !selectedCurves[0].isClosed && !selectedCurves[1].isClosed)
            {
                player.Combine(ref curves);
            }
        }
        else if (controller.GetButtonDown(OVRInput.RawButton.Y))
        {
            player.Remove(ref curves);
        }

        if (controller.GetButtonDown(OVRInput.RawButton.LIndexTrigger))
        {
            foreach (Curve curve in curves)
            {
                NewPositions = new List<Vector3>();
                Positions = curve.GetPositions();
                int N = Positions.Count;

                for (int i = 0; i < N; i += n_interval)
                {
                    if (i + n_interval < N)
                    {
                        k = n_interval;
                    }
                    else
                    {
                        k = N - i - 1;
                    }

                    PartialCurve = new BezierCurve(Positions.GetRange(i, k));
                    for (int j = 0; j < n_interval; j++)
                    {
                        NewPositions.Add(PartialCurve.GetPosition((float)j / k));
                    }
                }

                curve.UpdatePositions(NewPositions);
                curve.MeshUpdate();
                Debug.Log("Updated Knot");
            }
        }

        foreach (Curve curve in curves)
        {
            Material material = curve.isSelected ? selectedMaterial : defaultMaterial;
            Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, material, 0);
        }
    }
}