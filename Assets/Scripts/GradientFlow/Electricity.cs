// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using DrawCurve;

// // 暗黙の仮定：隣接する2点の間隔は一定、弧長は保存される
// public class Electricity
// {
//     private Curve curve;
//     private List<Vector3> pos;
//     private int len;
//     private float seg;
//     private float lr = 1e-03f;
//     private float alpha = 0.95f;
//     private List<Vector3> gradient;

//     public Electricity(Curve curve)
//     {
//         this.curve = curve;
//         this.pos = curve.positions;
//         this.len = curve.positions.Count;
//         this.seg = curve.segment;
//         this.gradient = Gradient();
//     }

//     public void Flow()
//     {
//         for (int i = 0; i < this.len; i++)
//         {
//             this.curve.positions[i] += this.gradient[i];
//         }
//     }

//     // momentum SGD
//     public void MomentumFlow()
//     {
//         for (int i = 0; i < this.len; i++)
//         {
//             this.curve.momentum[i] = this.alpha * this.curve.momentum[i] + this.gradient[i];
//             this.curve.positions[i] += this.curve.momentum[i];
//         }
//     }

//     public List<Vector3> Gradient()
//     {
//         float arc = ArcLength();
//         List<Vector3> gradient = new List<Vector3>();

//         for (int i = 0; i < this.len; i++)
//         {
//             gradient.Add(new Vector3(0, 0, 0));

//             for (int j = 0; j < this.len; j++)
//             {
//                 if (j != i)
//                 {
//                     gradient[i] += 4 * Mathf.Pow(this.seg, 2) * Coulomb(this.pos[i], this.pos[j]);
//                 }
//             }

//             gradient[i] *= this.lr;
//         }

//         return gradient;
//     }

//     public Vector3[] RestrictedGradient()
//     {
//         Vector3[] gradient = this.gradient.ToArray();
//         Vector3[][] matrix = ONRestrictionMatrix();

//         float[] product = new float[this.len];

//         for (int i = 0; i < this.len; i++)
//         {
//             product[i] = SequentialInnerProduct(matrix[i], gradient);
//         }

//         for (int j = 0; j < this.len; j++)
//         {
//             for (int i = 0; i < this.len; i++)
//             {
//                 gradient[j] -= product[i] * matrix[i][j];
//             }
//         }

//         return gradient;
//     }

//     public float Energy()
//     {
//         float arc = ArcLength();
//         float energy = 0.0f;

//         for (int i = 0; i < this.len; i++)
//         {
//             for (int j = 0; j < this.len; j++)
//             {
//                 if (i != j)
//                 {
//                     energy += Mathf.Pow(this.seg, 2) / (this.pos[i] - this.pos[j]).sqrMagnitude;
//                 }
//             }
//         }

//         energy += - Mathf.Pow(Mathf.PI, 2) * this.len / 3.0f + 4.0f;

//         return energy;
//     }

//     private Vector3 Coulomb(Vector3 v1, Vector3 v2)
//     {
//         return (v1 - v2) / Mathf.Pow((v1 - v2).sqrMagnitude, 2);
//     }

//     private Vector3 Tangent(int i)
//     {
//         return this.pos[(i + 1) % this.len] - this.pos[i];
//     }

//     public float ArcLength()
//     {
//         float arc = 0.0f;

//         for (int i = 0; i < len; i++)
//         {
//             arc += Vector3.Distance(pos[i], pos[(i + 1) % this.len]);
//         }

//         return arc;
//     }

//     private float SequentialNorm(Vector3[] sequence)
//     {
//         float _sum = 0.0f;

//         for (int i = 0; i < this.len - 1; i++)
//         {
//             _sum += sequence[i].sqrMagnitude;
//         }

//         return Mathf.Sqrt(_sum);
//     }

//     private float SequentialInnerProduct(Vector3[] sequence1, Vector3[] sequence2)
//     {
//         float _sum = 0.0f;

//         for (int i = 0; i < this.len - 1; i++)
//         {
//             _sum += Vector3.Dot(sequence1[i], sequence2[i]);
//         }

//         return _sum;
//     }

//     private void SequentialNormalize(Vector3[] sequence)
//     {
//         float norm = SequentialNorm(sequence);

//         for (int i = 0; i < this.len; i++)
//         {
//             sequence[i] = sequence[i] / norm;
//         }
//     }

//     private Vector3[][] RestrictionMatrix()
//     {
//         Vector3[][] _matrix = new Vector3[this.len][];

//         for (int i = 0; i < this.len; i++)
//         {
//             _matrix[i] = new Vector3[this.len];

//             for (int j = 0; j < this.len; j++)
//             {
//                 _matrix[i][j] = Vector3.zero;
//             }
//         }

//         for (int i = 0; i < this.len; i++)
//         {
//             _matrix[i][i] = -Tangent(i);
//             _matrix[i][(i + 1) % this.len] = Tangent(i);
//         }

//         return _matrix;
//     }

//     private Vector3[][] ONRestrictionMatrix()
//     {
//         Vector3[][] _matrix = RestrictionMatrix();

//         SequentialNormalize(_matrix[0]);

//         for (int i = 1; i < this.len - 1; i++)
//         {
//             float product = SequentialInnerProduct(_matrix[i - 1], _matrix[i]);

//             for (int j = 0; j < this.len; j++)
//             {
//                 _matrix[i][j] -= _matrix[i - 1][j] * product;
//             }

//             SequentialNormalize(_matrix[i]);
//         }

//         for (int i = 0; i < this.len - 1; i++)
//         {
//             float product = SequentialInnerProduct(_matrix[i], _matrix[this.len - 1]);

//             for (int j = 0; j < this.len; j++)
//             {
//                 _matrix[this.len - 1][j] -= _matrix[i][j] * product;
//             }
//         }

//         SequentialNormalize(_matrix[this.len - 1]);

//         return _matrix;
//     }
// }