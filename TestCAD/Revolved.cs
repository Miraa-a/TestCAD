using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCAD
{
    class Revolved : BaseModel
    {
        public IList<Vector2> points { get; set; } = new[] {
            new Vector2(0, 0.4f), new Vector2(0.06f, 0.36f),
            new Vector2(0.1f, 0.1f), new Vector2(0.34f, 0.1f),
            new Vector2(0.4f, 0.14f), new Vector2(0.5f, 0.5f),
            new Vector2(0.7f, 0.56f), new Vector2(1, 0.46f) };
        public Vector3 origin { get; set; } = new Vector3(0, 0, 0);
        public Vector3 direction { get; set; } = new Vector3(0, 1, 0);
        
        
        public override void Update()
        {
            direction.Normalize();

            // Find two unit vectors orthogonal to the specified direction

            var u = FindAnyPerpendicular(direction);
            Vector3 tmp = direction;
            var v = Vector3.Cross(tmp, u);
            u.Normalize();
            v.Normalize();
            int thetaDiv = 32;
            var circle = GetCircle(thetaDiv);

            int index0 = Positions.Count;
            int n = points.Count;

            int totalNodes = (points.Count - 1) * 2 * thetaDiv;
            int rowNodes = (points.Count - 1) * 2;

            for (int i = 0; i < thetaDiv; i++)
            {
                var w = (v * circle[i].X) + (u * circle[i].Y);

                for (int j = 0; j + 1 < n; j++)
                {
                    // Add segment
                    var q1 = origin + (direction * points[j].X) + (w * points[j].Y);
                    var q2 = origin + (direction * points[j + 1].X) + (w * points[j + 1].Y);

                    // TODO: should not add segment if q1==q2 (corner point)
                    // const double eps = 1e-6;
                    // if (Point3D.Subtract(q1, q2).LengthSquared < eps)
                    // continue;
                    Positions.Add(q1);
                    Positions.Add(q2);

                    if (Normals != null)
                    {
                        var tx = points[j + 1].X - points[j].X;
                        var ty = points[j + 1].Y - points[j].Y;
                        var normal = (-direction * ty) + (w * tx);
                        normal.Normalize();
                        Normals.Add(normal);
                        Normals.Add(normal);
                    }


                    int i0 = index0 + (i * rowNodes) + (j * 2);
                    int i1 = i0 + 1;
                    int i2 = index0 + ((((i + 1) * rowNodes) + (j * 2)) % totalNodes);
                    int i3 = i2 + 1;

                    Indices.Add(i1);
                    Indices.Add(i0);
                    Indices.Add(i2);

                    Indices.Add(i1);
                    Indices.Add(i2);
                    Indices.Add(i3);
                }
            }
        }
        private Vector3 FindAnyPerpendicular(Vector3 n)
        {
            n.Normalize();
            Vector3 u = Vector3.Cross(new Vector3(0, 1, 0), n);
            if (u.LengthSquared() < 1e-3)
            {
                u = Vector3.Cross(new Vector3(1, 0, 0), n);
            }

            return u;
        }

        private IList<Vector2> GetCircle(int thetaDiv, bool closed = false)
        {
            Dictionary<int, IList<Vector2>> CircleCache = new Dictionary<int, IList<Vector2>>();
            Dictionary<int, IList<Vector2>> ClosedCircleCache = new Dictionary<int, IList<Vector2>>();
            IList<Vector2> circle = null;
            if ((!closed && !CircleCache.TryGetValue(thetaDiv, out circle)) ||
                (closed && !ClosedCircleCache.TryGetValue(thetaDiv, out circle)))
            {
                circle = new List<Vector2>();
                if (!closed)
                {
                    CircleCache.Add(thetaDiv, circle);
                }
                else
                {
                    ClosedCircleCache.Add(thetaDiv, circle);
                }
                var num = closed ? thetaDiv : thetaDiv - 1;
                for (int i = 0; i < thetaDiv; i++)
                {
                    var theta = Math.PI * 2 * ((float)i / num);
                    circle.Add(new Vector2((float)Math.Cos(theta), (float)-Math.Sin(theta)));
                }
            }
            IList<Vector2> result = new List<Vector2>();
            foreach (var point in circle)
            {
                result.Add(new Vector2((float)point.X, (float)point.Y));
            }
            return result;
        }
    }
}
