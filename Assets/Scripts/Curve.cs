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
        this.position = position;
        this.rotation = rotation;
    }

    public void MeshUpdate()
    {
        mesh = MakeMesh.Curve(this.positions, this.meridian, this.radius, this.isClosed);
    }
}
