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
        List<Vector2> points { get; set; } =
            new() { new(1, 0), new Vector2(1, 2), new Vector2(0, 2), new Vector2(0, 0), };
        //List<Vector2> points { get; set; } = new() { new(0, 0), new Vector2(0, 1), new Vector2(2, 1), new Vector2(2, 0), };
        float Length { get; set; } = 5;
        double Angle { get; set; } = -23;


        public override void Update()
        {
            //Clear
            Positions.Clear();
            Indices.Clear();
            Normals.Clear();
            if (Angle > -24 && Angle < 0)
            {
                Error = true;
                ErrorStr = "Не верный угол, ребра пересекаются";
            }
            points = Check_Mistakes.strException(points);
            var inxs = CuttingEarsTriangulator.Triangulate(points); //триангулировали контур. 
            int inxCount = points.Count;
            bool rev = false;
            Dictionary<int, int> posinx = new Dictionary<int, int>();
            float len;
            if (CuttingEarsTriangulator.Area(points) > 0f)
            {
                rev = true;
                points.Reverse();
            }
            
            len = (float)Math.Sin((Angle * Math.PI) / 180) * Length;//вычислили длину
            
            points.ForEach(p =>
            {
                Positions.Add(p.ToVector3());
                Normals.Add(new Vector3(0, 0, -1));
            });
            if (!rev)
            {
                AddFace(inxs, 0);
            }
            else
            {
                AddFace(inxs, 0, rev);
            }

          
            List<Vector2> direction = new List<Vector2>();
            foreach (var q in FindPerp())
            {
                q.Normalize();
                if (Angle < 0)
                    direction.Add(q / len);
                else
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
            for(int i =0;i<copy.Count;i++)
            {
                copy[i] = copy[i] + directionpoint[i];
            }

             var v = new Vector3(0, 0, Length);
            copy.ForEach(p =>
            { 
                Positions.Add(p.ToVector3()+v);
                Normals.Add(new Vector3(0, 1, 0));
              
            });
            
            if (!rev)
            {
                AddFace(inxs, inxCount, rev);
            }
            else
            {
                AddFace(inxs, inxCount);
            }


            for (int i = 0; i < points.Count - 1; i++)
            {

                int i0 = i;
                int i1 = i + 1;
                int ip0 = i0 + points.Count;
                int ip1 = i1 + points.Count;

                var n = GetNormal(ip0, i0, i1);

                AddFace2(i0, ip0, i1, ip1, n);
            }

            {

                int i0 = points.Count - 1;
                int i1 = 0;
                int ip0 = i0 + points.Count;
                int ip1 = i1 + points.Count;

                var n = GetNormal(ip0, i0, i1);

                AddFace2(i0, ip0, i1, ip1, n);
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

        private void AddFace2(int i0, int ip0, int i1, int ip1, Vector3 n)
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
            AddEdge(face.Edges, a0, a1);
            AddEdge(face.Edges, a1, ap1);
            AddEdge(face.Edges, ap1, ap0);
            AddEdge(face.Edges, ap0, a0);

        }

        private void AddFace(ExposedArrayList<int> inxs, int inxCount, bool isReverse = true)
        {
            var face = new Face();
            for (int i = 0; i < inxs.Count; i += 3)
            {
                int i0 = inxs[i] + inxCount;
                int i1 = inxs[i + 1] + inxCount;
                int i2 = inxs[i + 2] + inxCount;

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
            

            for (int i = 0; i < inxs.Count - 1; i++)
            {
                AddEdge(Edges, inxs[i] + inxCount, inxs[i + 1] + inxCount);
                AddEdge(face.Edges, inxs[i] + inxCount, inxs[i + 1] + inxCount);
            }
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
