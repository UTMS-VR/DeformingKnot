using InputManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace ContextMenu {

  public class ContextMenu {

    private readonly OculusTouch controller;
    private GameObject panelPrefab;
    private GameObject selectionPrefab;
    private GameObject textPrefab;
    private GameObject canvasObject;
    private GameObject panelObject;
    private GameObject selectionObject;

    private readonly LogicalButton upButton;
    private readonly LogicalButton downButton;
    private readonly LogicalButton confirmButton;
    private readonly LogicalButton toggleMenuButton;

    private readonly uint? lockLevel;
    private Lock locq = null;

    private bool innerDisplayed = false;  // 直接使用は非推奨
    public bool displayed {
      get {
        return this.innerDisplayed;
      }
      set {
        this.innerDisplayed = value;
        if (value) {
          if (this.lockLevel is uint level) {
            this.locq = this.controller.GetLock(level);
          }
        } else {
          if (this.lockLevel is uint level) {
            this.controller.Unlock(this.locq);
            this.locq = null;
          }
        }
        UpdatePanelObject();
      }
    }

    private int innerCursorIndex = 0;  // 直接使用は非推奨
    public int cursorIndex {
      get {
        return this.innerCursorIndex;
      }
      set {
        var count = this.innerItems.Count;
        this.innerCursorIndex = (value % count + count) % count;
        UpdateSelectionObject();
      }
    }

    private readonly List<MenuItem> innerItems = new List<MenuItem>();
    public ReadOnlyCollection<MenuItem> items {
      get {
        return this.innerItems.AsReadOnly();
      }
    }

    public ContextMenu(
      OculusTouch controller,
      LogicalButton upButton,
      LogicalButton downButton,
      LogicalButton confirmButton,
      LogicalButton toggleMenuButton,
      uint? lockLevel = 10
    ) {
      this.controller = controller;
      this.upButton = upButton;
      this.downButton = downButton;
      this.confirmButton = confirmButton;
      this.toggleMenuButton = toggleMenuButton;
      this.lockLevel = lockLevel;
      FindCanvasObject();
      LoadPrefabs();
      CreatePanel();
    }

    public void Update() {
      ToggleDisplayed();
      ChangeSelectedIndex();
      ExecuteAction();
    }

    private void FindCanvasObject() {
      var canvasObject = GameObject.Find("ContextMenuCanvas");
      if (canvasObject != null) {
        this.canvasObject = canvasObject;
      } else {
        Debug.LogError("Cannot find ContextMenuCanvas. Please add it in the hierarchy.");
      }
    }

    private void LoadPrefabs() {
      this.panelPrefab = Resources.Load<GameObject>("MyPackage/ContextMenu/MenuPanel");
      this.selectionPrefab = Resources.Load<GameObject>("MyPackage/ContextMenu/SelectionPanel");
      this.textPrefab = Resources.Load<GameObject>("MyPackage/ContextMenu/MenuText");
    }

    private void CreatePanel() {
      this.panelObject = GameObject.Instantiate(this.panelPrefab, this.canvasObject.transform);
      this.selectionObject = GameObject.Instantiate(this.selectionPrefab, this.panelObject.transform);
      UpdatePanelObject();
      UpdateSelectionObject();
    }

    private void ToggleDisplayed() {
      var pushed = this.controller.GetButtonDown(this.toggleMenuButton, this.locq);
      if (pushed) {
        if (this.displayed) {
          Close();
        } else {
          Open();
        }
      }
    }

    private void ChangeSelectedIndex() {
      if (this.displayed) {
        var downPushed = this.controller.GetButtonDown(this.downButton, this.locq, repeat: true);
        var upPushed = this.controller.GetButtonDown(this.upButton, this.locq, repeat: true);
        if (downPushed) {
          MoveCursorDown();
        }
        if (upPushed) {
          MoveCursorUp();
        }
      }
    }

    private void ExecuteAction() {
      if (this.displayed) {
        var pushed = this.controller.GetButtonDown(this.confirmButton, this.locq);
        if (pushed) {
          this.selectionObject.GetComponent<SelectionManager>().Select();
          var item = this.innerItems[this.cursorIndex];
          if (item.action != null) {
            item.action();
          }
        }
      }
    }

    public MenuItem FindItem(Predicate<MenuItem> oldPredicate) {
      return this.innerItems.Find(oldPredicate);
    }

    public MenuItem FindItem(string oldMessage) {
      return FindItem((item) => item.message == oldMessage);
    }

    public List<MenuItem> FindAllItems(Predicate<MenuItem> oldPredicate) {
      return this.innerItems.FindAll(oldPredicate);
    }

    public List<MenuItem> FindAllItems(string oldMessage) {
      return FindAllItems((item) => item.message == oldMessage);
    }

    public void AddItem(MenuItem newItem) {
      this.innerItems.Add(newItem);
      UpdateItemObjects();
    }

    public void RemoveItem(MenuItem oldItem) {
      var index = this.innerItems.IndexOf(oldItem);
      if (index >= 0) {
        this.innerItems.RemoveAt(index);
        if (index <= this.cursorIndex) {
          this.cursorIndex --;
        }
        UpdateItemObjects();
      }
    }

    public void ReplaceItem(MenuItem oldItem, MenuItem newItem) {
      var index = this.innerItems.IndexOf(oldItem);
      if (index >= 0) {
        this.innerItems.RemoveAt(index);
        this.innerItems.Insert(index, newItem);
        UpdateItemObjects();
      }
    }

    public void ChangeItemMessage(MenuItem oldItem, string newMessage) {
      var index = this.innerItems.IndexOf(oldItem);
      if (index >= 0) {
        this.innerItems[index].message = newMessage;
        UpdateItemObjects();
      }
    }

    public void MoveCursorDown() {
      this.cursorIndex ++;
    }

    public void MoveCursorUp() {
      this.cursorIndex --;
    }

    public void Open() {
      this.displayed = true;
    }

    public void Close() {
      this.displayed = false;
    }

    // メニューのパネルに関する状態変化 (表示/非表示の変更など) があった場合に必ず実行してください。
    private void UpdatePanelObject() {
      this.panelObject.SetActive(this.displayed);
    }

    // カーソルに関する状態変化 (カーソル位置など) があった場合に必ず実行してください。
    private void UpdateSelectionObject() {
      var height = GetItemHeight();
      var selectionTransform = this.selectionObject.GetComponent<RectTransform>();
      var selectionPosition = selectionTransform.anchoredPosition;
      selectionPosition.y = -cursorIndex * height - 10;
      selectionTransform.anchoredPosition = selectionPosition;
    }

    // メニュー項目 (項目の追加やテキストの変更など) があった場合に必ず実行してください。
    private void UpdateItemObjects() {
      var height = GetItemHeight();
      var totalHeight = height * this.innerItems.Count + 20;
      foreach (Transform child in this.panelObject.transform) {
        var childObject = child.gameObject;
        if (childObject.GetComponent<Text>() != null) {
          GameObject.Destroy(child.gameObject);
        }
      }
      for (var index = 0 ; index < this.innerItems.Count ; index ++) {
        var item = this.innerItems[index];
        var textObject = GameObject.Instantiate(this.textPrefab, this.panelObject.transform);
        textObject.GetComponent<Text>().text = item.message;
        var textTransform = textObject.GetComponent<RectTransform>();
        var textPosition = textTransform.anchoredPosition;
        textPosition.y = -index * height - 10;
        textTransform.anchoredPosition = textPosition;
      }
      var size = this.panelObject.GetComponent<RectTransform>().sizeDelta;
      size.y = totalHeight;
      this.panelObject.GetComponent<RectTransform>().sizeDelta = size;
    }

    private float GetItemHeight() {
      var height = this.textPrefab.GetComponent<RectTransform>().sizeDelta.y;
      return height;
    }

  }

}