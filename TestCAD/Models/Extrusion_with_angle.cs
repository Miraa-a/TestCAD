﻿using HelixToolkit.SharpDX.Core;
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
            
            //Проверка контура на пересечение, повторение точек и количество точек.
            //В случае повтора, повторяющиеся точки удаляются оставляя только первое вхождение
            if (!Error)
                ErrorStr = CatchingContourErrors.Check_Contour(points);
            var points3D = (points.Select(t => t.ToVector3())).ToList();
            //Триангуляция контура, получение индексов треугольников. 
            List<Vector2> edg = new List<Vector2>();
            double copy_Angle = Angle;
            bool rev = false;
            //Проверка полярности контура (по часовой заполняется или против часовой)
            if (CuttingEarsTriangulator.Area(points3D) > 0f)
            {
                rev = true;
                copy_Angle = copy_Angle * (-1);
            }
            AddContourPosition(points3D, edg, 0, Vector3.Zero, -1, rev);

            //список нужен для содержания в нем перпендикуляров к ребру каждой грани
            List<Vector2> direction = new List<Vector2>();
            foreach (var q in FindPerp(points3D))//нашли перпендикуляры, которые представляют собой двумерные векторы
            {
                q.Normalize();//нормализировали каждый вектор
                direction.Add(q * ((float)Math.Tan((copy_Angle * Math.PI) / 180) * Length));//умножили каждый вектор на длину (для раскрытия или сужения фигуры)
            }
            //список нужен для содержания в нем направляющих для каждой точки
            List<Vector2> directionpoint = new List<Vector2>();
            directionpoint.Add(direction[0] + direction[direction.Count - 1]);
            for (int i = 1; i < direction.Count; i++)
            {
                directionpoint.Add(direction[i] + direction[i - 1]);
            }
            //к каждой точке прибавляем свою направляющую
            for (int i = 0; i < points3D.Count; i++)
            {
                points3D[i] = points3D[i] + directionpoint[i].ToVector3();
            }
            //Проверка полученного контура на пересечение, повторение точек и количество точек.
            //В случае повтора, повторяющиеся точки удаляются оставляя только первое вхождение
            if (!Error)
                ErrorStr = CatchingContourErrors.Check_Contour((points3D.Select(t => new Vector2(t.X, t.Y))).ToList());

            //Вектор длины (на сколько нужно выдавить по оси z)
            var v = new Vector3(0, 0, Length);
            //построение лицевой грани
            AddContourPosition(points3D, edg, points3D.Count, v, 1, rev);
            int sign = rev ? -1 : 1;//нужно ли перенаправить полярность? Для этого нужно будет изменить ориентацию нормалей
            AddSide(0, points3D.Count, sign, edg);
            //Проверка угла на корректность, т.е. не пересекаются ли у нас ребра при построении
            if (!Error)
                ErrorStr = CatchingContourErrors.DoEdgesCrosAfterBuildWithAngle(edg);


        }

        private void AddSide(int start, int end, int sign, List<Vector2> edg)//Боковые и нижняя грань
        {
            for (int i = start; i < end-1; i++)
            {
                int i0 = i;
                int i1 = i + 1;
                int ip0 = i0 + end;
                int ip1 = i1 + end;
                var n = GetNormal(ip0, i0, i1) * sign;//вычисление нормали через векторное произведение 
                AddingSideFace(i0, ip0, i1, ip1, n, edg);//построение боковых граней через индексы треугольников (кроме нижней)
            }

            {
                int i0 = end - 1;
                int i1 = start;
                int ip0 = i0 + end;
                int ip1 = i1 + end;
                var n = GetNormal(ip0, i0, i1) * sign;//вычисление нормали через векторное произведение
                AddingSideFace(i0, ip0, i1, ip1, n, edg);//построение нижней грани через индексы треугольников
            }
        }


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
