using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugUtil;

namespace DrawCurve
{
    public class Curve
    {
        public List<Vector3> positions;
        public List<Vector3> momentum;
        public Mesh mesh;
        public Mesh meshAtPositions;
        public bool close;
        public bool selected;
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        private float segment;
        private int meridian = 10;
        private float radius = 0.002f;
        public static float collision = 0.05f;
        public static Controller controller;
        public static ButtonConfig button;

        public Curve(List<Vector3> positions, bool close, float segment = 0.03f)
        {
            this.positions = positions;
            this.close = close;
            this.segment = segment;

            if (positions.Count >= 2)
            {
                this.mesh = MakeMesh.GetMesh(this.positions, meridian, radius, this.close);
            }
        }

        public static void SetUp(Controller argController, ButtonConfig argButton)
        {
            controller = argController;
            button = argButton;
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

        public void MomentumInitialize()
        {
            this.momentum = new List<Vector3>();

            for (int i = 0; i < Length(); i++)
            {
                this.momentum.Add(Vector3.zero);
            }
        }

        private int Length()
        {
            return this.positions.Count;
        }

        public void Draw()
        {
            if (controller.GetButton(button.draw))
            {
                Vector3 nowPosition = controller.rightHand.GetPosition();

                if (Length() == 0)
                {
                    this.positions.Add(nowPosition);
                }
                else if (Vector3.Distance(this.positions.Last(), nowPosition) >= this.segment)
                {
                    this.positions.Add(nowPosition);
                    this.MeshUpdate();
                }
            }
        }

        public void Move()
        {
            Vector3 nowPosition = controller.rightHand.GetPosition();
            Quaternion nowRotation = controller.rightHand.GetRotation();

            if (controller.GetButtonDown(button.move))
            {
                MoveSetUp(nowPosition, nowRotation);
            }

            if (controller.GetButton(button.move))
            {
                MoveUpdate(nowPosition, nowRotation);
            }

            if (controller.GetButtonUp(button.move))
            {
                MoveCleanUp();
            }
        }

        public void MoveSetUp(Vector3 position, Quaternion rotation)
        {
            this.positions = this.positions.Select(v => v - position).Select(v => Quaternion.Inverse(rotation) * v).ToList();
            MeshUpdate();
        }

        public void MoveUpdate(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public void MoveCleanUp()
        {
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
    }
}