using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 点列から曲線のメッシュを生成する
public class CurveFunction
{
    public static Mesh Curve(List<Vector3> positions, int meridian, float radius, bool closed)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles;
        List<Vector3> normals = new List<Vector3>();

        if (closed)
        {
            positions.Add(positions[0]);
            positions.Add(positions[1]);
        }

        int length = positions.Count;
        List<Vector3> tangents = Tangents(positions, closed);
        List<Vector3> principalNormals = PrincipalNormals(tangents);

        for (int j = 0; j < length; j++)
        {
            Vector3 binormal = Vector3.Cross(tangents[j], principalNormals[j]);

            for (int i = 0; i <= meridian; i++)
            {
                float theta = i * 2 * Mathf.PI / meridian;
                Vector3 direction = Mathf.Cos(theta) * principalNormals[j] + Mathf.Sin(theta) * binormal;
                vertices.Add(positions[j] + radius * direction);
                normals.Add(direction);
            }
        }

        triangles = Triangles(length, meridian);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        return mesh;
    }

    private static List<Vector3> Tangents (List<Vector3> positions, bool closed)
    {
        List<Vector3> tangents = new List<Vector3>();

        for (int i = 0; i < positions.Count - 1; i++)
        {
            tangents.Add((positions[i + 1] - positions[i]).normalized);
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

    private static List<Vector3> PrincipalNormals (List<Vector3> tangents)
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

    private static Vector3 NaturalNormal (Vector3 v)
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
