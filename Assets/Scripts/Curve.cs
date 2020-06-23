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

        List<Vector3> _positions = new List<Vector3>();
        _positions.Add(this.positions[0]);

        for (int i = 1; i < length; i++)
        {
            Completion(ref _positions, this.positions[i - 1], this.positions[i]);
        }

        if (this.isClosed)
        {
            Completion(ref _positions, this.positions[length - 1], this.positions[0]);

            if (_positions.Count > this.DivisionNumber())
            {
                Debug.Log("remove");
                _positions.Remove(_positions[_positions.Count - 1]);
            }
        }
        else
        {
            if (_positions.Count < this.DivisionNumber() + 1)
            {
                _positions.Add(this.positions[length - 1]);
            }
        }

        this.positions = _positions;
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

    public int DivisionNumber()
    {
        return Mathf.FloorToInt(this.ArcLength() / this.segment + 0.5f);
    }

    private void Completion(ref List<Vector3> newPositions, Vector3 start, Vector3 end)
    {
        Vector3 last = newPositions[newPositions.Count - 1];
        float remainder = Vector3.Distance(last, start);
        float distance = Vector3.Distance(start, end);
        float temporarySegment = this.ArcLength() / this.DivisionNumber();

        for (int i = 1; temporarySegment * i - remainder < distance; i++)
        {
            newPositions.Add(start + (end - start) * (temporarySegment * i - remainder) / distance);
        }
    }

    public void ParameterShift()
    {
        Vector3 endPosition = this.positions[0];
        this.positions.Remove(endPosition);
        this.positions.Add(endPosition);
    }
}
