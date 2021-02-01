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

    public class HoleTest : BaseModel
    {
        public List<Vector2> points { get; set; } =
           new()
           {
               new Vector2(-1, -1),
               new Vector2(-1, 4),
               new Vector2(4, 4),
               new Vector2(4, -1)
           };

        public float Length { get; set; } = 5;
        public double Angle { get; set; } = -15;
        public float Deltha1 { get; set; } = 1;
        public float Deltha2 { get; set; } = 2;
        public override void Update()
        {
            //Clear
            Positions.Clear();
            Indices.Clear();
            Normals.Clear();

            if (!Error)
                ErrorStr = CatchingContourErrors.Check_Contour(points);
            var copy_Points = (points.Select(t => t.ToVector3())).ToList();
            var Font = copy_Points;

            int pointsContourCount = copy_Points.Count;
            bool rev = false;
            double copy_Angle = Angle;

            //Проверка полярности контура (по часовой заполняется или против часовой)
            if (CuttingEarsTriangulator.Area(copy_Points) > 0f)
            {
                rev = true;
                copy_Angle = copy_Angle * (-1);
            }
            var inxs = CuttingEarsTriangulator.Triangulate(copy_Points);

            copy_Points.ForEach(p =>
            {
                Positions.Add(p);
                Normals.Add(new Vector3(0, 0, -1));
            });
            List<Vector2> edg = new List<Vector2>(); //этот список заполняется точками начала и конца ребер. (для проверки пересечения)
            AddingFrontOrBackFace(pointsContourCount, inxs, 0, edg, rev);
            //список нужен для содержания в нем перпендикуляров к ребру каждой грани
            List<Vector2> direction = new List<Vector2>();
            float len = ((float)Math.Tan((copy_Angle * Math.PI) / 180) * Length);
            foreach (var q in FindPerp(copy_Points))//нашли перпендикуляры, которые представляют собой двумерные векторы
            {
                q.Normalize();//нормализировали каждый вектор
                direction.Add(q * Deltha2);//умножили каждый вектор на длину (для раскрытия или сужения фигуры)
            }
            //список нужен для содержания в нем направляющих для каждой точки
            List<Vector2> directionpoint = new List<Vector2>();
            directionpoint.Add(direction[0] + direction[direction.Count - 1]);
            for (int i = 1; i < direction.Count; i++)
            {
                directionpoint.Add(direction[i] + direction[i - 1]);
            }
            // к каждой точке прибавляем свою направляющую
            //if (points[0].X == points[copy_Points.Count - 1].X)
            //    copy_Points[0] = new Vector3(copy_Points[copy_Points.Count - 1].X, copy_Points[0].Y + directionpoint[0].Y, copy_Points[0].Z);
            //else if (points[0].Y == points[copy_Points.Count - 1].Y)
            //    copy_Points[0] = new Vector3(copy_Points[0].X + directionpoint[0].X, copy_Points[copy_Points.Count - 1].Y, copy_Points[0].Z);
            //else
                copy_Points[0] = copy_Points[0] + directionpoint[0].ToVector3();
            copy_Points[1] = copy_Points[1] + directionpoint[1].ToVector3();
            copy_Points[2] = copy_Points[2] + directionpoint[2].ToVector3();
            copy_Points[3] = copy_Points[3] + directionpoint[3].ToVector3();
            copy_Points[4] = copy_Points[4] + directionpoint[4].ToVector3();
            for(int i = 0; i<points.Count-1; i++)
            {
                if (points[i].X == points[i + 1].X && copy_Points[i].X != copy_Points[i + 1].X)
                {
                    copy_Points[i] = new Vector3(copy_Points[i + 1].X, copy_Points[i].Y, copy_Points[i].Z);
                }
                else if (points[i].Y == points[i + 1].Y && copy_Points[i].Y != copy_Points[i + 1].Y)
                {
                    copy_Points[i] = new Vector3(copy_Points[i].X, copy_Points[i+1].Y, copy_Points[i].Z);
                }
            }
            //for (int i = 1; i < copy_Points.Count; i++)
            //{

            //        if (points[i].X == points[i - 1].X)
            //            copy_Points[i] = new Vector3(copy_Points[i-1].X, copy_Points[i].Y + directionpoint[i].Y, copy_Points[i].Z);
            //        else if (points[i].Y == points[i - 1].Y)
            //            copy_Points[i] = new Vector3(copy_Points[i].X + directionpoint[i].X, copy_Points[i-1].Y, copy_Points[i].Z);
            //    else
            //        copy_Points[i] = copy_Points[i] + directionpoint[i].ToVector3();
            //}
            ////Проверка полученного контура на пересечение, повторение точек и количество точек.
            ////В случае повтора, повторяющиеся точки удаляются оставляя только первое вхождение
            //if (!Error)
            //    ErrorStr = CatchingContourErrors.Check_Contour((copy_Points.Select(t => new Vector2(t.X, t.Y))).ToList());
            //inxs = CuttingEarsTriangulator.Triangulate(copy_Points);

            copy_Points.ForEach(p =>
            {
                Positions.Add(p+new Vector3(0,0,Length));
                Normals.Add(new Vector3(0, 0, 1));
            });
            //List<Vector2> edg = new List<Vector2>(); //этот список заполняется точками начала и конца ребер. (для проверки пересечения)
            AddingFrontOrBackFace(pointsContourCount, inxs, pointsContourCount, edg, rev);
        }
        List<Vector2> FindPerp(List<Vector3> p)//Поиск перепендикуляра к ребру каждой грани
        {
            List<Vector2> result = new List<Vector2>();
            for (int i = 0; i < p.Count - 1; i++)
            {
                Vector3 r = Vector3.Cross(p[i], p[i + 1]);
                result.Add(new Vector2(r.X, r.Y));
            }
            Vector3 tmp = Vector3.Cross(p[p.Count - 1], p[0]);
            result.Add(new Vector2(tmp.X, tmp.Y));
            return result;
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
        static void AddEdge(List<Edge> edges, int i0, int i1)
        {
            var edge = new Edge();
            edge.Indices.Add(i0);
            edge.Indices.Add(i1);
            edges.Add(edge);
        }
    }
}
