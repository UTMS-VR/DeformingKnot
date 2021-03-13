using System.Collections.Generic;
using System.Linq;
using System;
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
        private Dictionary<LogicalButton, (DateTime start, uint count, bool onRepeatFrame)> buttonHeldInfo;
        private uint repeatDelay;  // milliseconds
        private uint repeatInterval;  // milliseconds

        // デフォルト値の定義が複数箇所に渡るのを防ぐため、
        // コンストラクタ引数での repeatDelay と repeatInterval のデフォルト値は null とし、
        // コンストラクタ内部で本来のデフォルト値を代入するようにしてあります。
        public Controller(ButtonMap buttonMap, StickMap stickMap, PositionDeviceMap positionDeviceMap,
            uint? repeatDelay = null, uint? repeatInterval = null)
        {
            this.buttonMap = buttonMap;
            this.stickMap = stickMap;
            this.positionDeviceMap = positionDeviceMap;
            this.locks = new List<Lock> { };
            this.buttonLockLevels = new Dictionary<LogicalButton, uint?> { };
            this.buttonHeldInfo = new Dictionary<LogicalButton, (DateTime start, uint count, bool onRepeatFrame)> { };
            this.repeatDelay = repeatDelay ?? 500;
            this.repeatInterval = repeatInterval ?? 75;
        }

        public void UpdateFirst()
        {
            this.buttonMap.UpdateFirst();
            this.positionDeviceMap.UpdateFirst();
            this.UpdateLockFirst();
            this.UpdateButtonHeldFramesFirst();
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

        private void UpdateButtonHeldFramesFirst()
        {
            foreach (LogicalButton logicalButton in this.buttonMap.GetLogicalButtons())
            {
                if (this.buttonMap.GetDown(logicalButton))
                {
                    this.buttonHeldInfo[logicalButton] = (DateTime.Now, 0, false);
                }
                else if (this.buttonMap.GetUp(logicalButton))
                {
                    this.buttonHeldInfo.Remove(logicalButton);
                }
                else if (this.buttonMap.Get(logicalButton))
                {
                    var (start, count, onRepeatFrame) = this.buttonHeldInfo[logicalButton];
                    if (onRepeatFrame)
                    {
                        this.buttonHeldInfo[logicalButton] = (start, count, false);
                    } 
                    else
                    {
                        TimeSpan heldTimeSpan = DateTime.Now - start;
                        uint heldMilliseconds = (uint)heldTimeSpan.TotalMilliseconds;
                        if (heldMilliseconds > this.repeatDelay + this.repeatInterval * count)
                        {
                            this.buttonHeldInfo[logicalButton] = (start, count + 1, true);
                        }
                    }
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

        private bool HasAccessToButton(LogicalButton logicalButton, Lock lc)
        {
            uint? buttonLockLevel =
                this.buttonLockLevels.ContainsKey(logicalButton) ? this.buttonLockLevels[logicalButton] : null;
            if (this.GetLockLevel() == 0)
            {
                // Lock が一切取得されていない場合だけど、この場合分け要る？
                return buttonLockLevel == null;
            }
            else
            {
                uint level = lc?.level ?? 0;
                if (buttonLockLevel == null)
                {
                    // logicalButton が Lock されていない場合
                    return true;
                }
                if (level < buttonLockLevel)
                {
                    return false;
                }
                if (level > buttonLockLevel)
                {
                    throw new System.Exception("This lock has been already unlocked");
                }
                return true;
            }
        }


        public bool GetButton(LogicalButton logicalButton, Lock lc = null)
        {
            if (!this.HasAccessToButton(logicalButton, lc))
            {
                return false;
            }
            return this.buttonMap.Get(logicalButton);
        }

        public bool GetButtonDown(LogicalButton logicalButton, Lock lc = null, bool repeat = false)
        {
            if (!this.HasAccessToButton(logicalButton, lc))
            {
                return false;
            }
            if (this.buttonMap.GetDown(logicalButton))
            {
                return true;
            }
            if (!repeat)
            {
                return false;
            }
            // 以下、repeat する場合
            if (!this.buttonHeldInfo.ContainsKey(logicalButton))
            {
                return false;
            }
            return this.buttonHeldInfo[logicalButton].onRepeatFrame;
        }


        public bool GetButtonUp(LogicalButton logicalButton, Lock lc = null)
        {
            if (!this.HasAccessToButton(logicalButton, lc)) {
                return false;
            }
            return this.buttonMap.GetUp(logicalButton);
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
