using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX.Core;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCAD.Models
{
    public class Extrusion_with_Revolved_along_line : BaseModel
    {
        public List<Vector2> pointsFigure { get; set; } =
       new()
       {
           new Vector2(0, 0),
           new Vector2(1, 0),
           new Vector2(1, 1),
           new Vector2(0, 1)
       };
        public List<Vector2> pointsLine { get; set; } =
       new() { new(0, 0), new Vector2(2, 3), new Vector2(3, 2), new Vector2(4, 2), new Vector2(5, 2) };
        // public float Length { get; set; } = -8;
        // public double Angle { get; set; } = 0;

        public override void Update()
        {
            //Clear
            Positions.Clear();
            Indices.Clear();
            Normals.Clear();

            if (!Error)
                ErrorStr = CatchingContourErrors.Check_Contour(pointsFigure);
            var points3D = (pointsFigure.Select(t => t.ToVector3())).ToList();
            //Триангуляция контура, получение индексов треугольников. 
            List<Vector2> edg = new List<Vector2>();
            //double copy_Angle = pointsFigure;
            bool rev = false;
            //Проверка полярности контура (по часовой заполняется или против часовой)
            if (CuttingEarsTriangulator.Area(points3D) > 0f)
            {
                rev = true;
                
            }
            //нашли углы поворота кривой
            List<double> angles = new List<double>();
            for(int i = 0; i< pointsLine.Count-2; i++)
            {
                angles.Add(RecivAngle(pointsLine[i], pointsLine[i + 1], pointsLine[i + 1], pointsLine[i + 2]));
            }
            //записали порядок операции, где 1 - выдавить, а 0 - вращать
            List<int> codesOperations = new List<int>();
            codesOperations.Add(1);
            foreach (var v in angles)
            {
                if (v == 0)
                {
                    codesOperations.Add(1);
                    //codesOperations.Add(1);
                }
                else
                {
                    codesOperations.Add(0);
                    codesOperations.Add(1);
                }
            }
            //codesOperations.Add(1);
            int startPosCount = 0;
            int endPosCount = pointsFigure.Count;
            //в зависимости от операции совершаем действие
            //for (int i = 0;i<codesOperations.Count; i++)
            //{
                switch (1/*codesOperations[i]*/)
                {
                    case 0:
                        Revolved_along_line();
                        break;
                    case 1:
                        Extrusion_along_Line((pointsFigure.Select(t=>t.ToVector3(0))).ToList(), Math.Abs((pointsLine[1]- pointsLine[0]).Length()),ref startPosCount,ref endPosCount);
                        break;
                }
            //}
           // AddContourPosition(points3D, edg, 0, Vector3.Zero, -1, rev);

        }

        private void Extrusion_along_Line(List<Vector3>points, float Length, ref int startPosCount, ref int endPosCount)
        {
            var inxs = CuttingEarsTriangulator.Triangulate(points);//триангулировали контур. 
            //int inxCount = points.Count;
            bool rev = false;
            Dictionary<int, int> posinx = new Dictionary<int, int>();
            if (CuttingEarsTriangulator.Area(points) > 0f)
            {
                rev = true;
                points.Reverse();
            }

            points.ForEach(p =>
            {
                Positions.Add(p);
                Normals.Add(new Vector3(0, 0, -1));
            });
            AddFace(endPosCount, inxs, startPosCount, rev);

            var v = new Vector3(0, Length, Length);//должно быть в зависимости от того прямая или диагональ
            points.ForEach(p =>
            {
                Positions.Add(p + v);
                Normals.Add(new Vector3(0, 0, 1));
            });
            AddFace(endPosCount, inxs, endPosCount, rev);


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
        private void Revolved_along_line()
        {

        }
        private double RecivAngle(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            float h1 = Math.Abs(a2.Y - a1.Y);
            float h2 = Math.Abs(b1.Y - b2.Y);            
            float opposite2 = Math.Abs(a2.X - a1.X);
            float opposite1 = Math.Abs(b2.X - b1.X);
            double resultAngle = 0;
            if (h2 == 0 && opposite2 == 0 || h1 == 0 && opposite1 == 0)
            {
                resultAngle = 90;
            }
            else
            {
                if (h1 == 0) h1 = h2;
                else if (h2 == 0) h2 = h1;
                if (opposite1 == 0) opposite1 = opposite2;
                else if (opposite2 == 0) opposite2 = opposite1;
                double angle1 = (Math.Atan(Math.Tan(opposite2 / h1)) * 180) / Math.PI;
                double angle2 = (Math.Atan(Math.Tan(opposite1 / h2)) * 180) / Math.PI;
                
                if (Double.IsNaN(angle1 + angle2)) resultAngle = 0;
                else resultAngle = angle1 + angle2;
            }
            
            return resultAngle;
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

        private void AddFace(int conturCount, ExposedArrayList<int> inxs, int startPosInx, bool isReverse = false)
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
                AddEdge(face.Edges, i + startPosInx, i + 1 + startPosInx);
            }

            AddEdge(Edges, conturCount - 1 + startPosInx, 0 + startPosInx);
            AddEdge(face.Edges, conturCount + startPosInx, 0 + startPosInx);

        }

        void AddPosition(List<int> pos, Vector3 n)
        {
            foreach (var v in pos)
            {
                Positions.Add(Positions[v]);
                Normals.Add(n);
            }
        }
        // void AddEdge(List<Edge> edges, int i0, int i1)
        //{
        //    var edge = new Edge();
        //    edge.Indices.Add(i0);
        //    edge.Indices.Add(i1);
        //    edges.Add(edge);
        //}

        //-----------------------------------------------------------------------------------------------------------------
        private void AddContourPosition(List<Vector3> points, List<Vector2> edg, int k, Vector3 v, int normal, bool rev)
        {
            var inxs = CuttingEarsTriangulator.Triangulate(points);
            int pointsContourCount = points.Count;
            points.ForEach(p =>
            {
                Positions.Add(p + v);
                Normals.Add(new Vector3(0, 0, normal));
            });
            AddingFrontOrBackFace(pointsContourCount, inxs, k, edg, rev);
        }
        private void AddingFrontOrBackFace(int conturCount, ExposedArrayList<int> inxs, int startPosInx, List<Vector2> edg, bool isReverse = false)
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
        //Создание ребра по двум индексам точек
        static void AddEdge(List<Edge> edges, int i0, int i1)
        {
            var edge = new Edge();
            edge.Indices.Add(i0);
            edge.Indices.Add(i1);
            edges.Add(edge);
        }
    }
}
