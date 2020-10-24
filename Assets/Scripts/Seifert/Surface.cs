using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinimizeSurface
{
    using Neighbor = List<(int, List<(int, bool)>)>;
    
    public class Surface
    {
        private int boundaryCount;
        private int divisionNumber;
        private List<Vector3> vertices;
        private List<Neighbor> neighborhood;
        private List<(int, int, int)> triangles;
        public Mesh mesh;

        public Surface(List<Vector3> boundary, int divisionNumber)
        {
            this.boundaryCount = boundary.Count;
            this.divisionNumber = divisionNumber;
            this.vertices = boundary;
            this.Initialize(boundary);
            this.mesh = new Mesh();
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
            int index = this.neighborhood[l].FindIndex(x => (x.Item1 == m));
            if (index < 0) this.neighborhood[l].Add((m, new List<(int, bool)> { (n, b) }));
            else this.neighborhood[l][index].Item2.Add((n, b));
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
            this.mesh.vertices = this.vertices.ToArray();
            this.mesh.triangles = this.ToListInt(this.triangles).ToArray();
            this.mesh.RecalculateNormals();
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

        public int TriangleNumber()
        {
            return (2 * this.divisionNumber + 1) * this.boundaryCount;
        }

        public void GetMinimal()
        {

        }

        private void AreaMinimizing()
        {

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
                foreach ((int, List<(int, bool)>) x in this.neighborhood[i])
                {
                    float s = this.TriangleArea(vertices[i], vertices[x.Item1], vertices[x.Item2[0].Item1])
                              + this.TriangleArea(vertices[i], vertices[x.Item1], vertices[x.Item2[1].Item1]);
                    v += s * vertices[x.Item1];
                    sum += s;
                }
                newVertices.Add(v / sum);
            }

            if (this.SurfaceArea(this.vertices) > this.SurfaceArea(newVertices))
            {
                this.vertices = newVertices;
            }
        }

        private float TriangleArea(Vector3 x, Vector3 y, Vector3 z)
        {
            return Mathf.Sqrt(Mathf.Pow((y - x).magnitude * (z - x).magnitude, 2) - Mathf.Pow(Vector3.Dot(y - x, z - x), 2)) / 2;
        }

        private float SurfaceArea(List<Vector3> vertexList)
        {
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
                int neighborNum = this.neighborhood[i].Count;
                for (int n = neighborNum - 1; n >= 0; n--)
                {
                    (int, List<(int, bool)>) x = this.neighborhood[i][n];
                    int j = x.Item1;
                    int k0 = x.Item2[0].Item1;
                    int k1 = x.Item2[1].Item1;
                    bool b0 = x.Item2[0].Item2;
                    bool b1 = x.Item2[1].Item2;

                    float nowArea = this.TriangleArea(this.vertices[i], this.vertices[j], this.vertices[k0])
                                    + this.TriangleArea(this.vertices[i], this.vertices[j], this.vertices[k1]);
                    float newArea = this.TriangleArea(this.vertices[i], this.vertices[k0], this.vertices[k1])
                                    + this.TriangleArea(this.vertices[j], this.vertices[k0], this.vertices[k1]);

                    if (nowArea > newArea)
                    {
                        this.UpdateTriangles(i, j, k0, k1, b0, b1);
                        this.UpdateNeighborhood(i, j, k0, k1, b0, b1);
                    }
                }
            }
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
            this.neighborhood[i].RemoveAt(FindNeighborIndex(i, j));
            this.neighborhood[j].RemoveAt(FindNeighborIndex(j, i));
            this.neighborhood[i][FindNeighborIndex(i, k0)].Item2.Remove((j, !b0));
            this.neighborhood[i][FindNeighborIndex(i, k1)].Item2.Remove((j, !b1));
            this.neighborhood[j][FindNeighborIndex(j, k0)].Item2.Remove((i, b0));
            this.neighborhood[j][FindNeighborIndex(j, k1)].Item2.Remove((i, b1));
            this.neighborhood[k0][FindNeighborIndex(k0, i)].Item2.Remove((j, b0));
            this.neighborhood[k0][FindNeighborIndex(k0, j)].Item2.Remove((i, !b0));
            this.neighborhood[k1][FindNeighborIndex(k1, i)].Item2.Remove((j, b1));
            this.neighborhood[k1][FindNeighborIndex(k1, j)].Item2.Remove((i, !b1));

            this.neighborhood[k0].Add((k1, new List<(int, bool)> { (i, !b0), (j, b0) }));
            this.neighborhood[k1].Add((k0, new List<(int, bool)> { (i, b0), (j, !b0) }));
            this.neighborhood[i][FindNeighborIndex(i, k0)].Item2.Add((k1, !b0));
            this.neighborhood[i][FindNeighborIndex(i, k1)].Item2.Add((k0, !b1));
            this.neighborhood[j][FindNeighborIndex(j, k0)].Item2.Add((k1, b0));
            this.neighborhood[j][FindNeighborIndex(j, k1)].Item2.Add((k0, b1));
            this.neighborhood[k0][FindNeighborIndex(k0, i)].Item2.Add((k1, b0));
            this.neighborhood[k0][FindNeighborIndex(k0, j)].Item2.Add((k1, !b0));
            this.neighborhood[k1][FindNeighborIndex(k1, i)].Item2.Add((k0, b1));
            this.neighborhood[k1][FindNeighborIndex(k1, j)].Item2.Add((k0, !b1));
        }

        private int FindNeighborIndex(int i, int j)
        {
            return this.neighborhood[i].FindIndex(x => x.Item1 == j);
        }

        public (int, int) Valid()
        {
            int b0 = 0;
            int b1 = 0;
            int b2 = 0;

            for (int i = this.boundaryCount; i < this.vertices.Count; i++)
            {
                foreach ((int, List<(int, bool)>) x in this.neighborhood[i])
                {
                    if (x.Item2.Count != 2) b0++;
                    else if (x.Item2[0].Item2 == x.Item2[1].Item2) b1++;
                    b2++;
                }
            }

            return (b0, b1);
        }

        public void DebugLog()
        {
            for (int i = 0; i < this.neighborhood.Count; i++)
            {
                string s = i.ToString();
                foreach ((int, List<(int, bool)>) x in this.neighborhood[i])
                {
                    s += ", (" + x.Item1.ToString();
                    for (int j = 0; j < x.Item2.Count; j++) s += ", " + x.Item2[j].ToString();
                    s += ")";
                }
                Debug.Log(s);
            }
        }
    }
}