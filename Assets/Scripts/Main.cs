using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;

public class Main : MonoBehaviour
{
    private Controller controller;

    private List<Curve> curveList = new List<Curve>();

    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material selectedMaterial;
    private float segment = 0.02f;
    private float collision = 0.1f;

    private Vector3 predPosition = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        SetUpController();
    }

    // Update is called once per frame
    void Update()
    {
        controller.Update();
        Vector3 nowPosition = controller.rightHand.GetPosition();

        // Aボタン:曲線を生成
        if (controller.GetButtonDown(OVRInput.RawButton.A))
        {
            curveList.Add(new Curve(false, true, false, false, new List<Vector3>(), Vector3.zero, Quaternion.identity));
        }

        foreach (Curve curve in curveList)
        {
            if (curve.isBeingDrawn)
            {
                // Aボタンを押している間:曲線を描画
                if (controller.GetButton(OVRInput.RawButton.A))
                {
                    if (curve.positions.Count == 0)
                    {
                        curve.positions.Add(nowPosition);
                        predPosition = nowPosition;
                    }
                    else if (Vector3.Distance(nowPosition, predPosition) >= segment)
                    {
                        curve.positions.Add(nowPosition);
                        predPosition = nowPosition;
                        curve.MeshUpdate();
                    }
                }
                else if (controller.GetButtonUp(OVRInput.RawButton.A))
                {
                    curve.isBeingDrawn = false;
                }
            }

            // 右手人差し指:曲線を選択
            if (controller.GetButtonDown(OVRInput.RawButton.RIndexTrigger) && Dist(curve.positions, nowPosition) < collision)
            {
                curve.isSelected = !curve.isSelected;
            }

            // Bボタン:曲線を閉じる
            if (curve.isSelected && controller.GetButtonDown(OVRInput.RawButton.B))
            {
                curve.isClosed = !curve.isClosed;
                curve.MeshUpdate();
            }

            // Yボタン:曲線を消去
            if (curve.isSelected && controller.GetButtonDown(OVRInput.RawButton.Y))
            {
                curveList.Remove(curve);
            }
        }

        // 曲線を描画
        foreach (Curve curve in curveList)
        {
            Graphics.DrawMesh(curve.mesh, curve.position, curve.rotation, MaterialChoice(curve.isSelected), 0);
        }
    }

    private void SetUpController()
    {
        this.controller = new Controller(
            buttonMap: LiteralKeysPlus,
            rightHandMover: Stick3DMap.OKLSemiIComma,
            handScale: 0.03f,
            handSpeed: 0.01f
        );
    }

    private ButtonMap LiteralKeysPlus = new ButtonMap(new List<(OVRInput.RawButton, KeyCode)>{
            ( OVRInput.RawButton.A, KeyCode.A ),
            ( OVRInput.RawButton.B, KeyCode.B ),
            ( OVRInput.RawButton.X, KeyCode.X ),
            ( OVRInput.RawButton.Y, KeyCode.Y ),
            ( OVRInput.RawButton.RIndexTrigger, KeyCode.R ),
            ( OVRInput.RawButton.RHandTrigger, KeyCode.E ),
            ( OVRInput.RawButton.LIndexTrigger, KeyCode.Q ),
            ( OVRInput.RawButton.LHandTrigger, KeyCode.W )
        });

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

    public Material MaterialChoice(bool isSelected)
    {
        if (isSelected)
        {
            return selectedMaterial;
        }
        else
        {
            return defaultMaterial;
        }
    }
}
