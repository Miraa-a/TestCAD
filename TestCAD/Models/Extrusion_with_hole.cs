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
    /// <summary>
    /// Класс операции выдавливания.
    /// Содержит в себе набор точек, длину на которую выдавливать, угол для раскрытия или сужения фигуры и переопределенный метод для его построения.
    /// </summary>
    public class Extrusion_with_hole : BaseModel
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

            List<Vector2> edg = new List<Vector2>(); //этот список заполняется точками начала и конца ребер. (для проверки пересечения)

            List<int> ListIndexIn = new List<int>();
            ListIndexIn = AddContourPosition(copy_Points, edg, 0, Vector3.Zero, new Vector3(0, 0, -1), rev, ListIndexIn);

            Font = DirPoints(Font, ((float)Math.Tan((copy_Angle * Math.PI) / 180) * Length));                    
            AddContourPosition(Font, edg, pointsContourCount, new Vector3(0, 0, Length), new Vector3(0, 0, -1), rev);

            List<int> ListIndexNotIn = new List<int>();
            copy_Points = DirPoints(copy_Points, Deltha2);
            ListIndexNotIn = AddContourPosition(copy_Points, edg, 2 * pointsContourCount, Vector3.Zero, new Vector3(0, 0, -1), rev, ListIndexNotIn);

            if (!Error)
                AddingIndices(ListIndexIn, ListIndexNotIn, pointsContourCount, 0, edg);
            ListIndexIn.Clear();
            ListIndexNotIn.Clear();
            
            Font = DirPoints(Font, Deltha2);
            Font = DirPoints(Font, ((float)Math.Tan((copy_Angle * Math.PI) / 180) * Length));           
            AddContourPosition(Font, edg, 3 * pointsContourCount, new Vector3(0, 0, Length + Deltha1), new Vector3(0, 0, 1), rev);

            int sign = rev ? -1 : 1;//нужно ли перенаправить полярность? Для этого нужно будет изменить ориентацию нормалей

            //Проверка угла на корректность, т.е. не пересекаются ли у нас ребра при построении
            edg.Clear();
            AddSide(0, copy_Points.Count, -sign, edg, copy_Points.Count);
            AddSide(2 * pointsContourCount, 3 * pointsContourCount, sign, edg, copy_Points.Count);
            if (!Error)
                ErrorStr = CatchingContourErrors.DoEdgesCrosAfterBuildWithAngle(edg);

        }

        private void AddSide(int start, int end, int sign, List<Vector2> edg, int k)//Боковые и нижняя грань
        {
            for (int i = start; i < end - 1; i++)
            {
                int i0 = i;
                int i1 = i + 1;
                int ip0 = i0 + k;
                int ip1 = i1 + k;
                var n = GetNormal(ip0, i0, i1) * sign;//вычисление нормали через векторное произведение 
                AddingSideFace(i0, ip0, i1, ip1, n, edg);//построение боковых граней через индексы треугольников (кроме нижней)
            }

            {
                int i0 = end - 1;
                int i1 = start;
                int ip0 = i0 + k;
                int ip1 = i1 + k;
                var n = GetNormal(ip0, i0, i1) * sign;//вычисление нормали через векторное произведение
                AddingSideFace(i0, ip0, i1, ip1, n, edg);//построение нижней грани через индексы треугольников
            }
        }
        private List<int> AddContourPosition(List<Vector3> points, List<Vector2> edg, int k, Vector3 v, Vector3 normal, bool rev, List<int> list = null)
        {
            //var inxs = CuttingEarsTriangulator.Triangulate(points);
            int pointsContourCount = points.Count;
            if (CuttingEarsTriangulator.Area(points) > 0f)
            {
                rev = true;
            }
            points.ForEach(p =>
                {
                    Positions.Add(p + v);
                    Normals.Add(normal);
                });
            if (!Error)
                ErrorStr = CatchingContourErrors.Check_Contour((points.Select(t => (Vector2)t)).ToList());
            if (!Error)
                ErrorStr = CatchingContourErrors.DoEdgesCrosAfterBuildWithAngle(edg);
            
            if (!Error)
            {
                var inxs = CuttingEarsTriangulator.Triangulate(points);
                if (inxs.Count == 0) ErrorStr = CatchingContourErrors.Check_PointInOtherLine((points.Select(t => (Vector2)t)).ToList());
                if (k == pointsContourCount || k == 3 * pointsContourCount)
                    AddingIndicesBack(pointsContourCount, inxs, k, edg);
                else
                    list = CreateList(points.Count, inxs, k, rev);
            }
            return list;
        }

        private List<Vector3> DirPoints(List<Vector3> copy_Points, float value)
        {
            List<Vector2> direction = new List<Vector2>();
            foreach (var q in FindPerp(copy_Points))//нашли перпендикуляры, которые представляют собой двумерные векторы
            {
                q.Normalize();//нормализировали каждый вектор
                direction.Add(q * value);//умножили каждый вектор на длину (для раскрытия или сужения фигуры)
            }
            //список нужен для содержания в нем направляющих для каждой точки
            List<Vector2> directionpoint = new List<Vector2>();
            directionpoint.Add(direction[0] + direction[direction.Count - 1]);
            for (int i = 1; i < direction.Count; i++)
            {
                directionpoint.Add(direction[i] + direction[i - 1]);
            }
            //к каждой точке прибавляем свою направляющую
            for (int i = 0; i < copy_Points.Count; i++)
            {
                copy_Points[i] = copy_Points[i] + directionpoint[i].ToVector3();
            }
            //Проверка полученного контура на пересечение, повторение точек и количество точек.
            //В случае повтора, повторяющиеся точки удаляются оставляя только первое вхождение
            var tmp = (copy_Points.Select(t => new Vector2(t.X, t.Y))).ToList();
            CatchingContourErrors.Check_Contour(tmp);
            copy_Points = (tmp.Select(t => t.ToVector3())).ToList();
            return copy_Points;
        }
        /// <summary>
        /// С помощью векторного произведения находить нормаль
        /// </summary>
        /// <param name="ip0">
        /// Точка начала первого вектора
        /// </param>
        /// <param name="i0">
        /// Точка конца первого и второго вектора
        /// </param>
        /// <param name="i1">
        /// Точка начала второго вектора
        /// </param>
        /// <returns>
        /// Трехмерный вектор нормали
        /// </returns>
        private Vector3 GetNormal(int ip0, int i0, int i1)
        {
            var v1 = Positions[ip0] - Positions[i0];
            var v2 = Positions[i1] - Positions[i0];
            var n = Vector3.Cross(v1, v2);
            n.Normalize();
            return n;
        }
        private void AddingSideFace(int i0, int ip0, int i1, int ip1, Vector3 n, List<Vector2> edg)
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
        private List<int> CreateList(int conturCount, ExposedArrayList<int> inxs, int startPosInx,  bool isReverse = false)
        {
            List<int> res = new List<int>();
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

                res.Add(i0);
                res.Add(i1);
                res.Add(i2);
            }
           
            return res;
        }
      
        private void AddingIndices(List<int> first, List<int> second, int conturCount, int startPosInx, List<Vector2> edg)
        {
            //if (first.Count == 0 && first.Count != second.Count) ErrorStr = "Внутренний контур пересекается";
            //else if (second.Count == 0 && first.Count != second.Count) ErrorStr = "Внешний контур пересекается";
            if (!Error)
            {
                first.Sort();
                second.Sort();
                int i0 = first[first.Count - 1];
                int i1 = second[second.Count - 1];
                int i2 = second[0];
                int i3 = first[0];
                int i4 = first[second.Count - 1];

                Indices.Add(i0);
                Indices.Add(i1);
                Indices.Add(i2);
                Indices.Add(i2);
                Indices.Add(i3);
                Indices.Add(i4);

                List<int> toFaceAdd = (Indices.TakeLast(6)).ToList();
                AddingFace(toFaceAdd);

                for (int i = 0; i < first.Count - 1; i++)
                {
                    i0 = first[i];
                    i1 = second[i];
                    i2 = second[i + 1];
                    i3 = first[i + 1];
                    i4 = first[i];

                    Indices.Add(i0);
                    Indices.Add(i1);
                    Indices.Add(i2);
                    Indices.Add(i2);
                    Indices.Add(i3);
                    Indices.Add(i4);

                    toFaceAdd = (Indices.TakeLast(6)).ToList();
                    AddingFace(toFaceAdd);

                }
                AddingFaceEdges(conturCount, startPosInx, edg);
            }
        }
        private void AddingFace(List<int> p)
        {
            var face = new Face();
            foreach (var t in p)
            {
                face.Indices.Add(t);
            }
            Faces.Add(face);
        }
        private void AddingFaceEdges(int conturCount, int startPosInx, List<Vector2> edg)
        {
            var face = new Face();
            for (int i = 0; i < conturCount - 1; i++)
            {
                AddEdge(Edges, i + startPosInx, i + 1 + startPosInx);
                edg.Add((Vector2)Positions[i + startPosInx]);
                edg.Add((Vector2)Positions[i + 1 + startPosInx]);
                AddEdge(face.Edges, i + startPosInx, i + 1 + startPosInx);
            }
        }
        private void AddingIndicesBack(int conturCount, ExposedArrayList<int> inxs, int startPosInx, List<Vector2> edg, bool isReverse = false)
        {
            List<int> toFaceAdd = new List<int>();
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
                toFaceAdd = (Indices.TakeLast(3)).ToList();
                AddingFace(toFaceAdd);
                
            }
            AddingFaceEdges(conturCount, startPosInx, edg);            
        }
       
        //Создание ребра по двум индексам точек
        static void AddEdge(List<Edge> edges, int i0, int i1)
        {
            var edge = new Edge();
            edge.Indices.Add(i0);
            edge.Indices.Add(i1);
            edges.Add(edge);
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
    }
}
