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
    public class Extrusion : BaseModel
    {
        //List<Vector2> points { get; set; } = new() { new Vector2(0, 0), new Vector2(0, 2), new Vector2(1, 2), new(1, 0), };
         //List<Vector2> points { get; set; } = new() { new(0, 0), new Vector2(0, 1),  new Vector2(2, 1), new Vector2(2, 0), };
         List<Vector2> points { get; set; } = new() { new(1, 0), new Vector2(1, 2), new Vector2(0, 2), new Vector2(0, 0), };
        
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
            bool rev = false;
            Dictionary<int, int> posinx = new Dictionary<int, int>();
            if (CuttingEarsTriangulator.Area(points) > 0f)
            {
                rev = true;
                points.Reverse();
            }
            points.ForEach(p =>
            {
                Positions.Add(p.ToVector3());
                Normals.Add(new Vector3(0, 0, -1));
            });
            if (!rev)
            {
                AddFace(inxs, 0,true);
            }
            else
            {
                AddFace(inxs, 0, false);
            }

            var v = new Vector3(0, 0, Length);
            points.ForEach(p =>
            {
                Positions.Add(p.ToVector3() + v);
                Normals.Add(new Vector3(0, 0, 1));
            });
            if (!rev)
            {
                AddFace(inxs, inxCount, false);
            }
            else
            {
                AddFace(inxs, inxCount, true);
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

            //Positions.Add(Positions[ip1]);
            //Normals.Add(n);
            //int a0 = startInx+3;
            //Indices.Add(a0);
            //face.Indices.Add(a0);

            //Positions.Add(Positions[i1]);
            //Normals.Add(n);
            //int ap0 = startInx+2;
            //Indices.Add(ap0);
            //face.Indices.Add(ap0);

            //Positions.Add(Positions[ip0]);
            //Normals.Add(n);
            //int a1 = startInx+1;
            //Indices.Add(a1);
            //face.Indices.Add(a1);


            //Indices.Add(a1);
            //face.Indices.Add(a1);

            //Indices.Add(ap0);
            //face.Indices.Add(ap0);

            //Positions.Add(Positions[i0]);
            //Normals.Add(n);
            //int ap1 = startInx ;
            //Indices.Add(ap1);
            //face.Indices.Add(ap1);
            //*****************************************
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

        private void AddFace(ExposedArrayList<int> inxs, int inxCount, bool isReverse = false)
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

        void AddPosition(List<int> pos, Vector3 n)
        {
            foreach (var v in pos)
            {
                Positions.Add(Positions[v]);
                Normals.Add(n);
            }
        }
        static void AddEdge(List<Edge> edges, int i0, int i1)
        {
            var edge = new Edge();
            edge.Indices.Add(i0);
            edge.Indices.Add(i1);
            edges.Add(edge);
        }


        //void CreateTriangls(int a, int b, int c, Vector3 n, Dictionary<int, int> pos, Edge edge, Face face)
        //{
        //    int i0 = 0;
        //    int i2 = 0;
        //    foreach (var v in pos)
        //    {
        //        if (a == v.Key)
        //        {
        //            Indices.Add(v.Value);
        //            i0 = v.Value;
        //        }
        //        else if (b == v.Key)
        //        {
        //            Indices.Add(v.Value);

        //        }
        //        else if (c == v.Key)
        //        {
        //            Indices.Add(v.Value);
        //            i2 = v.Value;
        //        }
        //    }
        //    face.Indices.Add(a);
        //    face.Indices.Add(b);
        //    face.Indices.Add(c);

        //    AddEdge(i2, i0);
        //    AddLine(edge, i2, i0);
        //}


    }
}
