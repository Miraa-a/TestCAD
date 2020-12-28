using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input.Manipulations;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX.Core;

namespace TestCAD
{
    /// <summary>
    /// Класс операции выдавливания.
    /// Содержит в себе набор точек, длину на которую выдавливать и переопределенный метод для его построения.
    /// </summary>
    class Extrusion : BaseModel
    {
        List<Vector2> points { get; set; } = new() {new Vector2(0, 0),  new Vector2(0, 2), new Vector2(1, 2), new(1, 0), };
        float Length { get; set; } = 5;
        

        public override void Update()
        {
            //Clear
            Positions.Clear();
            Indices.Clear();
            Normals.Clear();
            points = Check_Mistakes.strException(points);
            var inxs = CuttingEarsTriangulator.Triangulate(points);//триангулировали контур. 
            int inxCount = points.Count;
            var face = new Face();
            var edge = new Edge();
            Dictionary<int, int> posinx = new Dictionary<int, int>();
            
            points.ForEach(p =>
            {
                Positions.Add(p.ToVector3());
                Normals.Add(new Vector3(0, 0, -1));
            });
            inxs.ForEach(i => Indices.Add(i));
            inxs.ForEach(i => face.Indices.Add(i));
            for (int i =0; i<inxs.Count-1;i++)
            {
                AddEdge(inxs[i],inxs[i+1]);
                AddLine(edge, inxs[i], inxs[i + 1]);
            }
          

            var v = new Vector3(0, 0, Length);
            points.ForEach(p =>
            {
                Positions.Add(p.ToVector3() + v);
                Normals.Add(new Vector3(0, 0, 1));
            });
            inxs.ForEach(i => Indices.Add(i + inxCount));
            inxs.ForEach(i => face.Indices.Add(i + inxCount));
            for (int i = 0; i < inxs.Count - 1; i++)
            {
                AddEdge(inxs[i] + inxCount, inxs[i + 1] + inxCount);
                AddLine(edge, inxs[i] + inxCount, inxs[i + 1] + inxCount);
            }

            List<int> pos = new List<int>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                int i0 = i;
                int i1 = i + 1;
                int ip0 = i0 + points.Count;
                int ip1 = i1 + points.Count;

                pos.Add((i0));
                pos.Add((i1));
                pos.Add((ip0));
                pos.Add((ip1));
                
                var v1 = Positions[ip0] - Positions[i0];
                var v2 = Positions[i1] - Positions[i0];
                var n = Vector3.Cross(v1, v2);
                n.Normalize();
                
                int startInx = Positions.Count;
                
                posinx.Add(i0, startInx);
                posinx.Add(ip0, startInx + 1);
                posinx.Add(i1, startInx + 2);                
                posinx.Add(ip1, startInx + 3);
                
                AddPosition(pos, n);
                
                CreateTriangls(i0, ip0, i1, n, posinx, edge,face);
                CreateTriangls(i1, ip0, ip1, n, posinx, edge,face);
                
                posinx.Clear();
                pos.Clear();
            }

            {
                int i0 = points.Count - 1;
                int i1 = 0;
                int ip0 = i0 + points.Count;
                int ip1 = i1 + points.Count;
                
                pos.Add((i0));
                pos.Add((i1));
                pos.Add((ip0));
                pos.Add((ip1));
               
                var v1 = Positions[ip0] - Positions[i0];
                var v2 = Positions[i1] - Positions[i0];
                var n = Vector3.Cross(v1, v2);
                n.Normalize();
               
                int startInx = Positions.Count; 
               
                posinx.Add(i0, startInx);
                posinx.Add(ip0, startInx + 1);
                posinx.Add(i1, startInx + 2);
                posinx.Add(ip1, startInx + 3);
                
                AddPosition(pos, n);
                 
                CreateTriangls(i0, ip0, i1, n, posinx, edge, face);
                CreateTriangls(i1, ip0, ip1, n, posinx, edge, face);
                
                posinx.Clear();
                pos.Clear();
            }
            Faces.Add(face);
            face.Edges.Add(edge);
        }
        void AddPosition(List<int> pos, Vector3 n)
        {
            
            foreach (var v in pos)
            {
                Positions.Add(Positions[v]);
               
                Normals.Add(n);
            }
            
        }
        void AddEdge(int i0, int i1)
        {
            var edge = new Edge();
            edge.Indices.Add(i0);
            edge.Indices.Add(i1);
            Edges.Add(edge);
        }
        static void AddLine(Edge edge, int i0, int i1)
        {
            edge.Indices.Add(i0);
            edge.Indices.Add(i1);
        }

         void CreateTriangls(int a, int b, int c, Vector3 n, Dictionary<int,int>pos, Edge edge, Face face)
         {
             int i0 = 0;
             
             int i2 = 0;
            foreach (var v in pos)
            {
                if (a == v.Key)
                {
                    Indices.Add(v.Value);
                    i0 = v.Value;
                }
                else if (b == v.Key)
                {
                    Indices.Add(v.Value);
                    
                }
                else if (c == v.Key)
                {
                    Indices.Add(v.Value);
                    i2 = v.Value;
                }
            }
            face.Indices.Add(a);
            face.Indices.Add(b);
            face.Indices.Add(c);
            
            AddEdge(i2, i0);
            AddLine(edge, i2, i0);
        }

        
    }
}
