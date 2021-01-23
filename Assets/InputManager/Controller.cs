﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InputManager
{
    public class Lock
    {
        private readonly int id;
        public readonly uint level;

        private Lock(int id, uint level)
        {
            if (level == 0)
            {
                throw new System.Exception("Lock level must be positive");
            }
            this.id = id;
            this.level = level;
        }

        private static int CreateId(uint level)
        {
            return System.Environment.TickCount;
        }

        public static Lock Create(uint level)
        {
            return new Lock(Lock.CreateId(level), level);
        }

        public override string ToString()
        {
            return $"LockId(id: {this.id}, level: {this.level})";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            Lock other = (Lock)obj;
            return (this.id == other.id);
        }

        public override int GetHashCode()
        {
            return this.id;
        }

        //public static uint GetLevel(Lock lc)
        //{
        //    if (lc == null)
        //    {
        //        return 0;
        //    }
        //    return lc.level;
        //}
    }

    public class Controller
    {
        private ButtonMap buttonMap;
        private StickMap stickMap;
        private PositionDeviceMap positionDeviceMap;
        private List<Lock> locks;
        private Dictionary<LogicalButton, uint?> buttonLockLevels;

        public Controller(ButtonMap buttonMap, StickMap stickMap, PositionDeviceMap positionDeviceMap)
        {
            this.buttonMap = buttonMap;
            this.stickMap = stickMap;
            this.positionDeviceMap = positionDeviceMap;
            this.locks = new List<Lock> { };
            this.buttonLockLevels = new Dictionary<LogicalButton, uint?> { };
        }

        public void UpdateFirst()
        {
            this.buttonMap.UpdateFirst();
            this.positionDeviceMap.UpdateFirst();
            this.UpdateLockFirst();
        }

        public void UpdateLast()
        {
            this.UpdateLockLast();
        }

        private void UpdateLockFirst()
        {
            uint level = this.GetLockLevel();
            if (level > 0)
            {
                // ↓ToListしないと、「foreachの途中にDictionaryを変更するな」というエラーが出る
                foreach (LogicalButton logicalButton in this.buttonLockLevels.Keys.ToList())
                {
                    if (this.buttonMap.GetDown(logicalButton))
                    {
                        this.buttonLockLevels[logicalButton] = level;
                    }
                }
            }
        }

        private void UpdateLockLast()
        {
            foreach (LogicalButton logicalButton in this.buttonLockLevels.Keys.ToList())
            {
                if (this.buttonMap.GetUp(logicalButton))
                {
                    this.buttonLockLevels[logicalButton] = null;
                }
            }
        }

        private uint GetLockLevel()
        {
            int count = this.locks.Count;
            if (count == 0) {
                return 0;
            }
            else
            {
                return this.locks[count - 1].level;
            }
        }

        public Lock GetLock(uint level)
        {
            if (level <= this.GetLockLevel())
            {
                return null;
            }
            else
            {
                foreach (LogicalButton logicalButton in this.buttonMap.GetLogicalButtons())
                {
                    if (!this.buttonLockLevels.ContainsKey(logicalButton))
                    {
                        this.buttonLockLevels[logicalButton] = null;
                    }
                }
                var lc = Lock.Create(level);
                this.locks.Add(lc);
                return lc;
            }
        }

        public void Unlock(Lock lc)
        {
            this.locks.Remove(lc);
            // TODO: empty になったら this.buttonLockLevels をクリアしても良いかも？
        }

        private bool GetButtonInternal(System.Func<LogicalButton, bool> func, LogicalButton logicalButton, Lock lc = null)
        {
            uint? buttonLockLevel = 
                this.buttonLockLevels.ContainsKey(logicalButton) ? this.buttonLockLevels[logicalButton] : null;
            if (this.GetLockLevel() == 0)
            {
                if (buttonLockLevel == null)
                {
                    return func(logicalButton);
                }
                return false;
            }
            else
            {
                uint level = lc?.level ?? 0;
                if (buttonLockLevel == null)
                {
                    // logicalButton がロックされていない場合
                    return func(logicalButton);
                }
                if (level < buttonLockLevel)
                {
                    return false;
                }
                if (level > buttonLockLevel)
                {
                    throw new System.Exception("This lock has been already unlocked");
                }
                return func(logicalButton);
            }
        }

        public bool GetButton(LogicalButton logicalButton, Lock lc = null)
        {
            return this.GetButtonInternal(this.buttonMap.Get, logicalButton, lc);
        }

        public bool GetButtonDown(LogicalButton logicalButton, Lock lc = null)
        {
            return this.GetButtonInternal(this.buttonMap.GetDown, logicalButton, lc);
        }

        public bool GetButtonUp(LogicalButton logicalButton, Lock lc = null)
        {
            return this.GetButtonInternal(this.buttonMap.GetUp, logicalButton, lc);
        }

        public Vector2 GetStick(LogicalStick logicalStick, Lock lc = null)
        {
            uint level = lc?.level ?? 0;
            if (level < this.GetLockLevel())
            {
                return Vector2.zero;
            }
            if (level > this.GetLockLevel())
            {
                throw new System.Exception("This lock has been already unlocked");
            }
            return this.stickMap.Get(logicalStick);
        }

        public Vector3 GetPosition(LogicalPositionDevice logicalPositionDevice)
        {
            return this.positionDeviceMap.GetPosition(logicalPositionDevice);
        }

        public Quaternion GetRotation(LogicalPositionDevice logicalPositionDevice)
        {
            return this.positionDeviceMap.GetRotation(logicalPositionDevice);
        }

        protected void MergeButtonMap(ButtonMap buttonMap)
        {
            this.buttonMap.Merge(buttonMap);
        }

        protected void MergeStickMap(StickMap stickMap)
        {
            this.stickMap.Merge(stickMap);
        }

        protected void MergePositionDeviceMap(PositionDeviceMap positionDeviceMap)
        {
            this.positionDeviceMap.Merge(positionDeviceMap);
        }
    }
}
