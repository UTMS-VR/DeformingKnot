using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DebugUtil;
using DrawCurve;

interface IKnotState
{
    IKnotState Update();
    List<Vector3> GetPoints();
}

class KnotData
{
    public List<Vector3> points;
    public (int first, int second) chosenPoints;
    public readonly Controller controller;
    public readonly int meridian;
    public readonly float radius;
    public readonly float distanceThreshold;
    public readonly List<Curve> collisionCurves;
    public readonly OVRInput.RawButton selectButton;
    public readonly OVRInput.RawButton cancelButton;
    public readonly OVRInput.RawButton optimizeButton;

    public KnotData(
        List<Vector3> points,
        (int first, int second) chosenPoints,
        Controller controller,
        float radius,
        int meridian,
        float distanceThreshold,
        List<Curve> collisionCurves,
        OVRInput.RawButton selectButton,
        OVRInput.RawButton cancelButton,
        OVRInput.RawButton optimizeButton
        )
    {
        this.points = points;
        this.chosenPoints = chosenPoints;
        this.controller = controller;
        this.meridian = meridian;
        this.radius = radius;
        this.distanceThreshold = distanceThreshold;
        this.collisionCurves = collisionCurves;
        this.selectButton = selectButton;
        this.cancelButton = cancelButton;
        this.optimizeButton = optimizeButton;
    }
}



class KnotStateBase : IKnotState
{
    private KnotData data;
    private Mesh knotMesh;

    public KnotStateBase(KnotData data)
    {
        this.data = data;
        this.knotMesh = MakeMesh.GetMesh(this.data.points, this.data.meridian, this.data.radius, true);
    }

    public IKnotState Update()
    {
        Graphics.DrawMesh(this.knotMesh, Vector3.zero, Quaternion.identity, MakeMesh.SelectedCurveMaterial, 0);

        if (this.data.controller.GetButtonDown(this.data.selectButton))
        {
            return new KnotStatePull(this.data);
        }
        else if (this.data.controller.GetButtonDown(this.data.cancelButton))
        {
            return new KnotStateChoose1(this.data);
        }
        else if (this.data.controller.GetButtonDown(this.data.optimizeButton))
        {
            return new KnotStateOptimize(this.data);
        }

        return null;
    }

    public List<Vector3> GetPoints()
    {
        return this.data.points;
    }
}

class KnotStatePull : IKnotState
{
    private KnotData data;
    private List<Curve> collisionCurves;
    private PullableCurve pullableCurve;

    public KnotStatePull(KnotData data)
    {
        this.data = data;
        this.collisionCurves = this.data.collisionCurves;
        int count = this.data.points.Count;
        this.pullableCurve = new PullableCurve(this.data.points, this.data.controller.rightHand, closed: true,
            meridian: this.data.meridian, radius: this.data.radius, distanceThreshold: this.data.distanceThreshold,
            weights: PullableCurve.GetWeights(count, this.data.chosenPoints.first, this.data.chosenPoints.second));
    }

    public IKnotState Update()
    {
        // List<Vector3> collisionPoints = this.collisionPoints;
        // List<Vector3> collisionPoints = this.GetCompliment(this.chosenPoints.first, this.chosenPoints.second);
        // collisionPoints = collisionPoints.Concat(this.collisionPoints).ToList();
        this.pullableCurve.Update(this.collisionCurves);
        Mesh knotMesh = this.pullableCurve.GetMesh();
        Mesh pointsMesh = MakeMesh.GetMeshAtPositions(this.pullableCurve.GetPoints(), this.data.radius * 3); 
        Graphics.DrawMesh(knotMesh, Vector3.zero, Quaternion.identity, MakeMesh.SelectedCurveMaterial, 0);
        Graphics.DrawMesh(pointsMesh, Vector3.zero, Quaternion.identity, MakeMesh.PositionMaterial, 0);
        // this.pointMesh = MakeMesh.GetMeshAtPositions(collisionPoints, this.radius * 2);

        if (this.data.controller.GetButtonDown(this.data.selectButton))
        {
            List<Vector3> newPoints = this.pullableCurve.GetPoints();
            this.data.points = newPoints;
            this.data.chosenPoints = this.pullableCurve.ChosenPoints();
            return new KnotStateBase(this.data);
        }
        else if (this.data.controller.GetButton(this.data.cancelButton))
        {
            return new KnotStateBase(this.data);
        }

        return null;
    }

    public List<Vector3> GetPoints()
    {
        return this.pullableCurve.GetPoints();
    }

    private List<Vector3> GetCompliment(int start, int end)
    {
        int numPoints = this.data.points.Count;
        int margin = 2;
        if (start <= end)
        {
            List<Vector3> range1 = this.data.points.GetRange(end + margin, numPoints - end - margin);
            List<Vector3> range2 = this.data.points.GetRange(0, start - margin);
            return range1.Concat(range2).ToList();
        }
        else
        {
            return this.data.points.GetRange(end + margin, start - end - margin);
        }
    }
}

class KnotStateChoose1 : IKnotState
{
    private KnotData data;
    private Mesh knotMesh;

    public KnotStateChoose1(KnotData data)
    {
        this.data = data;
        this.knotMesh = MakeMesh.GetMesh(this.data.points, this.data.meridian, this.data.radius, true);
    }

    public IKnotState Update()
    {
        int ind1 = KnotStateChoose1.FindClosestPoint(this.data.controller, this.data.points);
        var positions = new List<Vector3>() { this.data.points[ind1] };
        Mesh pointMesh = MakeMesh.GetMeshAtPositions(positions, this.data.radius * 5);

        Graphics.DrawMesh(this.knotMesh, Vector3.zero, Quaternion.identity, MakeMesh.SelectedCurveMaterial, 0);
        Graphics.DrawMesh(pointMesh, Vector3.zero, Quaternion.identity, MakeMesh.PositionMaterial, 0);

        if (this.data.controller.GetButtonDown(this.data.selectButton))
        {
            return new KnotStateChoose2(this.data, ind1);
        }
        else if (this.data.controller.GetButtonDown(this.data.cancelButton))
        {
            return new KnotStateBase(this.data);
        }

        return null;
    }

    public List<Vector3> GetPoints()
    {
        return this.data.points;
    }

    public static int FindClosestPoint(Controller controller, List<Vector3> points)
    {
        // KnotStateChoose2 からも呼び出せるように static メソッドにした
        Vector3 controllerPosition = controller.rightHand.GetPosition();
        int closestIndex = 0;
        float closestDistance = Vector3.Distance(points[closestIndex], controllerPosition);
        for (int i = 1; i < points.Count; i++)
        {
            float distance = Vector3.Distance(points[i], controllerPosition);
            if (distance < closestDistance)
            {
                closestIndex = i;
                closestDistance = distance;
            }
        }
        return closestIndex;
    }
}

class KnotStateChoose2 : IKnotState
{
    private KnotData data;
    private Mesh knotMesh;
    private int ind1;

    public KnotStateChoose2(KnotData data, int ind1)
    {
        this.data = data;
        this.knotMesh = MakeMesh.GetMesh(this.data.points, this.data.meridian, this.data.radius, true);
        this.ind1 = ind1;
    }

    public IKnotState Update()
    {
        int ind2 = KnotStateChoose1.FindClosestPoint(this.data.controller, this.data.points);
        var positions = new List<Vector3>() {
                    this.data.points[this.ind1],
                    this.data.points[ind2]
                };
        Mesh pointMesh = MakeMesh.GetMeshAtPositions(positions, this.data.radius * 5);

        Graphics.DrawMesh(this.knotMesh, Vector3.zero, Quaternion.identity, MakeMesh.SelectedCurveMaterial, 0);
        Graphics.DrawMesh(pointMesh, Vector3.zero, Quaternion.identity, MakeMesh.PositionMaterial, 0);

        if (this.data.controller.GetButtonDown(this.data.selectButton))
        {
            this.data.chosenPoints = KnotStateChoose2.ChooseShorterPath(
                (this.ind1, ind2), this.data.points.Count);
            // this.chosenPoints = this.currentPoints;  // ←そのままの順序で選ぶ場合
            return new KnotStateBase(this.data);
        }
        else if (this.data.controller.GetButtonDown(this.data.cancelButton))
        {
            return new KnotStateBase(this.data);
        }

        return null;
    }

    public List<Vector3> GetPoints()
    {
        return this.data.points;
    }

    private static (int first, int second) ChooseShorterPath((int first, int second) points, int numPoints)
    {
        int smaller = Mathf.Min(points.first, points.second);
        int larger = Mathf.Max(points.first, points.second);
        if (2 * (larger - smaller) <= numPoints)
        {
            return (smaller, larger);
        }
        else
        {
            return (larger, smaller);
        }
    }
}

class KnotStateOptimize : IKnotState
{
    private KnotData data;
    private List<Vector3> newPoints;
    private List<Vector3> momentum;

    public KnotStateOptimize(KnotData data)
    {
        this.data = data;
        this.newPoints = data.points;
        AdjustParameter.Equalize(ref this.newPoints, this.data.distanceThreshold, true);
        this.momentum = new List<Vector3>();

        for (int i = 0; i < newPoints.Count; i++)
        {
            this.momentum.Add(Vector3.zero);
        }
    }

    public IKnotState Update()
    {
        if (this.data.controller.GetButton(this.data.optimizeButton)) this.Optimize();
        Mesh knotMesh = MakeMesh.GetMesh(this.newPoints, this.data.meridian, this.data.radius, true);
        Graphics.DrawMesh(knotMesh, Vector3.zero, Quaternion.identity, MakeMesh.SelectedCurveMaterial, 0);

        if (this.data.controller.GetButtonDown(this.data.selectButton))
        {
            this.data.points = this.newPoints;
            return new KnotStateBase(this.data);
        }
        else if (this.data.controller.GetButtonDown(this.data.cancelButton))
        {
            return new KnotStateBase(this.data);
        }

        return null;
    }

    public List<Vector3> GetPoints()
    {
        return this.newPoints;
    }

    public void Optimize()
    {
        /*DiscreteMoebius optimizer1 = new DiscreteMoebius(this.newPoints, this.momentum);

        for (int i = 0; i < this.newPoints.Count; i++)
        {
            this.newPoints[i] -= this.momentum[i] + optimizer1.gradient[i];
        }

        List<Vector3> tempPositions = new List<Vector3>();

        for (int i = 0; i < this.newPoints.Count; i++)
        {
            tempPositions.Add(this.newPoints[i]);
        }

        while (true)
        {
            Elasticity optimizer2 = new Elasticity(this.newPoints, this.momentum, this.data.distanceThreshold);
            if (optimizer2.MaxError() < this.data.distanceThreshold * 0.1f) break;
            optimizer2.Flow();
        }

        for (int i = 0; i < this.newPoints.Count; i++)
        {
            this.momentum[i] = (this.momentum[i] + optimizer1.gradient[i]) * 0.95f
                                + (tempPositions[i] - this.newPoints[i]) * 0.3f;
        }*/

        DiscreteMoebius optimizer1 = new DiscreteMoebius(this.newPoints, this.momentum);
        optimizer1.MomentumFlow();

        while (true)
        {
            Elasticity optimizer2 = new Elasticity(this.newPoints, this.momentum, this.data.distanceThreshold);
            if (optimizer2.MaxError() < this.data.distanceThreshold * 0.1f) break;
            optimizer2.Flow();
        }
    }
}
