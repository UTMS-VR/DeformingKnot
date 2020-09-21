using UnityEngine;
using UnityEngine.UI;


namespace FixedInterface {

  public class FixedInterface : MonoBehaviour {

    private Canvas canvas;
    private Text text;
    private RectTransform textRectTransform;
    public FixedInterfaceEvent firstEvents;
    public FixedInterfaceEvent events;

    // ヒエラルキーに Canvas を 1 つ追加し、このスクリプトをアタッチしてください。
    // Canvas の各種設定はこのスクリプトが自動で行うので、ヒエラルキーに追加する以上の設定を行う必要はありません。
    // また、ヒエラルキーに OVRCameraRig が存在することを確認してください (存在しなかった場合は実行時にエラーログが出力されます)。
    public void Start() {
      Setup();
      var setting = CreateSetting(true);
      firstEvents.Invoke(setting);
      ApplySetting(setting);
    }

    public void Update() {
      var setting = CreateSetting(false);
      events.Invoke(setting);
      ApplySetting(setting);
    }

    private void Setup() {
      SetupCanvas();
      SetupText();
      SetupEvents();
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
      this.canvas = canvas;
    }

    private void SetupText() {
      var textObject = new GameObject("Fixed Text");
      var text = textObject.AddComponent<Text>();
      var textRectTransform = text.GetComponent<RectTransform>();
      textObject.transform.parent = this.transform;
      textRectTransform.localPosition = new Vector3(0, 0, 0);
      textRectTransform.localRotation = new Quaternion(0, 0, 0, 0);
      textRectTransform.localScale = new Vector3(1, 1, 1);
      textRectTransform.sizeDelta = this.GetTextSizeDelta();
      this.text = text;
      this.textRectTransform = textRectTransform;
    }

    private Vector2 GetTextSizeDelta() {
      // Fixed size on VR headset
      string productName = OVRPlugin.productName;
      if (productName == null || productName == "") {
        return this.canvas.GetComponent<RectTransform>().sizeDelta;
      } else {
        // return new Vector2(800, 500);
        return new Vector2(500, 300);
      }
    }

    private void SetupEvents() {
      if (this.events == null) {
        this.events = new FixedInterfaceEvent();
      }
    }

    private FixedInterfaceSetting CreateSetting(bool first) {
      var setting = new FixedInterfaceSetting();
      setting.canvasComponent = this.canvas;
      setting.textComponent = this.text;
      if (!first) {
        setting.text = this.text.text;
        setting.font = this.text.font;
        setting.fontSize = this.text.fontSize;
        setting.color = this.text.color;
        setting.alignment = this.text.alignment;
      }
      return setting;
    }

    private void ApplySetting(FixedInterfaceSetting setting) {
      this.text.text = setting.text;
      this.text.font = setting.font;
      this.text.fontSize = setting.fontSize;
      this.text.color = setting.color;
      this.text.alignment = setting.alignment;
    }

  }

}