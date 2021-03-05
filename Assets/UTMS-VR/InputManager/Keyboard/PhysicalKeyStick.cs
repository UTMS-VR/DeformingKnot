using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputManager
{
    public class KeyTo2D
    {
        public KeyCode up;
        public KeyCode down;
        public KeyCode right;
        public KeyCode left;
    }

    public class PhysicalKeyStick : IPhysicalStick
    {
        private KeyCode up;
        private KeyCode down;
        private KeyCode right;
        private KeyCode left;

        //public PhysicalKeyStick(KeyCode up, KeyCode down, KeyCode right, KeyCode left)
        //{
        //    this.up = up;
        //    this.down = down;
        //    this.right = right;
        //    this.left = left;
        //}

        public PhysicalKeyStick(KeyTo2D keyTo2D)
        {
            this.up = keyTo2D.up;
            this.down = keyTo2D.down;
            this.right = keyTo2D.right;
            this.left = keyTo2D.left;
        }

        public Vector2 Get()
        {
            Vector2 direction = new Vector2(0, 0);

            if (Input.GetKey(this.up))
            {
                direction += new Vector2(0, 1);
            }
            if (Input.GetKey(this.down))
            {
                direction += new Vector2(0, -1);
            }
            if (Input.GetKey(this.right))
            {
                direction += new Vector2(1, 0);
            }
            if (Input.GetKey(this.left))
            {
                direction += new Vector2(-1, 0);
            }
            return direction;
        }
    }
}
