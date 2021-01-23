using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrawCurve
{
    public class AdjustParameter
    {
        public static List<Vector3> Equalize(List<Vector3> points, float segment, bool closed)
        {
            int length = points.Count;
            List<Vector3> newPoints = new List<Vector3>();
            newPoints.Add(points[0]);
            float remainder = 0.0f;
            float temporarySegment = TemporarySegment(points, segment, closed);

            for (int i = 1; i < length; i++)
            {
                Completion(ref newPoints, points[i - 1], points[i], ref remainder, temporarySegment);
            }

            if (closed)
            {
                Completion(ref newPoints, points[length - 1], points[0], ref remainder, temporarySegment);

                if (newPoints.Count > DivisionNumber(points, segment, closed))
                {
                    newPoints.Remove(newPoints[newPoints.Count - 1]);
                }
            }
            else
            {
                if (newPoints.Count < DivisionNumber(points, segment, closed) + 1)
                {
                    newPoints.Add(points[length - 1]);
                }
            }

            return newPoints;
        }

        public static float ArcLength(List<Vector3> points, bool closed)
        {
            int length = points.Count;
            float arclength = 0.0f;

            for (int i = 1; i < length; i++)
            {
                arclength += Vector3.Distance(points[i - 1], points[i]);
            }

            if (closed)
            {
                arclength += Vector3.Distance(points[length - 1], points[0]);
            }

            return arclength;
        }

        public static float TemporarySegment(List<Vector3> points, float segment, bool closed)
        {
            return ArcLength(points, closed) / DivisionNumber(points, segment, closed);
        }

        private static int DivisionNumber(List<Vector3> points, float segment, bool closed)
        {
            return Mathf.FloorToInt(ArcLength(points, closed) / segment + 0.5f);
        }

        private static void Completion(ref List<Vector3> newPoints, Vector3 start, Vector3 end, ref float remainder, float segment)
        {
            float distance = Vector3.Distance(start, end);
            remainder += distance;

            while (segment < remainder)
            {
                remainder -= segment;
                newPoints.Add(start + (end - start) * (distance - remainder) / distance);
            }
        }

        public static List<Vector3> Shift(List<Vector3> points, int n) // 0 <= n < points.Count
        {
            List<Vector3> newPoints = new List<Vector3>();

            for (int i = n; i < points.Count; i++)
            {
                newPoints.Add(points[i]);
            }

            for (int i = 0; i < n; i++)
            {
                newPoints.Add(points[i]);
            }

            return newPoints;
        }
    }
}
