using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputManager
{
    public interface IPhysicalButton
    {
        void UpdateFirst();
        bool Get();
        bool GetDown();
        bool GetUp();
    }

    public interface IPhysicalStick
    {
        Vector2 Get();
    }

    public interface IPhysicalPositionDevice
    {
        void UpdateFirst();
        Vector3? GetPosition();
        Quaternion? GetRotation();
    }
}