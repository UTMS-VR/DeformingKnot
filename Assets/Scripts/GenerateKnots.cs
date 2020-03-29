using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;

public class GenerateKnots : MonoBehaviour
{
    public int knotNumber = 0;

    public GameObject knotObjPrefab;
    public GameObject knotObj;

    public Controller controller;

    // Start is called before the first frame update
    void Start()
    {
        controller = GameObject.Find("Setup").GetComponent<Setup>().controller;
    }

    // Update is called once per frame
    void Update()
    {
        // Aボタンを押すたびに曲線を生成する
        if (controller.GetButtonDown(OVRInput.RawButton.A))
        {
            GameObject knot = Instantiate(knotObjPrefab, knotObj.transform);
            knot.GetComponent<DrawKnot>().number = knotNumber;

            knotNumber++;
        }
    }
}
