using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputManager
{
    public class KeyTo3D
    {
        public KeyCode up;
        public KeyCode down;
        public KeyCode right;
        public KeyCode left;
        public KeyCode above;
        public KeyCode below;
    }

    public class PhysicalKeyPositionDevice : IPhysicalPositionDevice
    {
        private Vector3 position;
        private float speed;
        private GameObject cube;

        private KeyCode up;
        private KeyCode down;
        private KeyCode right;
        private KeyCode left;
        private KeyCode above;
        private KeyCode below;

        //public PhysicalKeyPositionDevice(
        //    KeyCode up, KeyCode down,
        //    KeyCode right, KeyCode left,
        //    KeyCode above, KeyCode below,
        //    float speed = 0.1f, float cubeScale = 0.3f)
        //{
        //    this.up = up;
        //    this.down = down;
        //    this.right = right;
        //    this.left = left;
        //    this.above = above;
        //    this.below = below;
        //    this.speed = speed;
        //}

        public PhysicalKeyPositionDevice(
            KeyTo3D keyTo3D, 
            float speed = 0.1f, float cubeScale = 0.3f
            )
        {
            this.up = keyTo3D.up;
            this.down = keyTo3D.down;
            this.right = keyTo3D.right;
            this.left = keyTo3D.left;
            this.above = keyTo3D.above;
            this.below = keyTo3D.below;

            this.speed = speed;

            // headset 上のときは非表示にしたい
            this.cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            this.cube.transform.position = this.position;
            this.cube.transform.localScale = new Vector3(1, 1, 1) * cubeScale;
        }

        public void UpdateFirst()
        {
            this.position += this.GetVelocity();
            this.cube.transform.position = this.position;
        }

        private Vector3 GetVelocity()
        {
            Vector3 direction = new Vector3(0, 0, 0);
            if (Input.GetKey(this.up))
            {
                direction += new Vector3(0, 1, 0);
            }
            if (Input.GetKey(this.down))
            {
                direction += new Vector3(0, -1, 0);
            }
            if (Input.GetKey(this.right))
            {
                direction += new Vector3(1, 0, 0);
            }
            if (Input.GetKey(this.left))
            {
                direction += new Vector3(-1, 0, 0);
            }
            if (Input.GetKey(this.above))
            {
                direction += new Vector3(0, 0, 1);
            }
            if (Input.GetKey(this.below))
            {
                direction += new Vector3(0, 0, -1);
            }
            return direction * this.speed;
        }

        public Vector3? GetPosition()
        {
            return this.position;
        }

        public Quaternion? GetRotation()
        {
            Debug.Log("GetRotation is not supported on non-VRheadset environment");
            return null;
        }
    }
}
