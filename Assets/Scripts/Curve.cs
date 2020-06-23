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

    public List<Vector3> GetPositions()
    {
        return this.positions;
    }

    public void UpdatePositions(List<Vector3> NewPositions)
    {
        this.positions = NewPositions;
    }

    public void MeshUpdate()
    {
        this.mesh = MakeMesh.GetMesh(this.positions, this.meridian, this.radius, this.isClosed);
    }

    public void MeshAtPositionsUpdate()
    {
        this.meshAtPositions = MakeMesh.GetMeshAtPositions(this.positions, this.radius * 5.0f);
    }

    public void MeshAtEndPositionUpdate()
    {
        this.meshAtPositions = MakeMesh.GetMeshAtEndPosition(this.positions, this.radius * 5.0f);
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
        List<Vector3> newpositions = new List<Vector3>();
        newpositions.Add(this.positions[0]);
        float remainder = 0.0f;

        for (int i = 1; i < length; i++)
        {
            Completion(ref newpositions, this.positions[i - 1], this.positions[i], ref remainder);
        }

        if (this.isClosed)
        {
            Completion(ref newpositions, this.positions[length - 1], this.positions[0], ref remainder);

            if (newpositions.Count > this.DivisionNumber())
            {
                newpositions.Remove(newpositions[newpositions.Count - 1]);
            }
        }
        else
        {
            if (newpositions.Count < this.DivisionNumber() + 1)
            {
                newpositions.Add(this.positions[length - 1]);
            }
        }

        this.positions = newpositions;
    }

    public float ArcLength()
    {
        int length = this.positions.Count;
        float arclength = 0.0f;

        for (int i = 1; i < length; i++)
        {
            arclength += Vector3.Distance(this.positions[i - 1], this.positions[i]);
        }

        if (this.isClosed)
        {
            arclength += Vector3.Distance(this.positions[length - 1], this.positions[0]);
        }

        return arclength;
    }

    private int DivisionNumber()
    {
        return Mathf.FloorToInt(this.ArcLength() / this.segment + 0.5f);
    }

    private void Completion(ref List<Vector3> newPositions, Vector3 start, Vector3 end, ref float remainder)
    {
        float temporarySegment = this.ArcLength() / this.DivisionNumber();
        float distance = Vector3.Distance(start, end);
        remainder += distance;

        while (temporarySegment < remainder)
        {
            remainder -= temporarySegment;
            newPositions.Add(start + (end - start) * (distance - remainder) / distance);
        }
    }

    public void ParameterShift(int n) // n < this.positions.Count
    {
        List<Vector3> newpositions = new List<Vector3>();

        for (int i = n; i < this.positions.Count; i++)
        {
            newpositions.Add(this.positions[i]);
        }

        for (int i = 0; i < n; i++)
        {
            newpositions.Add(this.positions[i]);
        }

        this.positions = newpositions;
    }
}
