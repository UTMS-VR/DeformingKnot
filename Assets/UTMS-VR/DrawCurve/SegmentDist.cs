using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrawCurve
{
    public static class SegmentDist
    {
        // 点 v から直線 w1w2 へ下ろした垂線の足を点 w3 としたときの, w3 - w1 = s * (w2 - w1) を満たす s を返す. 
        private static float PSRate(Vector3 v, Vector3 w1, Vector3 w2)
        {
            return Vector3.Dot(v - w1, (w2 - w1).normalized) / (w2 - w1).magnitude;
        }

        // 点 v と線分 w1w2 の間の距離を返す. 
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

        // 直線 v1v2, 直線 w1w2 と, その両方に直交する直線との交点をそれぞれ v3, w3 としたときの, 
        // v3 - v1 = s * (v2 - v1), w3 - w1 = t * (w2 - w1) を満たす s, t を返す. 
        private static (float, float) SSRate(Vector3 v1, Vector3 v2, Vector3 w1, Vector3 w2)
        {
            Vector3 d = v2 - v1;
            Vector3 e = w2 - w1;
            float denom = d.sqrMagnitude * e.sqrMagnitude - Mathf.Pow(Vector3.Dot(d, e), 2);
            float s = Vector3.Dot(e.sqrMagnitude * d - Vector3.Dot(d, e) * e, w1 - v1) / denom;
            float t = Vector3.Dot(Vector3.Dot(d, e) * d - d.sqrMagnitude * e, w1 - v1) / denom;

            return (s, t);
        }

        // 線分 v1v2 と線分 w1w2 の間の距離を返す. 
        public static float SSDist(Vector3 v1, Vector3 v2, Vector3 w1, Vector3 w2)
        {
            if (Vector3.Cross(v2 - v1, w2 - w1).magnitude > 0)
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