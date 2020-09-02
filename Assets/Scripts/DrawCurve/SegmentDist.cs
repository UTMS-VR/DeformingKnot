using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrawCurve
{
    public static class SegmentDist
    {
        private static float PSRate(Vector3 v, Vector3 w1, Vector3 w2)
        {
            return Vector3.Dot(v - w1, (w2 - w1).normalized) / (w2 - w1).magnitude;
        }

        private static float PSDist(Vector3 v, Vector3 w1, Vector3 w2)
        {
            if (w1 == w2)
            {
                return Vector3.Distance(v, w1);
            }

            float s = PSRate(v, w1, w2);

            if (s < 0)
            {
                return Vector3.Distance(v, w1);
            }
            else if (s > 1)
            {
                return Vector3.Distance(v, w2);
            }
            else
            {
                return Vector3.Distance(v, w1 + s * (w2 - w1));
            }
        }

        private static (float, float) SSRate(Vector3 v1, Vector3 v2, Vector3 w1, Vector3 w2)
        {
            Vector3 d = v2 - v1;
            Vector3 e = w2 - w1;
            float denom = d.sqrMagnitude * e.sqrMagnitude - Mathf.Pow(Vector3.Dot(d, e), 2);
            float s = Vector3.Dot(e.sqrMagnitude * d - Vector3.Dot(d, e) * e, w1 - v1) / denom;
            float t = Vector3.Dot(Vector3.Dot(d, e) * d - d.sqrMagnitude * e, w1 - v1) / denom;

            return (s, t);
        }

        public static float SSDist(Vector3 v1, Vector3 v2, Vector3 w1, Vector3 w2)
        {
            if (Vector3.Cross(v2 - v1, w2 - w1).magnitude == 0)
            {
                float s = SSRate(v1, v2, w1, w2).Item1;
                float t = SSRate(v1, v2, w1, w2).Item2;

                if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
                {
                    return Vector3.Distance(v1 + s * (v2 - v1), w1 + t * (w2 - w1));
                }
            }

            float d1 = PSDist(v1, w1, w2);
            float d2 = PSDist(v2, w1, w2);
            float d3 = PSDist(w1, v1, v2);
            float d4 = PSDist(w2, v1, v2);

            return Mathf.Min(d1, d2, d3, d4);
        }
    }
}