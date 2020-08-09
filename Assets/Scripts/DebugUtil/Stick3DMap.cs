using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugUtil
{
    using Stick3DMapData = List<(Stick3D stickDirection, KeyCode key)>;

    public enum Stick3D
    {
        Up, Down, Left, Right, Above, Below
    }

    public class Stick3DMap // : Stick3DMapData
    {
        private Dictionary<Stick3D, List<KeyCode>> mappedKeys = new Dictionary<Stick3D, List<KeyCode>> { };
        public Stick3DMap(Stick3DMapData data)
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

        public static Stick3DMap Empty = new Stick3DMap(new Stick3DMapData { });

        public static Stick3DMap WASDEC = new Stick3DMap(new Stick3DMapData
        {
            ( Stick3D.Up, KeyCode.W ),
            ( Stick3D.Down, KeyCode.S ),
            ( Stick3D.Right, KeyCode.D ),
            ( Stick3D.Left, KeyCode.A ),
            ( Stick3D.Above, KeyCode.E ),
            ( Stick3D.Below, KeyCode.C )
        });

        /*
        public static Stick3DMap ijklum = new Stick3DMap(new Stick3DMapData
        {
            ( Stick3D.Up, KeyCode.I ),
            ( Stick3D.Down, KeyCode.K ),
            ( Stick3D.Right, KeyCode.L ),
            ( Stick3D.Left, KeyCode.J ),
            ( Stick3D.Above, KeyCode.U ),
            ( Stick3D.Below, KeyCode.M )
        });
        */

        public static Stick3DMap OKLSemiIComma = new Stick3DMap(new Stick3DMapData
        {
            ( Stick3D.Up, KeyCode.O ),
            ( Stick3D.Down, KeyCode.L ),
            ( Stick3D.Right, KeyCode.Semicolon ),
            ( Stick3D.Left, KeyCode.K ),
            ( Stick3D.Above, KeyCode.I ),
            ( Stick3D.Below, KeyCode.Comma )
        });

        public bool Get(Stick3D stickDirection)
        {
            if (!this.mappedKeys.ContainsKey(stickDirection))
            {
                return false;
            }
            return this.mappedKeys[stickDirection].Any(key => Input.GetKey(key));
        }

        public Vector3 ToVector3()
        {
            Vector3 direction = new Vector3(0, 0, 0);
            if (this.Get(Stick3D.Up))
            {
                direction += new Vector3(0, 1, 0);
            }
            if (this.Get(Stick3D.Down))
            {
                direction += new Vector3(0, -1, 0);
            }
            if (this.Get(Stick3D.Right))
            {
                direction += new Vector3(1, 0, 0);
            }
            if (this.Get(Stick3D.Left))
            {
                direction += new Vector3(-1, 0, 0);
            }
            if (this.Get(Stick3D.Above))
            {
                direction += new Vector3(0, 0, 1);
            }
            if (this.Get(Stick3D.Below))
            {
                direction += new Vector3(0, 0, -1);
            }
            return direction;
        }
    }
}