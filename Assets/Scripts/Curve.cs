using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve
{
    public bool isSelected;
    public bool isBeingDrawn;
    public bool isBeingMoved;
    public bool isClosed;
    public List<Vector3> positions;
    public Mesh mesh;
    public Vector3 position;
    public Quaternion rotation;
    public List<Vector3> momentum;

    private int meridian = 100;
    private float radius = 0.01f;

    public Curve(
        bool isSelected,
        bool isBeingDrawn,
        bool isBeingMoving,
        bool isClosed,
        List<Vector3> positions,
        Vector3 position,
        Quaternion rotation)
    {
        this.isSelected = isSelected;
        this.isBeingDrawn = isBeingDrawn;
        this.isBeingMoved = isBeingMoving;
        this.isClosed = isClosed;
        this.positions = positions;

        if (positions.Count >= 2)
        {
            this.mesh = MakeMesh.GetMesh(this.positions, this.meridian, this.radius, this.isClosed);
        }

        this.position = position;
        this.rotation = rotation;
    }

    public void MeshUpdate()
    {
        this.mesh = MakeMesh.GetMesh(this.positions, this.meridian, this.radius, this.isClosed);
<<<<<<< HEAD
=======
    }

    public List<Vector3> GetTangents(bool closed)
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
>>>>>>> 99a5501... Add files about energy of knot
    }
}
