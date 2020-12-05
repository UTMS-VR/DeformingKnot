using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinimizeSurface
{
    using Neighbor = Dictionary<int, List<(int, bool)>>;
    
    public class Surface
    {
        private int boundaryCount;
        private int divisionNumber;
        private List<Vector3> vertices;
        private List<Neighbor> neighborhood;
        private List<(int, int, int)> triangles;
        public Mesh mesh1;
        public Mesh mesh2;

        public Surface(List<Vector3> boundary, int divisionNumber)
        {
            this.boundaryCount = boundary.Count;
            this.divisionNumber = divisionNumber;
            this.vertices = boundary;
            this.Initialize(boundary);
            this.mesh1 = new Mesh();
            this.mesh2 = new Mesh();
        }

        private void Initialize(List<Vector3> boundary)
        {
            int n = this.boundaryCount;
            int s = this.divisionNumber;
            Vector3 baryCenter = Barycenter(boundary);
            this.vertices.Add(baryCenter);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < s; j++)
                {
                    this.vertices.Add(boundary[i] + (baryCenter - boundary[i]) * (j + 1) / (s + 1));
                }
            }

            this.InitializeTriangles();
            this.GetNeighborhood();
        }

        private void InitializeTriangles()
        {
            int n = this.boundaryCount;
            int s = this.divisionNumber;
            this.triangles = new List<(int, int, int)>();

            for (int i = 0; i < n; i++)
            {
                this.triangles.Add((i, Succ(i), VertexNum(i, 1)));
                this.triangles.Add((VertexNum(i, 1), Succ(i), VertexNum(Succ(i), 1)));

                for (int j = 1; j < s; j++)
                {
                    this.triangles.Add((VertexNum(i, j), VertexNum(Succ(i), j), VertexNum(i, j + 1)));
                    this.triangles.Add((VertexNum(i, j + 1), VertexNum(Succ(i), j), VertexNum(Succ(i), j + 1)));
                }

                this.triangles.Add((VertexNum(i, s), VertexNum(Succ(i), s), n));
            }
        }

        public void GetNeighborhood()
        {
            int n = this.boundaryCount;
            int s = this.divisionNumber;
            this.neighborhood = new List<Neighbor>();

            for (int i = 0; i < this.vertices.Count; i++)
            {
                this.neighborhood.Add(new Neighbor());
            }

            foreach ((int, int, int) t in this.triangles)
            {
                this.AddNeighbor(t.Item1, t.Item2, t.Item3, true);
                this.AddNeighbor(t.Item1, t.Item3, t.Item2, false);
                this.AddNeighbor(t.Item2, t.Item1, t.Item3, false);
                this.AddNeighbor(t.Item2, t.Item3, t.Item1, true);
                this.AddNeighbor(t.Item3, t.Item1, t.Item2, true);
                this.AddNeighbor(t.Item3, t.Item2, t.Item1, false);
            }
        }

        private void AddNeighbor(int l, int m, int n, bool b)
        {
            if (this.neighborhood[l].ContainsKey(m))
            {
                this.neighborhood[l][m].Add((n, b));
            }
            else
            {
                this.neighborhood[l].Add(m, new List<(int, bool)> { (n, b) });
            }
        }

        private int Succ(int i)
        {
            return (i + 1) % this.boundaryCount;
        }

        private int VertexNum(int i, int j)
        {
            return this.boundaryCount + this.divisionNumber * i + j;
        }

        private Vector3 Barycenter(List<Vector3> points)
        {
            Vector3 center = Vector3.zero;
            for (int i = 0; i < points.Count; i++)
            {
                center += points[i];
            }
            center = center / points.Count;
            return center;
        }

        public void MeshUpdate()
        {
            this.mesh1.vertices = this.vertices.ToArray();
            this.mesh1.triangles = this.ToListInt(this.triangles).ToArray();
            this.mesh1.RecalculateNormals();

            this.mesh2.vertices = this.vertices.ToArray();
            this.mesh2.triangles = this.ToListIntReverse(this.triangles).ToArray();
            this.mesh2.RecalculateNormals();
        }

        private List<int> ToListInt(List<(int, int, int)> triangels)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < triangels.Count; i++)
            {
                list.Add(triangels[i].Item1);
                list.Add(triangels[i].Item2);
                list.Add(triangels[i].Item3);
            }
            return list;
        }

        private List<int> ToListIntReverse(List<(int, int, int)> triangels)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < triangels.Count; i++)
            {
                list.Add(triangles[i].Item1);
                list.Add(triangles[i].Item3);
                list.Add(triangles[i].Item2);
            }
            return list;
        }

        public int TriangleNumber()
        {
            return (2 * this.divisionNumber + 1) * this.boundaryCount;
        }

        public void GetMinimal()
        {
            float preArea = Mathf.Infinity;
            float nowArea = this.SurfaceArea(this.vertices);

            while (Mathf.Abs(preArea - nowArea) > 1e-4f)
            {
                this.LaplacianFairing();
                this.EdgeSwapping();
                preArea = nowArea;
                nowArea = this.SurfaceArea(this.vertices);
            }
        }

        public void AreaMinimizing()
        {
            List<Vector3> newVertices = new List<Vector3>();

            for (int i = 0; i < this.boundaryCount; i++)
            {
                newVertices.Add(this.vertices[i]);
            }

            for (int i = this.boundaryCount; i < this.vertices.Count; i++)
            {
                Vector3 v = new Vector3();
                Matrix3 c = new Matrix3(0.0f);

                foreach (var x in this.neighborhood[i])
                {
                    Vector3 vi = vertices[i];
                    Vector3 vj = vertices[x.Key];
                    Vector3 vji = vi - vj;

                    foreach (var y in x.Value)
                    {
                        Vector3 vk = vertices[y.Item1];
                        Vector3 vjk = vk - vj;
                        float s = 1.0f / Vector3.Cross(vjk, vji).sqrMagnitude;
                        v += (- Vector3.Dot(vjk, vj) * vjk + vjk.sqrMagnitude * vj) * s;

                        Matrix3 a = new Matrix3(- vjk.sqrMagnitude);
                        Matrix3 b = new Matrix3(vjk, vjk);
                        c = Matrix3.Add(c, Matrix3.Add(a, b).Mult(s));
                    }
                }

                newVertices.Add(- c.Inverse().Apply(v));
            }

            if (this.SurfaceArea(this.vertices) > this.SurfaceArea(newVertices))
            {
                this.vertices = newVertices;
            }
        }

        public void LaplacianFairing()
        {
            List<Vector3> newVertices = new List<Vector3>();

            for (int i = 0; i < this.boundaryCount; i++)
            {
                newVertices.Add(this.vertices[i]);
            }

            for (int i = this.boundaryCount; i < this.vertices.Count; i++)
            {
                Vector3 v = new Vector3();
                float sum = 0.0f;
                foreach (var x in this.neighborhood[i])
                {
                    float s = 0.0f;
                    foreach (var y in x.Value)
                    {
                        s += this.TriangleArea(vertices[i], vertices[x.Key], vertices[y.Item1]);
                    }
                    v += s * vertices[x.Key];
                    sum += s;
                }
                newVertices.Add(v / sum);
            }

            if (true) // (this.SurfaceArea(this.vertices) > this.SurfaceArea(newVertices))
            {
                this.vertices = newVertices;
            }
        }

        private float TriangleArea(Vector3 x, Vector3 y, Vector3 z)
        {
            return Vector3.Cross(y - x, z - x).magnitude / 2;
            // return Mathf.Sqrt(Mathf.Pow((y - x).magnitude * (z - x).magnitude, 2) - Mathf.Pow(Vector3.Dot(y - x, z - x), 2)) / 2;
        }

        public float SurfaceArea(List<Vector3> vertexList = null)
        {
            if (vertexList == null)
            {
                vertexList = this.vertices;
            }

            float area = 0.0f;

            foreach ((int, int, int) t in this.triangles)
            {
                area += this.TriangleArea(vertexList[t.Item1], vertexList[t.Item2], vertexList[t.Item3]);
            }

            return area;
        }

        public void EdgeSwapping()
        {
            for (int i = this.boundaryCount; i < this.vertices.Count; i++)
            {
                List<int> neighborhoodKeys = new List<int>();
                foreach (int j in this.neighborhood[i].Keys)
                {
                    neighborhoodKeys.Add(j);
                }

                foreach (int j in neighborhoodKeys)
                {
                    int k0 = this.neighborhood[i][j][0].Item1;
                    int k1 = this.neighborhood[i][j][1].Item1;
                    bool b0 = this.neighborhood[i][j][0].Item2;
                    bool b1 = this.neighborhood[i][j][1].Item2;

                    if (this.ValidSwapping(i, j, k0, k1))
                    {
                        this.UpdateTriangles(i, j, k0, k1, b0, b1);
                        this.UpdateNeighborhood(i, j, k0, k1, b0, b1);
                    }
                }
            }
        }

        private bool ValidSwapping(int i, int j, int k0, int k1)
        {
            float nowArea = this.TriangleArea(this.vertices[i], this.vertices[j], this.vertices[k0])
                            + this.TriangleArea(this.vertices[i], this.vertices[j], this.vertices[k1]);
            float newArea = this.TriangleArea(this.vertices[i], this.vertices[k0], this.vertices[k1])
                            + this.TriangleArea(this.vertices[j], this.vertices[k0], this.vertices[k1]);

            return (nowArea > newArea) && (!this.neighborhood[k0].ContainsKey(k1));
        }

        private void UpdateTriangles(int i, int j, int k0, int k1, bool b0, bool b1)
        {
            this.RemoveTriangle(i, j, k0, b0);
            this.RemoveTriangle(i, j, k1, b1);

            if (b0)
            {
                this.triangles.Add((i, k1, k0));
                this.triangles.Add((j, k0, k1));
            }
            else
            {
                this.triangles.Add((i, k0, k1));
                this.triangles.Add((j, k1, k0));
            }
        }

        private void RemoveTriangle(int i, int j, int k, bool b)
        {
            if (b)
            {
                this.triangles.Remove((i, j, k));
                this.triangles.Remove((j, k, i));
                this.triangles.Remove((k, i, j));
            }
            else
            {
                this.triangles.Remove((j, i, k));
                this.triangles.Remove((i, k, j));
                this.triangles.Remove((k, j, i));
            }
        }

        private void UpdateNeighborhood(int i, int j, int k0, int k1, bool b0, bool b1)
        {
            this.neighborhood[i].Remove(j);
            this.neighborhood[j].Remove(i);
            this.neighborhood[i][k0].Remove((j, !b0));
            this.neighborhood[i][k1].Remove((j, !b1));
            this.neighborhood[j][k0].Remove((i, b0));
            this.neighborhood[j][k1].Remove((i, b1));
            this.neighborhood[k0][i].Remove((j, b0));
            this.neighborhood[k0][j].Remove((i, !b0));
            this.neighborhood[k1][i].Remove((j, b1));
            this.neighborhood[k1][j].Remove((i, !b1));

            this.neighborhood[k0].Add(k1, new List<(int, bool)> { (i, !b0), (j, b0) });
            this.neighborhood[k1].Add(k0, new List<(int, bool)> { (i, b0), (j, !b0) });
            this.neighborhood[i][k0].Add((k1, !b0));
            this.neighborhood[i][k1].Add((k0, !b1));
            this.neighborhood[j][k0].Add((k1, b0));
            this.neighborhood[j][k1].Add((k0, b1));
            this.neighborhood[k0][i].Add((k1, b0));
            this.neighborhood[k0][j].Add((k1, !b0));
            this.neighborhood[k1][i].Add((k0, b1));
            this.neighborhood[k1][j].Add((k0, !b1));
        }
    }
}