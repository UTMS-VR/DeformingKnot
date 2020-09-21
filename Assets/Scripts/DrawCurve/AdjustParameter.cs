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
            List<Vector3> newPositions = new List<Vector3>();
            newPositions.Add(positions[0]);
            float remainder = 0.0f;
            float temporarySegment = TemporarySegment(positions, segment, closed);

            for (int i = 1; i < length; i++)
            {
                Completion(ref newPositions, positions[i - 1], positions[i], ref remainder, temporarySegment);
            }

            if (closed)
            {
                Completion(ref newPositions, positions[length - 1], positions[0], ref remainder, temporarySegment);

                if (newPositions.Count > DivisionNumber(positions, segment, closed))
                {
                    newPositions.Remove(newPositions[newPositions.Count - 1]);
                }
            }
            else
            {
                if (newPositions.Count < DivisionNumber(positions, segment, closed) + 1)
                {
                    newPositions.Add(positions[length - 1]);
                }
            }

            positions = newPositions;
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

        public static float TemporarySegment(List<Vector3> positions, float segment, bool closed)
        {
            return ArcLength(positions, closed) / DivisionNumber(positions, segment, closed);
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
                float s = (distance - remainder) / distance;
                newPositions.Add(start + (end - start) * s);
            }
        }

        public static void Shift(ref List<Vector3> positions, int n) // 0 <= n < positions.Count
        {
            List<Vector3> newPositions = new List<Vector3>();

            for (int i = n; i < positions.Count; i++)
            {
                newPositions.Add(positions[i]);
            }

            for (int i = 0; i < n; i++)
            {
                newPositions.Add(positions[i]);
            }

            positions = newPositions;
        }
    }
}
