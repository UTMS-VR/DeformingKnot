using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 点列から曲線のメッシュを生成する
public class CurveFunction
{
    public Mesh Curve(List<Vector3> positions, int meridian, float radius, bool closed)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles;
        List<Vector3> normals = new List<Vector3>();

        if (closed)
        {
            positions.Add(positions[0]);
        }

        int length = positions.Count;
        List<Vector3> tangents = Tangents(positions, closed);
        List<Vector3> principalNormals = PrincipalNormals(tangents);

        for (int j = 0; j < length; j++)
        {
            AddVertexNormal(positions[j], tangents[j], principalNormals[j], meridian, radius, vertices, normals);
        }

        triangles = Triangles(length, meridian);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        return mesh;
    }

    private List<Vector3> Tangents (List<Vector3> positions, bool closed)
    {
        List<Vector3> tangents = new List<Vector3>();

        for (int i = 0; i < positions.Count - 1; i++)
        {
            tangents.Add((positions[i + 1] - positions[i]).normalized);
        }

        if (closed)
        {
            tangents.Add(tangents[0]);
        }
        else
        {
            tangents.Add(tangents[tangents.Count - 1]);
        }

        return tangents;
    }

    private List<Vector3> PrincipalNormals (List<Vector3> tangents)
    {
        int length = tangents.Count;
        List<Vector3> principalNormals = new List<Vector3>();
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

    private Vector3 NaturalNormal (Vector3 v)
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

    private void AddVertexNormal(Vector3 position, Vector3 tangent, Vector3 principalNormal, int meridian, float radius, List<Vector3> vertices, List<Vector3> normals)
    {
        Vector3 binormal = Vector3.Cross(tangent, principalNormal);

        for (int i = 0; i <= meridian; i++)
        {
            float theta = i * 2 * Mathf.PI / meridian;
            Vector3 direction = Mathf.Cos(theta) * principalNormal + Mathf.Sin(theta) * binormal;
            vertices.Add(position + radius * direction);
            normals.Add(direction);
        }
    }

    private List<int> Triangles(int length, int meridian)
    {
        List<int> triangles = new List<int>();
        for (int j = 0; j <= length - 2; j++)
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

    int GetIndex(int i, int j, int meridian)
    {
        return j * (meridian + 1) + i;
    }
}
