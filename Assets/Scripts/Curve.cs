using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DrawCurve;
using DebugUtil;

public class Curve
{
    public List<Vector3> positions;
    public List<Vector3> momentum;
    public Mesh mesh;
    public Mesh meshAtPositions;
    public bool close;
    public bool selected = false;
    public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;
    public float segment = 0.03f;
    public static Controller controller;
    public static float collision = 0.05f;
    private static int meridian = 10;
    private static float radius = 0.002f;

    public Curve(List<Vector3> positions, bool close)
    {
        this.positions = positions;
        this.close = close;

        if (positions.Count >= 2)
        {
            this.mesh = MakeMesh.GetMesh(this.positions, meridian, radius, this.close);
        }
    }

    public void MeshUpdate()
    {
        this.mesh = MakeMesh.GetMesh(this.positions, meridian, radius, this.close);
    }

    public void MeshAtPositionsUpdate()
    {
        this.meshAtPositions = MakeMesh.GetMeshAtPositions(this.positions, radius * 2.0f);
    }

    public void MeshAtEndPositionUpdate()
    {
        this.meshAtPositions = MakeMesh.GetMeshAtEndPosition(this.positions, radius * 2.0f);
    }

    public static (int, float) Distance(List<Vector3> positions, Vector3 position)
    {
        List<Vector3> relPositions = positions.Select(v => v - position).ToList();

        int num = 0;
        float min = relPositions[0].magnitude;

        for (int i = 0; i < positions.Count - 1; i++)
        {
            if (relPositions[i + 1].magnitude < min)
            {
                num = i + 1;
                min = relPositions[i + 1].magnitude;
            }
        }

        return (num, min);
    }

    public void Draw()
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();
        
        if (this.positions.Count == 0)
        {
            this.positions.Add(nowPosition);
        }
        else if (Vector3.Distance(this.positions.Last(), nowPosition) >= this.segment)
        {
            this.positions.Add(nowPosition);
            this.MeshUpdate();
        }
    }

    public void MoveSetUp()
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();
        Quaternion nowRotation = controller.rightHand.GetRotation();
        this.positions = this.positions.Select(v => v - nowPosition).Select(v => Quaternion.Inverse(nowRotation) * v).ToList();
        MeshUpdate();
    }

    public void Move()
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();
        Quaternion nowRotation = controller.rightHand.GetRotation();
        this.position = nowPosition;
        this.rotation = nowRotation;
    }

    public void MoveCleanUp()
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();
        Quaternion nowRotation = controller.rightHand.GetRotation();
        this.positions = this.positions.Select(v => this.rotation * v).Select(v => v + this.position).ToList();
        MeshUpdate();
        this.position = Vector3.zero;
        this.rotation = Quaternion.identity;
    }

    public void Select()
    {
        Vector3 nowPosition = controller.rightHand.GetPosition();

        if (Distance(this.positions, nowPosition).Item2 < collision)
        {
            this.selected = !this.selected;
        }
    }

    public void Close()
    {
        if (Vector3.Distance(this.positions.First(), this.positions.Last()) < collision)
        {
            this.close = !this.close;
            MeshUpdate();
        }
    }

    public List<Curve> Cut()
    {
        List<Curve> newCurves = new List<Curve>();
        Vector3 nowPosition = controller.rightHand.GetPosition();
        (int, float) distance = Distance(this.positions, nowPosition);
        int num = distance.Item1;

        if (distance.Item2 < collision)
        {
            if (this.close)
            {
                newCurves.Add(CutKnot(num));
            }
            else if (2 <= num && num <= Length() - 3)
            {
                (Curve, Curve) curves = CutCurve(num);
                newCurves.Add(curves.Item1);
                newCurves.Add(curves.Item2);
            }
        }

        return newCurves;
    }

    private Curve CutKnot(int num)
    {
        List<Vector3> newPositions = new List<Vector3>();

        for (int i = num + 1; i < Length(); i++)
        {
            newPositions.Add(this.positions[i]);
        }

        for (int i = 0; i < num; i++)
        {
            newPositions.Add(this.positions[i]);
        }

        Curve cutKnot = new Curve(newPositions, false);

        return cutKnot;
    }

    private (Curve, Curve) CutCurve(int num)
    {
        List<Vector3> newPositions1 = new List<Vector3>();
        List<Vector3> newPositions2 = new List<Vector3>();

        for (int i = 0; i < num; i++)
        {
            newPositions1.Add(this.positions[i]);
        }

        for (int i = num + 1; i < Length(); i++)
        {
            newPositions2.Add(this.positions[i]);
        }

        Curve newCurve1 = new Curve(newPositions1, false);
        Curve newCurve2 = new Curve(newPositions2, false);

        return (newCurve1, newCurve2);
    }

    public static List<Curve> Combine(Curve curve1, Curve curve2)
    {
        List<Curve> newCurves = new List<Curve>();
        List<Vector3> positions1 = curve1.positions;
        List<Vector3> positions2 = curve2.positions;
        AdjustOrientation(ref positions1, ref positions2);

        if (Vector3.Distance(positions1.Last(), positions2.First()) < collision)
        {
            foreach (Vector3 v in positions2)
            {
                positions1.Add(v);
            }

            bool close = Vector3.Distance(positions1.First(), positions2.Last()) < collision ? true : false;
            newCurves.Add(new Curve(positions1, close));
        }

        return newCurves;
    }

    private static void AdjustOrientation(ref List<Vector3> positions1, ref List<Vector3> positions2)
    {
        if (Vector3.Distance(positions1.Last(), positions2.Last()) < collision)
        {
            positions2.Reverse();
        }
        else if (Vector3.Distance(positions1.First(), positions2.First()) < collision)
        {
            positions1.Reverse();
        }
        else if (Vector3.Distance(positions1.First(), positions2.Last()) < collision)
        {
            positions1.Reverse();
            positions2.Reverse();
        }
    }

    private int Length()
    {
        return this.positions.Count;
    }
}
