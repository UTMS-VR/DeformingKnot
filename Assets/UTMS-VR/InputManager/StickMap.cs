using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InputManager
{
    using StickMapData = List<(LogicalStick logicalStick, IPhysicalStick physicalStick)>;
    //using StickMapData = List<(LogicalStick logicalStick, PhysicalKey up, PhysicalKey down, PhysicalKey right, PhysicalKey left)>

    public class StickMap
    {
        private Dictionary<LogicalStick, List<IPhysicalStick>> mappedSticks;

        public StickMap(StickMapData data)
        {
            this.mappedSticks = new Dictionary<LogicalStick, List<IPhysicalStick>> { };
            foreach (var map in data)
            {
                if (this.mappedSticks.ContainsKey(map.logicalStick))
                {
                    this.mappedSticks[map.logicalStick].Add(map.physicalStick);
                }
                else
                {
                    this.mappedSticks[map.logicalStick] = new List<IPhysicalStick> { map.physicalStick };
                }
            }
        }

        public void Merge(StickMap other)
        {
            foreach (var mappedStick in other.mappedSticks)
            {
                LogicalStick logicalStick = mappedStick.Key;
                List<IPhysicalStick> physicalSticks = mappedStick.Value;
                if (this.mappedSticks.ContainsKey(logicalStick))
                {
                    this.mappedSticks[logicalStick].AddRange(physicalSticks);
                }
                else
                {
                    this.mappedSticks[logicalStick] = physicalSticks;
                }
            }
        }

        public Vector2 Get(LogicalStick logicalStick)
        {
            if (!this.mappedSticks.ContainsKey(logicalStick))
            {
                return new Vector2(0, 0);
            }
            Vector2 direction = new Vector2(0, 0);
            foreach (var physicalStick in this.mappedSticks[logicalStick])
            {
                direction += physicalStick.Get();
            }
            return direction;
        }

        //public List<LogicalStick> GetLogicalSticks()
        //{
        //    return this.mappedSticks.Keys.ToList();
        //}
    }
}