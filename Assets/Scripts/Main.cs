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
        else if (controller.GetButton(OVRInput.RawButton.LHandTrigger))
        {
            player.Optimize(curves);
        }

        foreach (Curve curve in curves)
        {
            Material material = curve.isSelected ? selectedMaterial : defaultMaterial;
            Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, material, 0);
        }
    }
}