using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static OVRInput;

namespace DebugUtil
{
    using ButtonMapData = List<(RawButton button, KeyCode key)>;

    public class ButtonMap // : ButtonMapData
    {
        private Dictionary<RawButton, List<KeyCode>> mappedKeys = new Dictionary<RawButton, List<KeyCode>> { };
        public ButtonMap(ButtonMapData data)
        {
            foreach (var map in data)
            {
                if (this.mappedKeys.ContainsKey(map.button))
                {
                    this.mappedKeys[map.button].Add(map.key);
                }
                else
                {
                    this.mappedKeys[map.button] = new List<KeyCode> { map.key };
                }
            }
        }

        /*public static implicit operator ButtonMap(ButtonMapData data)
        {
            return new ButtonMap(data);
        }*/

        public static ButtonMap Empty = new ButtonMap(new ButtonMapData { });

        public static ButtonMap LiteralKeys = new ButtonMap(new ButtonMapData{
            ( RawButton.A, KeyCode.A ),
            ( RawButton.B, KeyCode.B ),
            ( RawButton.X, KeyCode.X ),
            ( RawButton.Y, KeyCode.Y ),
            ( RawButton.RIndexTrigger, KeyCode.R ),
            ( RawButton.LIndexTrigger, KeyCode.L )
        });

        public static ButtonMap PositionalKeys = new ButtonMap(new ButtonMapData{
            ( RawButton.A, KeyCode.Period ),
            ( RawButton.B, KeyCode.Slash ),
            ( RawButton.X, KeyCode.X ),
            ( RawButton.Y, KeyCode.Z ),
            ( RawButton.RIndexTrigger, KeyCode.P ),
            ( RawButton.LIndexTrigger, KeyCode.Q )
        });

        public bool Get(RawButton button)
        {
            if (!this.mappedKeys.ContainsKey(button))
            {
                return false;
            }
            return this.mappedKeys[button].Any(key => Input.GetKey(key));
        }

        public bool GetDown(RawButton button)
        {
            if (!this.mappedKeys.ContainsKey(button))
            {
                return false;
            }
            return this.mappedKeys[button].Any(key => Input.GetKeyDown(key));
        }

        public bool GetUp(RawButton button)
        {
            if (!this.mappedKeys.ContainsKey(button))
            {
                return false;
            }
            return this.mappedKeys[button].Any(key => Input.GetKeyUp(key));
        }
    }

}