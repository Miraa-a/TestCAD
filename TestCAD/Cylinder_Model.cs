using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCAD
{

    class Cylinder_Model : BaseModel
    {
        public float Radius { get; set; } = 1;

        public override void Update()
        {
            Vector3 point1 = new Vector3(0, 0, 0);//origin
            Vector3 point2 = new Vector3(0, 2, 0);
            Vector3 n = point2 - point1;//direction
            var l = Math.Sqrt(n.X * n.X + n.Y * n.Y + n.Z * n.Z);
            n.Normalize();
            int thetaDiv = 32;//
            var pc = new List<Vector2>();//points
            pc.Add(new Vector2(0, 0));
            pc.Add(new Vector2(0, Radius));
            pc.Add(new Vector2((float)l, Radius));
            pc.Add(new Vector2((float)l, 0));
            n.Normalize();
            Vector3 u = Vector3.Cross(new Vector3(0, 1, 0), n);
            if (u.LengthSquared() < 1e-3)
            {
                u = Vector3.Cross(new Vector3(1, 0, 0), n);
            }
            var v = Vector3.Cross(n, u);
            u.Normalize();
            v.Normalize();
            var circle = AddCircle(thetaDiv);
            int index0 = Positions.Count;
            int counter = pc.Count;
            int totalNodes = (pc.Count - 1) * 2 * thetaDiv;
            int rowNodes = (pc.Count - 1) * 2;
            for (int i = 0; i < thetaDiv; i++)
            {
                var w = (v * circle[i].X) + (u * circle[i].Y);

                for (int j = 0; j + 1 < counter; j++)
                {
                    var q1 = point1 + (n * pc[j].X) + (w * pc[j].Y);
                    var q2 = point1 + (n * pc[j + 1].X) + (w * pc[j + 1].Y);
                    Positions.Add(new Vector3((float)q1.X, (float)q1.Y, (float)q1.Z));
                    Positions.Add(new Vector3((float)q2.X, (float)q2.Y, (float)q2.Z));

                    if (Normals != null)
                    {
                        var tx = pc[j + 1].X - pc[j].X;
                        var ty = pc[j + 1].Y - pc[j].Y;
                        var normal = (-n * ty) + (w * tx);
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
        private IList<Vector2> AddCircle(int thetaDiv, bool closed = false)
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
