using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace DrawCurve
{
    public partial class Curve
    {
        public static Material CurveMaterial = Resources.Load<Material>("UTMSVR/DrawCurve/Curve");
        public static Material RainbowCurveMaterial = Resources.Load<Material>("UTMSVR/DrawCurve/Rainbow");
        public static Material RainbowCurveMaterial2 = Resources.Load<Material>("UTMSVR/DrawCurve/Rainbow2");
        public static Material PointMaterial = Resources.Load<Material>("UTMSVR/DrawCurve/Point");
        public static Material SelectedCurveMaterial = Resources.Load<Material>("UTMSVR/DrawCurve/SelectedCurve");

        private static class MakeMesh
        {
            public static (List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv, List<int> triangles)
            GetMeshInfo(Curve curve, int meridianCount, float radius, bool closed)
            {
                List<Vector3> points = curve.GetPoints();
                List<float> vCoordinates = curve.GetVCoordinates();

                List<Vector3> pointsCopy = new List<Vector3>();
                List<float> vCoordinatesCopy = new List<float>();

                foreach (Vector3 point in points)
                    {
                        pointsCopy.Add(point);
                    }
                foreach (float v in vCoordinates)
                    {
                        vCoordinatesCopy.Add(v);
                    }

                // Mesh mesh = new Mesh();

                if (pointsCopy.Count < 2)
                    {
                        return (new List<Vector3>(), new List<Vector3>(), new List<Vector2>(), new List<int>());
                    }

                List<Vector3> vertices = new List<Vector3>();
                List<int> triangles;
                List<Vector3> normals = new List<Vector3>();
                List<Vector2> uv = new List<Vector2>();

                if (closed)
                    {
                        pointsCopy.Add(pointsCopy[0]);
                        pointsCopy.Add(pointsCopy[1]);

                        float vLast = vCoordinates.Last();
                        float vBeforeLast = vCoordinates[vCoordinates.Count - 2];
                        float vDiff = vLast - vBeforeLast;
                        vCoordinatesCopy.Add(vLast + vDiff);
                        vCoordinatesCopy.Add(vLast + 2 * vDiff);
                    }

                int length = pointsCopy.Count;
                List<Vector3> tangents = Tangents(pointsCopy, closed);
                List<Vector3> principalNormals = PrincipalNormals(tangents);

                for (int j = 0; j < length; j++)
                    {
                        Vector3 binormal = Vector3.Cross(tangents[j], principalNormals[j]);

                        for (int i = 0; i <= meridianCount; i++)
                            {
                                float theta = i * 2 * Mathf.PI / meridianCount;
                                Vector3 direction = Mathf.Cos(theta) * principalNormals[j] + Mathf.Sin(theta) * binormal;
                                vertices.Add(pointsCopy[j] + radius * direction);
                                normals.Add(direction);
                                float u = ((float)i) / meridianCount;
                                float v = vCoordinatesCopy[j];
                                uv.Add(new Vector2(u, v));
                            }
                    }

                triangles = Triangles(length, meridianCount);

                // mesh.vertices = vertices.ToArray();
                // mesh.triangles = triangles.ToArray();
                // mesh.normals = normals.ToArray();
                // mesh.uv = uv.ToArray();

                return (vertices, normals, uv, triangles);
            }

            private static List<Vector3> Tangents(List<Vector3> points, bool closed)
            {
                List<Vector3> tangents = new List<Vector3>();

                for (int i = 0; i < points.Count - 1; i++)
                    {
                        tangents.Add((points[i + 1] - points[i]).normalized);
                    }

                if (closed)
                    {
                        tangents.Add(tangents[1]);
                    }
                else
                    {
                        tangents.Add(tangents[tangents.Count - 1]);
                    }

                return tangents;
            }

            private static List<Vector3> PrincipalNormals(List<Vector3> tangents)
            {
                int length = tangents.Count;
                List<Vector3> principalNormals = new List<Vector3>();

                if (length == 0)
                    {
                        return principalNormals;
                    }

                principalNormals.Add(NaturalNormal(tangents[0]));

                for (int i = 0; i < length - 1; i++)
                    {
                        Vector3 v = Vector3.ProjectOnPlane(principalNormals[i], tangents[i + 1]).normalized;
                        if (v.magnitude < 0.1f)
                            {
                                v = NaturalNormal(tangents[i + 1]);
                            }
                        principalNormals.Add(v);
                    }

                return principalNormals;
            }

            private static Vector3 NaturalNormal(Vector3 v)
            {
                Vector3 w = new Vector3();

                if (v.x < -0.001f || v.x > 0.001f)
                    {
                        w = Vector3.ProjectOnPlane(Vector3.forward, w).normalized;
                    }
                else
                    {
                        w = new Vector3(v.x * v.x - 1, v.x * v.y, v.x * v.z).normalized;
                    }

                return w;
            }

            private static List<int> Triangles(int length, int meridianCount)
            {
                List<int> triangles = new List<int>();
                for (int j = 0; j < length - 1; j++)
                    {
                        for (int i = 0; i < meridianCount; i++)
                            {
                                triangles.Add(GetIndex(i, j, meridianCount));
                                triangles.Add(GetIndex(i + 1, j, meridianCount));
                                triangles.Add(GetIndex(i, j + 1, meridianCount));

                                triangles.Add(GetIndex(i + 1, j, meridianCount));
                                triangles.Add(GetIndex(i + 1, j + 1, meridianCount));
                                triangles.Add(GetIndex(i, j + 1, meridianCount));
                            }
                    }

                return triangles;
            }

            private static int GetIndex(int i, int j, int meridianCount)
            {
                return j * (meridianCount + 1) + i;
            }
        }
    }
}
