using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InputManager
{
    using ButtonMapData = List<(LogicalButton logicalButton, IPhysicalButton physicalButton)>;

    public class ButtonMap
    {
        private Dictionary<LogicalButton, List<IPhysicalButton>> mappedButtons;
        public ButtonMap(ButtonMapData data)
        {
            this.mappedButtons = new Dictionary<LogicalButton, List<IPhysicalButton>> { };
            foreach (var map in data){
                if (this.mappedButtons.ContainsKey(map.logicalButton))
                {
                    this.mappedButtons[map.logicalButton].Add(map.physicalButton);
                }
                else
                {
                    this.mappedButtons[map.logicalButton] = new List<IPhysicalButton> { map.physicalButton };
                }
            }
        }

        public void Merge(ButtonMap other)
        {
            foreach (var mappedButton in other.mappedButtons)
            {
                LogicalButton logicalButton = mappedButton.Key;
                List<IPhysicalButton> physicalButtons = mappedButton.Value;
                if (this.mappedButtons.ContainsKey(logicalButton))
                {
                    this.mappedButtons[logicalButton].AddRange(physicalButtons);
                }
                else
                {
                    this.mappedButtons[logicalButton] = physicalButtons;
                }
            }
        }

        public void UpdateFirst()
        {
            foreach (var physicalButtonList in this.mappedButtons.Values)
            {
                foreach (var physicalButton in physicalButtonList)
                {
                    physicalButton.UpdateFirst();
                }
            }
        }

        public bool Get(LogicalButton logicalButton)
        {
            if (!this.mappedButtons.ContainsKey(logicalButton))
            {
                return false;
            }
            return this.mappedButtons[logicalButton].Any(physicalButton => physicalButton.Get());
        }

        public bool GetDown(LogicalButton logicalButton)
        {
            if (!this.mappedButtons.ContainsKey(logicalButton))
            {
                return false;
            }
            return this.mappedButtons[logicalButton].Any(physicalButton => physicalButton.GetDown());
        }

        public bool GetUp(LogicalButton logicalButton)
        {
            if (!this.mappedButtons.ContainsKey(logicalButton))
            {
                return false;
            }
            return this.mappedButtons[logicalButton].Any(physicalButton => physicalButton.GetUp());
        }

        public List<LogicalButton> GetLogicalButtons()
        {
            return this.mappedButtons.Keys.ToList();
        }
    }
}