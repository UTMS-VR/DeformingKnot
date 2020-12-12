using UnityEngine;
using UnityEngine.UI;


namespace ContextMenu {

  public class SelectionManager : MonoBehaviour {

    private long count = 0;
    private long selectCount = 0;

    public void Start() {
    }

    public void Update() {
      this.ChangeColor();
      this.UpdateCounts();
    }

    public void OnDisable() {
      this.ResetCounts();
    }

    private void ChangeColor() {
      var hueRatio = this.selectCount / 15f;
      var alphaRatio = this.count / 90f;
      var hue = (30 * hueRatio + 60 * (1 - hueRatio)) / 360f;
      var alpha = (Mathf.Sin(alphaRatio * Mathf.PI * 2) + 1) / 2 * 0.3f + 0.7f;
      var color = Color.HSVToRGB(hue, 1, 1);
      color.a = alpha;
      this.gameObject.GetComponent<Image>().color = color;
    }

    private void UpdateCounts() {
      this.count ++;
      if (this.selectCount > 0) {
        this.selectCount --;
      }
    }

    private void ResetCounts() {
      this.count = 0;
      this.selectCount = 0;
    }

    public void Select() {
      this.selectCount = 15;
    }

  }

}