using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DebugUtil;

public class Main : MonoBehaviour
{
    private Controller controller;
    private Player player;

    private List<Curve> curves = new List<Curve>();
    private int n_interval = 20;
    private string text1;
    // private Text text2;

    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material selectedMaterial;

    // Start is called before the first frame update
    void Start()
    {
        MyController.SetUp(ref controller);
        player = new Player(controller);

        // text2 = GameObject.Find("Text").GetComponent<Text>();
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
        else if (controller.GetButtonDown(OVRInput.RawButton.LIndexTrigger))
        {
            player.MakeBezierCurve(ref curves, n_interval);
        }
        else if (controller.GetButton(OVRInput.RawButton.LHandTrigger))
        {
            player.Optimize(curves);
        }

        // text2.text = "text2 Energy : ";
        text1 = "text1 Energy : ";

        foreach (Curve curve in curves)
        {
            Material material = curve.isSelected ? selectedMaterial : defaultMaterial;
            Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, material, 0);
            DiscreteMoebius discreteMoebius = new DiscreteMoebius(curve.positions, 1e-06f);
            // text2.text += discreteMoebius.Energy(curve.positions.ToArray()) + " ";
            text1 += discreteMoebius.Energy(curve.positions.ToArray()) + " ";
        }
    }

    public void UpdateFixedInterface(FixedInterface.FixedInterfaceSetting setting)
    {
        setting.text = text1;
    }
}