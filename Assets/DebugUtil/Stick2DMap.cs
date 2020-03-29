using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugUtil
{
    using Stick2DMapData = List<(Stick2D stickDirection, KeyCode key)>;
    public enum Stick2D
    {
        Up, Down, Left, Right
    }

    public class Stick2DMap // : Stick2DMapData
    {
        private Dictionary<Stick2D, List<KeyCode>> mappedKeys = new Dictionary<Stick2D, List<KeyCode>> { };
        public Stick2DMap(Stick2DMapData data)
        {
            foreach (var map in data)
            {
                if (this.mappedKeys.ContainsKey(map.stickDirection))
                {
                    this.mappedKeys[map.stickDirection].Add(map.key);
                }
                else
                {
                    this.mappedKeys[map.stickDirection] = new List<KeyCode> { map.key };
                }
            }
        }

        public static Stick2DMap Arrows = new Stick2DMap(new Stick2DMapData
        {
            ( Stick2D.Up, KeyCode.UpArrow ),
            ( Stick2D.Down, KeyCode.DownArrow ),
            ( Stick2D.Right, KeyCode.RightArrow ),
            ( Stick2D.Left, KeyCode.LeftArrow )
        });

        public static Stick2DMap WASD = new Stick2DMap(new Stick2DMapData
        {
            ( Stick2D.Up, KeyCode.W ),
            ( Stick2D.Down, KeyCode.S ),
            ( Stick2D.Right, KeyCode.D ),
            ( Stick2D.Left, KeyCode.A )
        });

        /*
        public static Stick2DMap ijkl = new Stick2DMap(new Stick2DMapData {
            ( Stick2D.Up, KeyCode.I ),
            ( Stick2D.Down, KeyCode.K ),
            ( Stick2D.Right, KeyCode.L ),
            ( Stick2D.Left, KeyCode.J )
        });
        */

        public static Stick2DMap OKLSemi = new Stick2DMap(new Stick2DMapData {
            ( Stick2D.Up, KeyCode.O ),
            ( Stick2D.Down, KeyCode.L ),
            ( Stick2D.Right, KeyCode.K ),
            ( Stick2D.Left, KeyCode.Comma )
        });

        public bool Get(Stick2D stickDirection)
        {
            if (!this.mappedKeys.ContainsKey(stickDirection))
            {
                return false;
            }
            return this.mappedKeys[stickDirection].Any(key => Input.GetKey(key));
        }

        public Vector2 ToVector2()
        {
            Vector2 direction = new Vector2(0, 0);
            if (this.Get(Stick2D.Up))
            {
                direction += new Vector2(0, 1);
            }
            if (this.Get(Stick2D.Down))
            {
                direction += new Vector2(0, -1);
            }
            if (this.Get(Stick2D.Right))
            {
                direction += new Vector2(1, 0);
            }
            if (this.Get(Stick2D.Left))
            {
                direction += new Vector2(-1, 0);
            }
            return direction;
        }
    }

}