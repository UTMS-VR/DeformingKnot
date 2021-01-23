using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace FixedInterface {

  [Serializable]
  public class FixedInterfaceSetting {

    public Canvas canvasComponent = null;
    public Text textComponent = null;
    
    public string text = "Nothing";
    public Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    public int fontSize = 30;
    public Color color = new Color(0, 0, 0);
    public TextAnchor alignment = TextAnchor.UpperLeft;

  }


  [Serializable]
  public class FixedInterfaceEvent : UnityEvent<FixedInterfaceSetting> {

  }

}