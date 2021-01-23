using UnityEngine.Events;


namespace ContextMenu {

  public class MenuItem {

    public string message;
    public UnityAction action;

    public MenuItem(string message, UnityAction action) {
      this.message = message;
      this.action = action;
    }

  }

}