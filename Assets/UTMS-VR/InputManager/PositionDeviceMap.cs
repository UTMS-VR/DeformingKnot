using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputManager
{
    using PositionDeviceMapData = List<(
        LogicalPositionDevice logicalPositionDevice,
        IPhysicalPositionDevice physicalPositionDevice)>;

    public class PositionDeviceMap
    {
        private Dictionary<LogicalPositionDevice, List<IPhysicalPositionDevice>> mappedPositionDevices;
        private Vector3 defaultPosition = Vector3.zero;
        private Quaternion defaultRotation = Quaternion.identity;

        public PositionDeviceMap(PositionDeviceMapData data)
        {
            this.mappedPositionDevices = new Dictionary<LogicalPositionDevice, List<IPhysicalPositionDevice>> { };
            foreach (var map in data)
            {
                if (this.mappedPositionDevices.ContainsKey(map.logicalPositionDevice))
                {
                    this.mappedPositionDevices[map.logicalPositionDevice].Add(map.physicalPositionDevice);
                }
                else
                {
                    this.mappedPositionDevices[map.logicalPositionDevice] =
                        new List<IPhysicalPositionDevice> { map.physicalPositionDevice };
                }
            }
        }

        public void Merge(PositionDeviceMap other)
        {
            foreach (var mappedPositionDevices in other.mappedPositionDevices)
            {
                LogicalPositionDevice logicalPositionDevice = mappedPositionDevices.Key;
                List<IPhysicalPositionDevice> physicalPositionDevices = mappedPositionDevices.Value;
                if (this.mappedPositionDevices.ContainsKey(logicalPositionDevice))
                {
                    this.mappedPositionDevices[logicalPositionDevice].AddRange(physicalPositionDevices);
                }
                else
                {
                    this.mappedPositionDevices[logicalPositionDevice] = physicalPositionDevices;
                }
            }
        }

        public void UpdateFirst()
        {
            foreach (var physicalPositionDeviceList in this.mappedPositionDevices.Values)
            {
                foreach (var physicalPositionDevice in physicalPositionDeviceList)
                {
                    physicalPositionDevice.UpdateFirst();
                }
            }
        }

        public Vector3 GetPosition(LogicalPositionDevice logicalPositionDevice)
        {
            if (!this.mappedPositionDevices.ContainsKey(logicalPositionDevice)){
                return this.defaultPosition;
            }
            foreach (var physicalPositionDevice in this.mappedPositionDevices[logicalPositionDevice])
            {
                Vector3? position = physicalPositionDevice.GetPosition();
                if (position != null)
                {
                    return (Vector3)position;
                }
            }
            return this.defaultPosition;
        }

        public Quaternion GetRotation(LogicalPositionDevice logicalPositionDevice)
        {
            if (!this.mappedPositionDevices.ContainsKey(logicalPositionDevice))
            {
                return this.defaultRotation;
            }
            foreach (var physicalPositionDevice in this.mappedPositionDevices[logicalPositionDevice])
            {
                Quaternion? rotation = physicalPositionDevice.GetRotation();
                if (rotation != null)
                {
                    return (Quaternion)rotation;
                }
            }
            return this.defaultRotation;
        }
    }
}