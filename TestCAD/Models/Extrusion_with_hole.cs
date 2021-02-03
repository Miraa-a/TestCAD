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
            var copy_Points = (points.Select(t => t.ToVector3(0))).ToList();
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
            int sign = rev ? -1 : 1;
            List<Vector2> edg = new List<Vector2>(); //этот список заполняется точками начала и конца ребер. (для проверки пересечения)
            Vector3 n = new Vector3(0, 0, 0);
            AddContourPosition(copy_Points, edg, 0, new Vector3(0, 0, 0), new Vector3(0, 0, -1), rev);
            copy_Points = DirPointsWithDeltha2(copy_Points, points, -Deltha2);
            AddContourPosition(copy_Points, edg, pointsContourCount, new Vector3(0, 0, 0-Deltha1*sign), new Vector3(0, 0, 1), rev);
            List<int> ListIndexIn = new List<int>();
            //Font = DirPointsWithDeltha2(Font, points, -Deltha2);
            Font = DirPointsWithDeltha2(Font, points, ((float)Math.Tan((copy_Angle * Math.PI) / 180) * Length));
            ListIndexIn = AddContourPosition(Font, edg, 2 * pointsContourCount, new Vector3(0, 0, -Length), new Vector3(0, 0, 1), rev, ListIndexIn);

            List<int> ListIndexNotIn = new List<int>();
            Font = DirPointsWithDeltha2(Font, points, -Deltha2);
            Font = DirPointsWithDeltha2(Font, (Font.Select(t => (Vector2)t)).ToList(), ((float)Math.Tan((copy_Angle * Math.PI) / 180) * Length));
            ListIndexNotIn = AddContourPosition(Font, edg, 3 * pointsContourCount, new Vector3(0, 0, -Length), new Vector3(0, 0, 1), rev, ListIndexNotIn);

            if (!Error)
                AddingIndices(ListIndexIn, ListIndexNotIn, pointsContourCount, 0, edg);

            //нужно ли перенаправить полярность? Для этого нужно будет изменить ориентацию нормалей

            ////Проверка угла на корректность, т.е. не пересекаются ли у нас ребра при построении
            edg.Clear();

            for (int i = 0; i < Positions.Count; i++)
            {
                Positions[i] = (new Vector3(Positions[i].X,
                    Positions[i].Y * (float)(Math.Cos((180 * Math.PI) / 180)) + Positions[i].Z * (float)(Math.Sin((180 * Math.PI) / 180)),
                    -Positions[i].Y * (float)(Math.Sin((180 * Math.PI) / 180)) + Positions[i].Z * (float)(Math.Cos((180 * Math.PI) / 180))));
            }
            AddSide(0, copy_Points.Count, -sign, edg, 2 * copy_Points.Count);
            AddSide(pointsContourCount, 2 * pointsContourCount, sign, edg, 2 * pointsContourCount);
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
                if (k == 0 || k == pointsContourCount)
                    AddingIndicesBack(pointsContourCount, inxs, k, edg);
                else
                    list = CreateList(points.Count, inxs, k, rev);
            }
            return list;
        }
        private List<Vector3> DirPointsWithDeltha2(List<Vector3> copy_Points, List<Vector2> points_Isch, float value)
        {
            List<Vector2> result = FindPerp((points.Select(t => new Vector3(t.X, t.Y, 1))).ToList());
            List<Vector2> pointsDouble = new List<Vector2>();

            for (int i = 0; i < points_Isch.Count - 1; i++)
            {

                pointsDouble.Add(points_Isch[i]);
                pointsDouble.Add(points_Isch[i + 1]);

            }
            pointsDouble.Add(points_Isch[points_Isch.Count - 1]);
            pointsDouble.Add(points_Isch[0]);
            int j = 0;
            var tmpX = 1;
            var tmpY = 1;
            for (int i = 0; i < pointsDouble.Count - 1; i += 2)
            {

                if (result[j].X < 0) tmpX = -1; else tmpX = 1;
                if (result[j].Y < 0) tmpY = -1; else tmpY = 1;
                if (result[j].X != 0 && result[j].Y == 0)
                {

                    pointsDouble[i] = new Vector2(pointsDouble[i].X + value * tmpX, pointsDouble[i].Y);
                    pointsDouble[i + 1] = new Vector2(pointsDouble[i + 1].X + value * tmpX, pointsDouble[i + 1].Y);
                }
                if (result[j].X == 0 && result[j].Y != 0)
                {
                    pointsDouble[i] = new Vector2(pointsDouble[i].X, pointsDouble[i].Y + value * tmpY);
                    pointsDouble[i + 1] = new Vector2(pointsDouble[i + 1].X, pointsDouble[i + 1].Y + value * tmpY);
                }
                else if (result[j].X != 0 && result[j].Y != 0)
                {
                    pointsDouble[i] = new Vector2(pointsDouble[i].X + value * tmpX, pointsDouble[i].Y + value * tmpY);
                    pointsDouble[i + 1] = new Vector2(pointsDouble[i + 1].X + value * tmpX, pointsDouble[i + 1].Y + value * tmpY);
                }
                j++;
            }
            List<Vector2> resultList = new List<Vector2>();
            resultList.Add(GetIntersectionPointOfTwoLines(pointsDouble[pointsDouble.Count - 1], pointsDouble[pointsDouble.Count - 2], pointsDouble[0], pointsDouble[1]));
            for (int i = 0; i < pointsDouble.Count - 2; i += 2)
            {
                resultList.Add(GetIntersectionPointOfTwoLines(pointsDouble[i], pointsDouble[i + 1], pointsDouble[i + 2], pointsDouble[i + 3]));
            }
            for (int i = 0; i < resultList.Count; i++)
            {
                copy_Points[i] = resultList[i].ToVector3(0);
            }
            var tmp = (copy_Points.Select(t => new Vector2(t.X, t.Y))).ToList();
            CatchingContourErrors.Check_Contour(tmp);
            copy_Points = (tmp.Select(t => t.ToVector3(0))).ToList();
            return copy_Points;

        }
        /// <summary>
        /// Возвращает точку пересечения двух прямых
        /// </summary>
        /// <param name="p1_1">Первая точка прямой 1 КЕК</param>
        /// <param name="p1_2">Вторая точка прямой 1</param>
        /// <param name="p2_1">Первая точка прямой 2</param>
        /// <param name="p2_2">Вторая точка прямой 2</param>
        /// <param name="state">-1, если параллельны, 0 если совпадают, 1 если пересекаются, -2 если ошибка</param>
        /// <returns></returns>
        public Vector2 GetIntersectionPointOfTwoLines(Vector2 p1_1, Vector2 p1_2, Vector2 p2_1, Vector2 p2_2/*, out int state*/)
        {
            //state = -2;
            Vector2 result = new Vector2();
            //Если знаменатель (n) равен нулю, то прямые параллельны.
            //Если и числитель (m или w) и знаменатель (n) равны нулю, то прямые совпадают.
            //Если нужно найти пересечение отрезков, то нужно лишь проверить, лежат ли ua и ub на промежутке [0,1].
            //Если какая-нибудь из этих двух переменных 0 <= ui <= 1, то соответствующий отрезок содержит точку пересечения.
            //Если обе переменные приняли значения из [0,1], то точка пересечения прямых лежит внутри обоих отрезков.
            float m = ((p2_2.X - p2_1.X) * (p1_1.Y - p2_1.Y) - (p2_2.Y - p2_1.Y) * (p1_1.X - p2_1.X));
            float w = ((p1_2.X - p1_1.X) * (p1_1.Y - p2_1.Y) - (p1_2.Y - p1_1.Y) * (p1_1.X - p2_1.X)); //Можно обойтись и без этого
            float n = ((p2_2.Y - p2_1.Y) * (p1_2.X - p1_1.X) - (p2_2.X - p2_1.X) * (p1_2.Y - p1_1.Y));

            float Ua = m / n;
            float Ub = w / n;

            if ((n == 0) && (m != 0))
            {
                //state = -1; //Прямые параллельны (не имеют пересечения)
            }
            else if ((m == 0) && (n == 0))
            {
                //state = 0; //Прямые совпадают
                result.X = p1_2.X;
                result.Y = p1_2.Y;
            }
            else
            {
                //Прямые имеют точку пересечения
                result.X = p1_1.X + Ua * (p1_2.X - p1_1.X);
                result.Y = p1_1.Y + Ua * (p1_2.Y - p1_1.Y);

                // Проверка попадания в интервал
                bool a = result.X >= p1_1.X; bool b = result.X <= p1_1.X; bool c = result.X >= p2_1.X; bool d = result.X <= p2_1.X;
                bool e = result.Y >= p1_1.Y; bool f = result.Y <= p1_1.Y; bool g = result.Y >= p2_1.Y; bool h = result.Y <= p2_1.Y;

                if (((a || b) && (c || d)) && ((e || f) && (g || h)))
                {
                    // state = 1; //Прямые имеют точку пересечения
                }
            }
            return result;
        }
        private List<Vector3> DirPointsWithAngle(List<Vector3> copy_Points, float value)
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
            //Vector3 z = new Vector3(0, 0, 1);
            //for(int i = 0; i<p.Count-1;i++)
            //{
            //    var tmp = p[i + 1] - p[i];
            //    var t = Vector3.Cross(z, tmp);
            //    result.Add(new Vector2(t.X, t.Y));
            //}
            //Vector3 tmp1 = Vector3.Cross(p[p.Count - 1], p[0]);
            //result.Add(new Vector2(tmp1.X, tmp1.Y));
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
 