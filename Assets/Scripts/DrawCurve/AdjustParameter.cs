using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrawCurve
{
    public class AdjustParameter
    {
        public static void Equalize(ref List<Vector3> positions, float segment, bool closed)
        {
            int length = positions.Count;
            List<Vector3> newpositions = new List<Vector3>();
            newpositions.Add(positions[0]);
            float remainder = 0.0f;
            float temporarySegment = ArcLength(positions, closed) / DivisionNumber(positions, segment, closed);

            for (int i = 1; i < length; i++)
            {
                Completion(ref newpositions, positions[i - 1], positions[i], ref remainder, temporarySegment);
            }

            if (closed)
            {
                Completion(ref newpositions, positions[length - 1], positions[0], ref remainder, temporarySegment);

                if (newpositions.Count > DivisionNumber(positions, segment, closed))
                {
                    newpositions.Remove(newpositions[newpositions.Count - 1]);
                }
            }
            else
            {
                if (newpositions.Count < DivisionNumber(positions, segment, closed) + 1)
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
            List<Vector3> newpositions = new List<Vector3>();

            for (int i = n; i < positions.Count; i++)
            {
                newpositions.Add(positions[i]);
            }

            for (int i = 0; i < n; i++)
            {
                newpositions.Add(positions[i]);
            }

            positions = newpositions;
        }
    }
}
