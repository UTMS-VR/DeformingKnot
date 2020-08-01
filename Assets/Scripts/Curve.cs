using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;

public class Curve
{
    public bool isSelected;
    public bool isBeingDrawn;
    public bool isBeingMoved;
    public bool isClosed;
    public List<Vector3> positions;
    public List<Vector3> momentum;
    public Mesh mesh;
    public Mesh meshAtPositions;
    public Vector3 position;
    public Quaternion rotation;

    public float segment = 0.01f;
    private int meridian = 10;
    private float radius = 0.002f;

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
        this.meshAtPositions = MakeMesh.GetMeshAtPositions(this.positions, this.radius * 2.0f);
    }

    public void MeshAtEndPositionUpdate()
    {
        this.meshAtPositions = MakeMesh.GetMeshAtEndPosition(this.positions, this.radius * 2.0f);
    }
}
