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
        //public List<Vector3> momentum;
        public Mesh mesh;
        public Mesh meshAtPositions;
        public bool close;
        public bool selected;
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public float segment;
        public int meridian;
        public float radius;
        public static float collision = 0.05f;
        public static Controller controller;
        public static OVRInput.RawButton drawButton;
        public static OVRInput.RawButton moveButton;

        public Curve(List<Vector3> positions, bool close, bool selected = false, float segment = 0.03f, int meridian = 10, float radius = 0.002f)
        {
            this.positions = positions;
            //this.momentum = new List<Vector3>();
            this.close = close;
            this.selected = selected;
            this.segment = segment;
            this.meridian = meridian;
            this.radius = radius;
            this.mesh = MakeMesh.GetMesh(this.positions, this.meridian, this.radius, this.close);
        }

        public static void SetUp(Controller controller, OVRInput.RawButton drawButton, OVRInput.RawButton moveButton)
        {
            Curve.controller = controller;
            Curve.drawButton = drawButton;
            Curve.moveButton = moveButton;
        }

        public void MeshUpdate()
        {
            this.mesh = MakeMesh.GetMesh(this.positions, this.meridian, this.radius, this.close);
        }

        public void MeshAtPositionsUpdate()
        {
            this.meshAtPositions = MakeMesh.GetMeshAtPositions(this.positions, this.radius * 2.0f);
        }

        public void MeshAtEndPositionUpdate()
        {
            this.meshAtPositions = MakeMesh.GetMeshAtEndPosition(this.positions, this.radius * 2.0f);
        }

        /*public void MomentumInitialize()
        {
            this.momentum = new List<Vector3>();

            for (int i = 0; i < Length(); i++)
            {
                this.momentum.Add(Vector3.zero);
            }
        }*/

        private int Length()
        {
            return this.positions.Count;
        }

        public float ArcLength()
        {
            float arclength = 0.0f;

            for (int i = 1; i < Length(); i++)
            {
                arclength += Vector3.Distance(positions[i - 1], positions[i]);
            }

            if (this.close)
            {
                arclength += Vector3.Distance(positions[Length() - 1], positions[0]);
            }

            return arclength;
        }

        public void ScaleTranslation()
        {
            for (int i = 0; i < Length(); i++)
            {
                this.positions[i] *= this.segment * Length() / ArcLength();
            }
        }

        public void Draw()
        {
            if (controller.GetButton(drawButton))
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

            if (controller.GetButtonDown(moveButton))
            {
                MoveSetUp(nowPosition, nowRotation);
            }

            if (controller.GetButton(moveButton))
            {
                MoveUpdate(nowPosition, nowRotation);
            }

            if (controller.GetButtonUp(moveButton))
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

        /*public void Optimize()
        {
            DiscreteMoebius optimizer = new DiscreteMoebius(this.positions, this.momentum);
            
            for (int i = 0; i < this.Length(); i++)
            {
                this.positions[i] -= this.momentum[i] + optimizer.gradient[i];
            }

            List<Vector3> tempPositions = new List<Vector3>();

            for (int i = 0; i < this.Length(); i++)
            {
                tempPositions.Add(this.positions[i]);
            }

            while (true)
            {
                Elasticity optimizer2 = new Elasticity(this.positions, this.momentum, this.segment);
                if (optimizer2.MaxError() < this.segment * 0.1f) break;
                optimizer2.Flow();
            }

            for (int i = 0; i < this.Length(); i++)
            {
                this.momentum[i] = (this.momentum[i] + optimizer.gradient[i]) * 0.95f
                                    + (tempPositions[i] - this.positions[i]) * 0.3f;
            }

            DiscreteMoebius optimizer = new DiscreteMoebius(this);
            //Electricity optimizer = new Electricity(this);
            optimizer.MomentumFlow();
            //this.ScaleTranslation();

            while (true)
            {
                Elasticity optimizer2 = new Elasticity(this);
                if (optimizer2.MaxError() < this.segment * 0.1f) break;
                optimizer2.MomentumFlow();
            }

            this.MeshUpdate();
            this.MeshAtPositionsUpdate();
        }*/

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

            Curve cutKnot = new Curve(newPositions, false, selected: true);

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

            Curve newCurve1 = new Curve(newPositions1, false, selected: true);
            Curve newCurve2 = new Curve(newPositions2, false, selected: true);

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
                newCurves.Add(new Curve(positions1, close, selected: true));
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

        public float MinSegmentDist()
        {
            List<Vector3> seq = this.positions;
            int n = Length();
            float min = SegmentDist.SSDist(seq[0], seq[1], seq[2], seq[3]);
            int endi = this.close ? n - 3 : n - 4;

            for (int i = 0; i <= endi; i++)
            {
                int endj = (i == 0 && !this.close) ? n - 2 : n - 1;
                for (int j = i + 2; j <= endj; j++)
                {
                    float dist = SegmentDist.SSDist(seq[i], seq[i + 1], seq[j], seq[(j + 1) % n]);
                    if (dist < min) min = dist;
                }
            }

            return min;
        }

        public Curve DeepCopy()
        {
            List<Vector3> positions = ListVector3Copy(this.positions);
            Curve curve = new Curve(positions, this.close, segment: this.segment);
            //curve.momentum = ListVector3Copy(this.momentum);
            curve.mesh = this.mesh;
            curve.meshAtPositions = this.meshAtPositions;
            curve.close = this.close;
            curve.selected = this.selected;
            curve.position = Vector3Copy(this.position);
            curve.rotation = QuaternionCopy(this.rotation);
            curve.meridian = this.meridian;
            curve.radius = this.radius;
            return curve;
        }

        private Vector3 Vector3Copy(Vector3 v)
        {
            Vector3 w = new Vector3(v.x, v.y, v.z);
            return w;
        }

        private Quaternion QuaternionCopy(Quaternion v)
        {
            Quaternion w = new Quaternion(v.x, v.y, v.z, v.w);
            return w;
        }

        private List<Vector3> ListVector3Copy(List<Vector3> l)
        {
            List<Vector3> m = new List<Vector3>();
            
            foreach (Vector3 v in l)
            {
                m.Add(Vector3Copy(v));
            }

            return m;
        }

        public float MinCos()
        {
            float max = 1.0f;

            for (int i = 0; i < Length(); i++)
            {
                Vector3 next = this.positions[(i + 1) % Length()] - this.positions[i];
                Vector3 previous = this.positions[i] - this.positions[(i + Length() - 1) % Length()];
                float cos = Vector3.Dot(next.normalized, previous.normalized);
                if (cos < max) max = cos;
            }

            return max;
        }
    }
}