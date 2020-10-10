using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface
{
    private int boundaryCount;
    private int divisionNumber;
    private List<Vector3> vertices;
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

    private void LaplacianFairing()
    {

    }

    private void EdgeSwapping()
    {

    }
}
