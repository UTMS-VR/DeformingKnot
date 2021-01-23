using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DrawCurve;
using InputManager;
using FileManager;

public class Main : MonoBehaviour
{
    private Controller controller;
    private Player player;
    private string text;

    // Start is called before the first frame update
    void Start()
    {
        this.controller = new Controller();
        this.player = new Player(this.controller);
        Curve.SetUp(this.controller.oculusTouch, this.controller.draw, this.controller.move);
    }

    // Update is called once per frame
    void Update()
    {
        this.controller.oculusTouch.UpdateFirst();

        this.text = this.player.state.ToString();

        if (this.player.state == State.BasicDeform && this.controller.ValidBaseButtonInput())
        {
            this.player.DeepCopy();
            this.player.ChangeState();
            this.player.Draw();
            this.player.Move();
            this.player.Select();
            this.player.Cut();
            this.player.Combine();
            this.player.Remove();
            this.player.Undo();
        }
        else if (this.player.state == State.ContiDeform)
        {
            this.player.ChangeState();
            this.player.Select();
            this.player.deformingCurve.Update();
        }

        this.player.Display();
        
        this.controller.oculusTouch.UpdateLast();
    }

    public void UpdateFixedInterface(FixedInterface.FixedInterfaceSetting setting)
    {
        setting.text = this.text;
        if (this.player.state == State.ContiDeform)
        {
            this.player.deformingCurve.UpdateFixedInterface(setting);
        }
    }
}