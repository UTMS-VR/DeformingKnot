using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace DrawCurve {
    public class VisiblePoints {
        private List<Vector3> points;
        private Mesh mesh = new Mesh();
        private float radius;

        public VisiblePoints(List<Vector3> points, float radius = Curve.defaultRadius * 2.0f) {
            this.points = points;
            this.radius = radius;
        }

        public void SetPoints(List<Vector3> points) {
            this.points = points;
        }

        public Mesh GetMesh() {
            if (this.mesh.vertices.Length == 0) {
                this.UpdateMesh();
            }
            return this.mesh;
        }

        public void UpdateMesh() {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            foreach (Vector3 point in this.points) {
                var meshInfo = GetMeshInfoAtPoint(point, this.radius);
                int offset = vertices.Count;
                vertices.AddRange(meshInfo.vertices);
                triangles.AddRange(meshInfo.triangles.Select(i => i + offset));
                normals.AddRange(meshInfo.normals);
            }

            var mesh = new Mesh();
            this.mesh.vertices = vertices.ToArray();
            this.mesh.triangles = triangles.ToArray();
            this.mesh.normals = normals.ToArray();
        }

        private static (List<Vector3> vertices, List<int> triangles, List<Vector3> normals) GetMeshInfoAtPoint(Vector3 point, float radius) {
            float r = radius;

            var vertices = new List<Vector3>
            {
                point + new Vector3(r, 0, 0),
                point + new Vector3(0, r, 0),
                point + new Vector3(-r, 0, 0),
                point + new Vector3(0, -r, 0),
                point + new Vector3(0, 0, r),
                point + new Vector3(0, 0, -r)
            };

            var triangles = new List<int>
            {
                4, 0, 1,
                4, 1, 2,
                4, 2, 3,
                4, 3, 0,
                5, 1, 0,
                5, 2, 1,
                5, 3, 2,
                5, 0, 3
            };

            var normals = new List<Vector3>
            {
                new Vector3(r, 0, 0),
                new Vector3(0, r, 0),
                new Vector3(-r, 0, 0),
                new Vector3(0, -r, 0),
                new Vector3(0, 0, r),
                new Vector3(0, 0, -r)
            };

            return (vertices, triangles, normals);
        }
    }
}
