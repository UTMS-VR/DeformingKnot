using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DrawCurve
{
    public class AdjustParameter
    {
        public static void EqualizeP(ref List<Vector3> positions, float segment, bool closed)
        {
            int division = DivisionNumber(positions, segment, closed);
            EqualizeD(ref positions, division, closed);
        }

        public static void EqualizeL(ref List<Vector3> positions, float segment, int division, bool closed)
        {
            float arcLength = ArcLength(positions, closed);
            positions = positions.Select(v => v * segment * division / arcLength).ToList();
            EqualizeD(ref positions, division, closed);
        }

        public static void EqualizeD(ref List<Vector3> positions, int division, bool closed)
        {
            int length = positions.Count;
            float segment = ArcLength(positions, closed) / division;

            List<Vector3> newpositions = new List<Vector3>();
            newpositions.Add(positions[0]);
            float remainder = 0.0f;

            for (int i = 1; i < length; i++)
            {
                Completion(ref newpositions, positions[i - 1], positions[i], ref remainder, segment);
            }

            if (closed)
            {
                Completion(ref newpositions, positions[length - 1], positions[0], ref remainder, segment);

                if (newpositions.Count > division)
                {
                    newpositions.Remove(newpositions[newpositions.Count - 1]);
                }
            }
            else
            {
                if (newpositions.Count < division + 1)
                {
                    newpositions.Add(positions[length - 1]);
                }
            }

            positions = newpositions;
        }

        public static float ArcLength(List<Vector3> positions, bool closed)
        {
            int length = positions.Count;
            float arclength = 0.0f;

            for (int i = 1; i < length; i++)
            {
                arclength += Vector3.Distance(positions[i - 1], positions[i]);
            }

            if (closed)
            {
                arclength += Vector3.Distance(positions[length - 1], positions[0]);
            }

            return arclength;
        }

        private static int DivisionNumber(List<Vector3> positions, float segment, bool closed)
        {
            return Mathf.FloorToInt(ArcLength(positions, closed) / segment + 0.5f);
        }

        private static void Completion(ref List<Vector3> newPositions, Vector3 start, Vector3 end, ref float remainder, float segment)
        {
            float distance = Vector3.Distance(start, end);
            remainder += distance;

            while (segment < remainder)
            {
                remainder -= segment;
                newPositions.Add(start + (end - start) * (distance - remainder) / distance);
            }
        }

        public static void Shift(ref List<Vector3> positions, int n) // 0 <= n < positions.Count
        {
            for (int i = 0; i < n; i++)
            {
                Vector3 position = positions[0];
                positions.Remove(position);
                positions.Add(position);
            }
        }
    }
}
