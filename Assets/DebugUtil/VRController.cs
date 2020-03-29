using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugUtil
{
    public class VRController
    {
        private string handAnchorName; // warning の表示に使うだけ
        private GameObject handAnchor;
        private OVRInput.RawAxis2D stick;
        private Stick2DMap stickMap;
        private Stick3DMap positionMover;
        private Vector3 position = new Vector3(0, 0, 0);
        private GameObject cube;
        private bool onHeadSet;

        public VRController(string handAnchorName, OVRInput.RawAxis2D stick, Stick2DMap stickMap, Stick3DMap positionMover, bool onHeadSet)
        {
            this.handAnchor = GameObject.Find(handAnchorName);
            this.stick = stick;
            this.stickMap = stickMap;
            this.positionMover = positionMover;
            this.onHeadSet = onHeadSet;

            if (!onHeadSet)
            {
                this.cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                this.cube.transform.position = this.position;
                this.cube.transform.localScale = new Vector3(1, 1, 1) * 0.03f;
            }
        }

        public void Update()
        {
            if (!this.onHeadSet)
            {
                if (this.positionMover != null)
                {
                    this.position += this.positionMover.ToVector3() * 0.01f;
                }
                this.cube.transform.position = this.position;
            }
        }

        public Vector2 GetStick()
        {
            if (this.onHeadSet)
            {
                return OVRInput.Get(this.stick);
            }
            else
            {
                if (this.stickMap == null)
                {
                    Debug.LogWarning($"VRController.stickMap ({this.handAnchorName}) is null");
                    return new Vector2(0, 0);
                }
                else
                {
                    return this.stickMap.ToVector2();
                }
            }
        }

        public Vector3 GetPosition()
        {
            if (this.onHeadSet)
            {
                return this.handAnchor.GetComponent<Transform>().position;
            }
            else
            {
                return this.position;
            }
        }

        public Quaternion GetRotation()
        {
            if (this.onHeadSet)
            {
                return this.handAnchor.GetComponent<Transform>().rotation;
            }
            else
            {
                Debug.Log("GetRotation is not suppported on non-VRheadset environment");
                return Quaternion.identity;
            }
        }
    }


}