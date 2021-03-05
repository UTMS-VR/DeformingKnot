using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputManager
{
    class PhysicalKey : IPhysicalButton
    {
        private KeyCode keyCode;

        public PhysicalKey(KeyCode keyCode)
        {
            this.keyCode = keyCode;
        }

        public bool Get()
        {
            return Input.GetKey(this.keyCode);
        }

        public bool GetDown()
        {
            return Input.GetKeyDown(this.keyCode);
        }

        public bool GetUp()
        {
            return Input.GetKeyUp(this.keyCode);
        }

        public void UpdateFirst() { }
    }
}