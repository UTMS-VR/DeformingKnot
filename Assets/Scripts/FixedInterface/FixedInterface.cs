using UnityEngine;
using UnityEngine.UI;


namespace FixedInterface {

  public class FixedInterface : MonoBehaviour {

    private Canvas canvas;
    private Text text;
    private RectTransform textRectTransform;
    public FixedInterfaceEvent events;

    // ヒエラルキーに Canvas を 1 つ追加し、このスクリプトをアタッチしてください。
    // Canvas の各種設定はこのスクリプトが自動で行うので、ヒエラルキーに追加する以上の設定を行う必要はありません。
    // また、ヒエラルキーに OVRCameraRig が存在することを確認してください (存在しなかった場合は実行時にエラーログが出力されます)。
    public void Start() {
      var setting = new FixedInterfaceSetting();
      SetupCanvas();
      SetupText();
      SetupEvents();
      ApplySetting(setting);
    }

    public void Update() {
      var setting = CreateSetting();
      events.Invoke(setting);
      ApplySetting(setting);
    }

    private void SetupCanvas() {
      var canvas = this.gameObject.GetComponent<Canvas>();
      var anchor = GameObject.Find("CenterEyeAnchor");
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvas.pixelPerfect = true;
      if (anchor != null) {
        canvas.worldCamera = anchor.GetComponent<Camera>();
      } else {
        Debug.LogError("Cannot find CenterEyeAnchor inside OVRCameraRig. Please add it in the hierarchy.");
      }
      this.gameObject.layer = 5;
      this.canvas = canvas;
    }

    private void SetupText() {
      var textObject = new GameObject("Fixed Text");
      var text = textObject.AddComponent<Text>();
      var textRectTransform = text.GetComponent<RectTransform>();
      var canvasRectTransform = this.canvas.GetComponent<RectTransform>();
      textObject.transform.parent = this.transform;
      textRectTransform.localPosition = new Vector3(0, 0, 0);
      textRectTransform.localRotation = new Quaternion(0, 0, 0, 0);
      textRectTransform.localScale = new Vector3(1, 1, 1);
      textRectTransform.sizeDelta = canvasRectTransform.sizeDelta;
      this.text = text;
      this.textRectTransform = textRectTransform;
    }

    private void SetupEvents() {
      if (this.events == null) {
        this.events = new FixedInterfaceEvent();
      }
    }

    private FixedInterfaceSetting CreateSetting() {
      var setting = new FixedInterfaceSetting();
      setting.text = text.text;
      setting.font = text.font;
      setting.fontSize = text.fontSize;
      setting.color = text.color;
      setting.alignment = text.alignment;
      return setting;
    }

    private void ApplySetting(FixedInterfaceSetting setting) {
      text.text = setting.text;
      text.font = setting.font;
      text.fontSize = setting.fontSize;
      text.color = setting.color;
      text.alignment = setting.alignment;
    }

  }

}