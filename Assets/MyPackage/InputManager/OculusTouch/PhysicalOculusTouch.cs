using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InputManager
{
    class PhysicalOculusTouchButton : IPhysicalButton
    {
        private OVRInput.RawButton rawButton;

        public PhysicalOculusTouchButton(OVRInput.RawButton rawButton)
        {
            this.rawButton = rawButton;
        }

        public bool Get()
        {
            return OVRInput.Get(this.rawButton);
        }

        public bool GetDown()
        {
            return OVRInput.GetDown(this.rawButton);
        }

        public bool GetUp()
        {
            return OVRInput.GetUp(this.rawButton);
        }

        public void UpdateFirst() { }
    }

    class PhysicalOculusTouchStick : IPhysicalStick
    {
        private OVRInput.RawAxis2D stick;

        public PhysicalOculusTouchStick(OVRInput.RawAxis2D stick)
        {
            this.stick = stick;
        }

        public Vector2 Get()
        {
            return OVRInput.Get(this.stick);
        }
    }

    enum HandAnchorKey
    {
        RightHand, LeftHand
    }

    class PhysicalOculusTouchPositionDevice : IPhysicalPositionDevice
    {
        private GameObject handAnchor;
        private bool onHeadset;

        public PhysicalOculusTouchPositionDevice(HandAnchorKey key)
        {
            string handAnchorName;
            switch (key)
            {
                case HandAnchorKey.RightHand:
                    handAnchorName = "RightHandAnchor";
                    break;
                case HandAnchorKey.LeftHand:
                    handAnchorName = "LeftHandAnchor";
                    break;
                default:
                    throw new System.Exception("This can't happen!");
            }
            this.handAnchor = GameObject.Find(handAnchorName);
            // headset上かどうか判定
#if UNITY_EDITOR
            this.onHeadset = false;
#else
            this.onHeadset = true;
#endif
        }

        public void UpdateFirst() { }

        public Vector3? GetPosition()
        {
            return this.handAnchor.GetComponent<Transform>().position;
        }

        public Quaternion? GetRotation()
        {
            return this.handAnchor.GetComponent<Transform>().rotation;
        }
    }
}