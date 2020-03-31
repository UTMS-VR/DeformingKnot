using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;

public class KnotMaster : MonoBehaviour
{
    public Controller controller;

    public int knotNumber = 0;
    public GameObject knotObjPrefab;
    public GameObject knotObj;

    // Start is called before the first frame update
    void Awake()
    {
        // this.SetCameraPosition();
        this.SetUpController();
    }

    // Update is called once per frame
    void Update()
    {
        this.controller.Update();

        // Aボタンを押すたびに曲線を生成する
        if (controller.GetButtonDown(OVRInput.RawButton.A))
        {
            GameObject knot = Instantiate(knotObjPrefab, knotObj.transform);
            knot.GetComponent<DrawKnot>().number = knotNumber;

            knotNumber++;
        }
    }

    /*void SetCameraPosition()
    {
        GameObject camera = GameObject.Find("OVRCameraRig");
        Vector3 cameraPosition = new Vector3(0, 0, -2);
        camera.transform.position = cameraPosition;
        camera.transform.forward = -cameraPosition;
    }*/

    void SetUpController()
    {
        this.controller = new Controller(
            buttonMap: ButtonMap.LiteralKeys,
            rightHandMover: Stick3DMap.OKLSemiIComma,
            // leftHandMover: Stick3DMap.WASDEC,
            handScale: 0.03f,
            handSpeed: 0.01f
        );
    }
}
