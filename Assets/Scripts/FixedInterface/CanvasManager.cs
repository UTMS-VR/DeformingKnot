using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FixedInterface {
    public class CanvasManager : MonoBehaviour {
        // Start is called before the first frame update
        void Start() {
            SetupCanvas();
        }

        // Update is called once per frame
        void Update() {
            
        }

        private void SetupCanvas() {
            var canvas = this.gameObject.GetComponent<Canvas>();
            var anchorObject = GameObject.Find("CenterEyeAnchor");
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.pixelPerfect = true;
            if (anchorObject != null) {
                canvas.worldCamera = anchorObject.GetComponent<Camera>();
            } else {
                Debug.LogError("Cannot find CenterEyeAnchor inside OVRCameraRig. Please add it in the hierarchy.");
            }
            this.gameObject.layer = 5;
        }
    }
}
