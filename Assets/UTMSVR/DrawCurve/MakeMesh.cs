using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DrawCurve
{
    using UvTransformer = Func<Vector2, Vector2>;

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
            GetMeshInfo(Curve curve, int meridian, float radius, bool closed,
                        UvTransformer uvTransformer = null)
            {
                List<Vector3> points = curve.GetPoints();
                if (uvTransformer == null)
                    {
                        // 変数名を uv にしたら 下で宣言してるやつと重複してると怒られた
                        uvTransformer = _uv => _uv;
                    }

                List<Vector3> pointsCopy = new List<Vector3>();
                foreach (Vector3 v in points)
                    {
                        pointsCopy.Add(v);
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
                    }

                int length = pointsCopy.Count;
                int numSegments = closed ? points.Count : points.Count - 1;
                List<Vector3> tangents = Tangents(pointsCopy, closed);
                List<Vector3> principalNormals = PrincipalNormals(tangents);

                for (int j = 0; j < length; j++)
                    {
                        Vector3 binormal = Vector3.Cross(tangents[j], principalNormals[j]);

                        for (int i = 0; i <= meridian; i++)
                            {
                                float theta = i * 2 * Mathf.PI / meridian;
                                Vector3 direction = Mathf.Cos(theta) * principalNormals[j] + Mathf.Sin(theta) * binormal;
                                vertices.Add(pointsCopy[j] + radius * direction);
                                normals.Add(direction);
                                float u = ((float)i) / meridian;
                                float v = ((float)j) / numSegments;
                                uv.Add(uvTransformer(new Vector2(u, v)));
                            }
                    }

                triangles = Triangles(length, meridian);

                // mesh.vertices = vertices.ToArray();
                // mesh.triangles = triangles.ToArray();
                // mesh.normals = normals.ToArray();
                // mesh.uv = uv.ToArray();

                return (vertices, normals, uv, triangles);
            }


            public static Mesh GetMeshAtPoints(Curve curve, float radius)
            {
                List<Vector3> points = curve.GetPoints();
                var vertices = new List<Vector3>();
                var triangles = new List<int>();
                var normals = new List<Vector3>();
                foreach (Vector3 point in points)
                    {
                        var meshInfo = GetMeshInfoAtPoint(point, radius);
                        int offset = vertices.Count;
                        vertices.AddRange(meshInfo.vertices);
                        triangles.AddRange(meshInfo.triangles.Select(i => i + offset));
                        normals.AddRange(meshInfo.normals);
                    }

                var mesh = new Mesh();
                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.normals = normals.ToArray();

                return mesh;
            }

            public static Mesh GetMeshAtEndPoint(Curve curve, float radius)
            {
                List<Vector3> points = curve.GetPoints();
                var mesh = new Mesh();

                if (points.Count == 0)
                    {
                        return mesh;
                    }

                var vertices = new List<Vector3>();
                var triangles = new List<int>();
                var normals = new List<Vector3>();

                var meshInfo = GetMeshInfoAtPoint(points[0], radius);
                vertices.AddRange(meshInfo.vertices);
                triangles.AddRange(meshInfo.triangles);
                normals.AddRange(meshInfo.normals);

                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.normals = normals.ToArray();

                return mesh;
            }

            private static (List<Vector3> vertices, List<int> triangles, List<Vector3> normals) GetMeshInfoAtPoint(Vector3 point, float radius)
            {
                float r = radius;

                var vertices = new List<Vector3>
                {
                    point + new Vector3(r, 0, 0),
                    point + new Vector3(0, r, 0),
                    point + new Vector3(-r, 0, 0),
                    point + new Vector3(0, -r, 0),
                    point + new Vector3(0, 0, r),
                    point + new Vector3(0, 0, -r)
                };

                var triangles = new List<int>
                {
                    4, 0, 1,
                    4, 1, 2,
                    4, 2, 3,
                    4, 3, 0,
                    5, 1, 0,
                    5, 2, 1,
                    5, 3, 2,
                    5, 0, 3
                };

                var normals = new List<Vector3>
                {
                    new Vector3(r, 0, 0),
                    new Vector3(0, r, 0),
                    new Vector3(-r, 0, 0),
                    new Vector3(0, -r, 0),
                    new Vector3(0, 0, r),
                    new Vector3(0, 0, -r)
                };

                return (vertices, triangles, normals);
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

            private static List<int> Triangles(int length, int meridian)
            {
                List<int> triangles = new List<int>();
                for (int j = 0; j < length - 1; j++)
                    {
                        for (int i = 0; i < meridian; i++)
                            {
                                triangles.Add(GetIndex(i, j, meridian));
                                triangles.Add(GetIndex(i + 1, j, meridian));
                                triangles.Add(GetIndex(i, j + 1, meridian));

                                triangles.Add(GetIndex(i + 1, j, meridian));
                                triangles.Add(GetIndex(i + 1, j + 1, meridian));
                                triangles.Add(GetIndex(i, j + 1, meridian));
                            }
                    }

                return triangles;
            }

            private static int GetIndex(int i, int j, int meridian)
            {
                return j * (meridian + 1) + i;
            }
        }
    }
}
