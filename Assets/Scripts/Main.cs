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
    private float collision = 0.05f;

    private Vector3 predPosition = new Vector3();
    private Vector3 stdPosition = new Vector3();
    private Quaternion stdRotation = new Quaternion();

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

        List<Curve> removeCurves = new List<Curve>();
        List<Curve> addCurves = new List<Curve>();

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

            // 右人差し指:曲線を選択
            if (controller.GetButtonDown(OVRInput.RawButton.RIndexTrigger) && Dist(curve.positions, nowPosition).Item2 < collision)
            {
                curve.isSelected = !curve.isSelected;
            }

            if (curve.isSelected)
            {
                // Bボタン:曲線を閉じる
                if (controller.GetButtonDown(OVRInput.RawButton.B))
                {
                    curve.isClosed = !curve.isClosed;
                    curve.MeshUpdate();
                }

                // 右中指:曲線を移動
                if (controller.GetButtonDown(OVRInput.RawButton.RHandTrigger) && Dist(curve.positions, nowPosition).Item2 < collision)
                {
                    curve.isBeingMoved = true;
                    stdPosition = nowPosition;
                    stdRotation = controller.rightHand.GetRotation();
                    curve.positions = MapPlus(curve.positions, -stdPosition);
                    curve.MeshUpdate();
                }
                else if (controller.GetButtonUp(OVRInput.RawButton.RHandTrigger))
                {
                    curve.isBeingMoved = false;
                    curve.positions = MapPlus(MapRotation(curve.positions, curve.rotation), curve.position);
                    curve.MeshUpdate();
                    curve.position = Vector3.zero;
                    curve.rotation = Quaternion.identity;
                }

                if (controller.GetButton(OVRInput.RawButton.RHandTrigger) && curve.isBeingMoved)
                {
                    curve.position = nowPosition;
                    curve.rotation = controller.rightHand.GetRotation() * Quaternion.Inverse(stdRotation);
                }

                // 左人差し指:曲線を切る
                if (controller.GetButtonDown(OVRInput.RawButton.LIndexTrigger) && Dist(curve.positions, nowPosition).Item2 < collision)
                {
                    int num = Dist(curve.positions, nowPosition).Item1;
                    if (2 <= num && num <= curve.positions.Count - 3)
                    {
                        if (curve.isClosed)
                        {
                            List<Vector3> newPositions = new List<Vector3>();

                            for (int i = num + 1; i < curve.positions.Count; i++)
                            {
                                newPositions.Add(curve.positions[i]);
                            }

                            for (int i = 0; i < num; i++)
                            {
                                newPositions.Add(curve.positions[i]);
                            }

                            Curve newCurve = new Curve(false, false, false, false, newPositions, curve.position, curve.rotation);
                            newCurve.MeshUpdate();

                            removeCurves.Add(curve);
                            addCurves.Add(newCurve);
                        }
                        else
                        {
                            List<Vector3> newPositions1 = new List<Vector3>();
                            List<Vector3> newPositions2 = new List<Vector3>();

                            for (int i = 0; i < num; i++)
                            {
                                newPositions1.Add(curve.positions[i]);
                            }

                            for (int i = num + 1; i < curve.positions.Count; i++)
                            {
                                newPositions2.Add(curve.positions[i]);
                            }

                            Curve newCurve1 = new Curve(false, false, false, false, newPositions1, curve.position, curve.rotation);
                            newCurve1.MeshUpdate();
                            Curve newCurve2 = new Curve(false, false, false, false, newPositions2, curve.position, curve.rotation);
                            newCurve2.MeshUpdate();

                            removeCurves.Add(curve);
                            addCurves.Add(newCurve1);
                            addCurves.Add(newCurve2);
                        }
                    }
                }

                // Yボタン:曲線を消去
                if (controller.GetButtonDown(OVRInput.RawButton.Y))
                {
                    removeCurves.Add(curve);
                }
            }
        }

        // 左中指:曲線の結合1
        if (controller.GetButtonDown(OVRInput.RawButton.LHandTrigger))
        {
            List<Curve> selectedCurves = SelectedCurves(curveList);
            if (selectedCurves.Count == 2 && !selectedCurves[0].isClosed && !selectedCurves[1].isClosed)
            {
                List<Vector3> positions0 = selectedCurves[0].positions;
                List<Vector3> positions1 = selectedCurves[1].positions;

                if (Vector3.Distance(positions0[positions0.Count - 1], positions1[positions1.Count - 1]) < collision)
                {
                    positions1.Reverse();
                }
                else if (Vector3.Distance(positions0[0], positions1[0]) < collision)
                {
                    positions0.Reverse();
                }
                else if (Vector3.Distance(positions0[0], positions1[positions1.Count - 1]) < collision)
                {
                    positions0.Reverse();
                    positions1.Reverse();
                }

                foreach (Vector3 v in positions1)
                {
                    positions0.Add(v);
                }

                Curve newCurve = new Curve(false, false, false, false, positions0, Vector3.zero, Quaternion.identity);
                newCurve.MeshUpdate();

                removeCurves.Add(selectedCurves[0]);
                removeCurves.Add(selectedCurves[1]);
                addCurves.Add(newCurve);
            }
        }

        foreach (Curve curve in removeCurves)
        {
            curveList.Remove(curve);
        }

        foreach (Curve curve in addCurves)
        {
            curveList.Add(curve);
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

    private Tuple<int, float> Dist(List<Vector3> positions, Vector3 position)
    {
        List<Vector3> relPositions = MapPlus(positions, -position);

        int num = 0;
        float min = relPositions[0].magnitude;

        for (int i = 0; i < relPositions.Count - 1; i++)
        {
            if (relPositions[i + 1].magnitude < min)
            {
                num = i + 1;
                min = relPositions[i + 1].magnitude;
            }
        }

        return new Tuple<int, float>(num, min);
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

    private List<Curve> SelectedCurves (List<Curve> curveList)
    {
        List<Curve> newCurveList = new List<Curve>();

        foreach (Curve curve in curveList)
        {
            if (curve.isSelected)
            {
                newCurveList.Add(curve);
            }
        }

        return newCurveList;
    }

    private Material MaterialChoice(bool isSelected)
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
