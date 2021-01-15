using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;

namespace TestCAD.Models
{

    public class Extrusion_with_angle : BaseModel
    {
        public List<Vector2> points { get; set; } =
            new() { new(1, 0), new Vector2(1, 2), new Vector2(0, 2), new Vector2(0, 0), };
        
        public float Length { get; set; } = -8;
        public double Angle { get; set; } = 0;

        public override void Update()
        {
            //Clear
            Positions.Clear();
            Indices.Clear();
            Normals.Clear();
            string message = "";
            points = Check_Mistakes.strException(points,ref message);
            ErrorStr = message;
            if (ErrorStr == "")
            {
                var inxs = CuttingEarsTriangulator.Triangulate(points); //триангулировали контур. 
                int inxCount = points.Count;
                bool rev = false;
                float len;
                if (CuttingEarsTriangulator.Area(points) > 0f)
                {
                    rev = true;
                    Angle = Angle * (-1);
                    //points.Reverse();
                }

                List<Vector2> edg = new List<Vector2>();
                len = (float) Math.Tan((Angle * Math.PI) / 180) * Length; //вычислили длину
                points.ForEach(p =>
                {
                    Positions.Add(p.ToVector3());
                    Normals.Add(new Vector3(0, 0, -1));
                });
                AddFace(inxCount, inxs, 0, edg, rev);



                List<Vector2> direction = new List<Vector2>();
                foreach (var q in FindPerp())
                {
                    q.Normalize();
                    direction.Add(q * len);
                }

                List<Vector2> directionpoint = new List<Vector2>();
                for (int i = 0; i < direction.Count; i++)
                {
                    if (i == 0)
                        directionpoint.Add(direction[i] + direction[direction.Count - 1]);
                    else
                        directionpoint.Add(direction[i] + direction[i - 1]);
                }

                List<Vector2> copy = new List<Vector2>();
                copy = points;
                for (int i = 0; i < copy.Count; i++)
                {
                    copy[i] = copy[i] + new Vector2(directionpoint[i].X, directionpoint[i].Y);

                }
                Check_Mistakes.strException(copy, ref message);
                //ErrorStr = Check_Mistakes.Cross_(copy);
                ErrorStr = message;
                if (ErrorStr == "")
                {
                    
                    //ErrorStr = Check_Mistakes.CheckAngel(copy, Angle);

                    var v = new Vector3(0, 0, Length);
                    
                    copy.ForEach(p =>
                    {
                        Positions.Add(p.ToVector3() + v);
                        Normals.Add(new Vector3(0, 0, 1));
                    });
                    AddFace(inxCount, inxs, inxCount, edg, rev);

                    int sign = rev ? -1 : 1;
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        int i0 = i;
                        int i1 = i + 1;
                        int ip0 = i0 + points.Count;
                        int ip1 = i1 + points.Count;
                        var n = GetNormal(ip0, i0, i1) * sign;
                        AddFace2(i0, ip0, i1, ip1, n, ref edg);
                    }

                    {
                        int i0 = points.Count - 1;
                        int i1 = 0;
                        int ip0 = i0 + points.Count;
                        int ip1 = i1 + points.Count;
                        var n = GetNormal(ip0, i0, i1) * sign ;
                        AddFace2(i0, ip0, i1, ip1, n, ref edg);
                    }
                    ErrorStr = Check_Mistakes.CheckAngel(edg);
                    
                }
            }
        }
        private Vector3 GetNormal(int ip0, int i0, int i1)
        {
            var v1 = Positions[ip0] - Positions[i0];
            var v2 = Positions[i1] - Positions[i0];
            var n = Vector3.Cross(v1, v2);
            n.Normalize();
            return n;
        }
        private void AddFace2(int i0, int ip0, int i1, int ip1, Vector3 n, ref List<Vector2>edg)
        {
            var face = new Face();
            int startInx = Positions.Count;
            Positions.Add(Positions[i0]);
            Normals.Add(n);
            int a0 = startInx;
            Indices.Add(a0);
            face.Indices.Add(a0);
            Positions.Add(Positions[ip0]);
            Normals.Add(n);
            int ap0 = startInx + 1;
            Indices.Add(ap0);
            face.Indices.Add(ap0);
            Positions.Add(Positions[i1]);
            Normals.Add(n);
            int a1 = startInx + 2;
            Indices.Add(a1);
            face.Indices.Add(a1);
            Indices.Add(a1);
            face.Indices.Add(a1);
            Indices.Add(ap0);
            face.Indices.Add(ap0);
            Positions.Add(Positions[ip1]);
            Normals.Add(n);
            int ap1 = startInx + 3;
            Indices.Add(ap1);
            face.Indices.Add(ap1);
            Faces.Add(face);
            AddEdge(Edges, a0, ap0);
            edg.Add(new Vector2(Positions[a0].X, Positions[a0].Y));
            edg.Add(new Vector2(Positions[ap0].X, Positions[ap0].Y));
            AddEdge(face.Edges, a0, a1);
            AddEdge(face.Edges, a1, ap1);
            AddEdge(face.Edges, ap1, ap0);
            AddEdge(face.Edges, ap0, a0);
            
        }

        private void AddFace(int conturCount, ExposedArrayList<int> inxs, int startPosInx,List<Vector2>edg, bool isReverse = false)
        {
            var face = new Face();
            for (int i = 0; i < inxs.Count; i += 3)
            {
                int i0 = inxs[i] + startPosInx;
                int i1 = inxs[i + 1] + startPosInx;
                int i2 = inxs[i + 2] + startPosInx;

                if (isReverse)
                {
                    int tmp = i1;
                    i1 = i2;
                    i2 = tmp;
                }

                Indices.Add(i0);
                Indices.Add(i1);
                Indices.Add(i2);

                face.Indices.Add(i0);
                face.Indices.Add(i1);
                face.Indices.Add(i2);
            }

            Faces.Add(face);
            for (int i = 0; i < conturCount - 1; i++)
            {
                AddEdge(Edges, i + startPosInx, i + 1 + startPosInx);
                edg.Add(new Vector2(Positions[i + startPosInx].X, Positions[i + startPosInx].Y));
                edg.Add(new Vector2(Positions[i + 1 + startPosInx].X, Positions[i + 1 + startPosInx].Y));
                AddEdge(face.Edges, i + startPosInx, i + 1 + startPosInx);
            }

            AddEdge(Edges, conturCount - 1 + startPosInx, 0 + startPosInx);
            edg.Add(new Vector2(Positions[conturCount - 1 + startPosInx].X, Positions[conturCount - 1 + startPosInx].Y));
            edg.Add(new Vector2(Positions[0 + startPosInx].X, Positions[0 + startPosInx].Y));
            AddEdge(face.Edges, conturCount + startPosInx, 0 + startPosInx);

        }
        static void AddEdge(List<Edge> edges, int i0, int i1)
        {
            var edge = new Edge();
            edge.Indices.Add(i0);
            edge.Indices.Add(i1);
            edges.Add(edge);
        }
        List<Vector2> FindPerp()//нашли перпендикуляр
        {
            
            List<Vector2> result = new List<Vector2>();
            List<Vector3> tmp = new List<Vector3>();
            for (int i = 0; i < points.Count+1; i++)
            {
                if (i == points.Count)
                {
                    tmp.Add(new Vector3(points[0].X, points[0].Y, 1));
                }
                else
                { 
                    tmp.Add(new Vector3(points[i].X , points[i].Y, 1));
                }
                
            }

            for (int i = 0; i < tmp.Count-1; i++)
            {
                Vector3 r = Vector3.Cross(tmp[i], tmp[i + 1]);
                result.Add(new Vector2(r.X,r.Y));
            }
            return result;
        }
    }
}
