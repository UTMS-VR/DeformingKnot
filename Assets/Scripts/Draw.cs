using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

public class Draw : MonoBehaviour
{
    List<Vector3> positions;
    Curve curve;
    List<Vector3> vertices;
    Mesh mesh;
    int meridian = 10;
    float radius = 1.0f;
    List<Curve> edges;
    Material black;

    // Start is called before the first frame update
    void Start()
    {
        black = Resources.Load<Material>("MyPackage/DrawCurve/black");

        positions = new List<Vector3>();
        positions.Add(new Vector3(-2, 0, 10));
        positions.Add(new Vector3(-1, 0, 10));
        positions.Add(new Vector3(0, 0.3f, 10));
        positions.Add(new Vector3(1, 0.3f, 10));
        positions.Add(new Vector3(2, 0, 10));
        positions.Add(new Vector3(3, -0.3f, 10));
        positions.Add(new Vector3(4, -0.3f, 10));
        curve = new Curve(positions, false, meridian: this.meridian, radius: this.radius);
        curve.mesh.normals = new Vector3[0];
        vertices = this.GetVertices(positions, this.meridian, this.radius, false);
        mesh = MakeMesh.GetMeshAtPositions(vertices, 0.05f);

        List<int> triangles = Triangles(7, this.meridian);
        edges = new List<Curve>();
        int n = triangles.Count / 3;
        for (int i = 0; i < n; i++)
        {
            List<Vector3> pos = new List<Vector3>();
            pos.Add(vertices[triangles[3 * i]]);
            pos.Add(vertices[triangles[3 * i + 1]]);
            edges.Add(new Curve(pos, false, meridian: 4, radius: 0.01f));

            pos = new List<Vector3>();
            pos.Add(vertices[triangles[3 * i + 1]]);
            pos.Add(vertices[triangles[3 * i + 2]]);
            edges.Add(new Curve(pos, false, meridian: 4, radius: 0.01f));

            pos = new List<Vector3>();
            pos.Add(vertices[triangles[3 * i + 2]]);
            pos.Add(vertices[triangles[3 * i]]);
            edges.Add(new Curve(pos, false, meridian: 4, radius: 0.01f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        Graphics.DrawMesh(curve.mesh, Vector3.zero, Quaternion.identity, MakeMesh.CurveMaterial, 0);
        Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, MakeMesh.PositionMaterial, 0);

        foreach (Curve edge in edges)
        {
            Graphics.DrawMesh(edge.mesh, Vector3.zero, Quaternion.identity, black, 0);
        }
    }

    public List<Vector3> GetVertices(List<Vector3> positions, int meridian, float radius, bool closed)
    {
        List<Vector3> vertices = new List<Vector3>();
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
            }
        }

        return vertices;
    }

    public List<Vector3> Tangents(List<Vector3> positions, bool closed)
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

    private List<Vector3> PrincipalNormals(List<Vector3> tangents)
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

    private Vector3 NaturalNormal(Vector3 v)
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
