using UnityEngine;
using UnityEngine.UI;


namespace ContextMenu {

  public class CanvasManager : MonoBehaviour {

    public void Start() {
      SetupCanvas();
    }

    public void Update() {
    }

    // キャンバスを描画するカメラを VR 用のカメラに設定します。
    // これ以外の設定 (描画モードなど) はプレハブの uGUI からすでに行っているので、このスクリプトでは行っていません。
    private void SetupCanvas() {
      var canvas = this.gameObject.GetComponent<Canvas>();
      var anchorObject = GameObject.Find("CenterEyeAnchor");
      if (anchorObject != null) {
        canvas.worldCamera = anchorObject.GetComponent<Camera>();
      } else {
        Debug.LogError("Cannot find CenterEyeAnchor inside OVRCameraRig. Please add it in the hierarchy.");
      }
    }

  }

}