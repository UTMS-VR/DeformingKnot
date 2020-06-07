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
    public Mesh meshAtPositions;
    public Vector3 position;
    public Quaternion rotation;
    public List<Vector3> momentum;

    public float segment = 0.1f;
    private int meridian = 10;
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
    }

    public void MeshAtPositionsUpdate()
    {
        this.meshAtPositions = MakeMesh.GetMeshAtPositions(this.positions, this.radius * 5.0f);
    }

    public List<Vector3> GetTangents()
    {
        List<Vector3> tangents = new List<Vector3>();

        for (int i = 0; i < positions.Count - 1; i++)
        {
            tangents.Add((positions[i + 1] - positions[i]).normalized);
        }

        if (this.isClosed)
        {
            tangents.Add(tangents[1]);
        }
        else
        {
            tangents.Add(tangents[tangents.Count - 1]);
        }

        return tangents;
    }

    public void ParameterExchange()
    {
        int length = this.positions.Count;

        List<Vector3> newPositions = new List<Vector3>();
        newPositions.Add(this.positions[0]);

        Vector3 endPosition = newPositions[0];

        for (int i = 1; i < length; i++)
        {
            Completion(ref newPositions, positions[i], false);
        }

        Completion(ref newPositions, positions[0], true);

        this.positions = newPositions;
    }

    private void Completion(ref List<Vector3> newPositions, Vector3 position, bool end)
    {
        Vector3 endPosition = newPositions[newPositions.Count - 1];
        float distance = Vector3.Distance(endPosition, position);

        if (distance > this.segment * 1.1f)
        {
            for (int i = 1; this.segment * i < distance; i++)
            {
                newPositions.Add(endPosition + (position - endPosition) * this.segment * i / distance);
            }
        }

        if (Vector3.Distance(newPositions[newPositions.Count - 1], position) > this.segment * 0.9f && !end)
        {
            newPositions.Add(position);
        }
    }
}
