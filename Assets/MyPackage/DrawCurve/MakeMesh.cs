using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DrawCurve
{
    public static class MakeMesh
    {
        public static Material CurveMaterial = Resources.Load<Material>("MyPackage/DrawCurve/Curve");
        public static Material PointMaterial = Resources.Load<Material>("MyPackage/DrawCurve/Point");
        public static Material SelectedCurveMaterial = Resources.Load<Material>("MyPackage/DrawCurve/SelectedCurve");

        public static Mesh GetMesh(List<Vector3> points, int meridian, float radius, bool closed)
        {
            List<Vector3> pointsCopy = new List<Vector3>();
            foreach (Vector3 v in points)
            {
                pointsCopy.Add(v);
            }

            Mesh mesh = new Mesh();

            if (pointsCopy.Count < 2)
            {
                return mesh;
            }

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles;
            List<Vector3> normals = new List<Vector3>();

            if (closed)
            {
                pointsCopy.Add(pointsCopy[0]);
                pointsCopy.Add(pointsCopy[1]);
            }

            int length = pointsCopy.Count;
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
                }
            }

            triangles = Triangles(length, meridian);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();

            return mesh;
        }


        public static Mesh GetMeshAtPoints(List<Vector3> points, float radius)
        {
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

        public static Mesh GetMeshAtEndPoint(List<Vector3> points, float radius)
        {
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

        public static List<Vector3> Tangents(List<Vector3> points, bool closed)
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

        public static List<Vector3> PrincipalNormals(List<Vector3> tangents)
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