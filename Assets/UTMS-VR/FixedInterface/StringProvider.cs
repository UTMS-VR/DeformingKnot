using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace FixedInterface {

  [Serializable]
  public class FixedInterfaceSetting {
    
    public string text = "Nothing";
    public Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    public int fontSize = 30;
    public Color color = new Color(0, 0, 0);

  }


  [Serializable]
  public class FixedInterfaceEvent : UnityEvent<FixedInterfaceSetting> {

  }

}