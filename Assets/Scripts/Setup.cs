using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;

public class Setup : MonoBehaviour
{
    public Controller controller;

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
            rightHandMover: Stick3DMap.OKLSemiIComma//,
            // leftHandMover: Stick3DMap.WASDEC
        );
    }
}
