using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCAD
{
    /// <summary>
    /// Класс операции вращения.
    /// Содержит в себе набор точек, которые определяют x-расстояние от начала координат вдоль оси вращения и координаты y-радиус, начало оси вращения,
    /// направление оси вращения и переопределенный метод для его построения.
    /// </summary>
    class Revolved : BaseModel
    {
        /// <summary>
        /// Добавляет поверхность вращения.
        /// </summary>
        /// <param name="points">
        /// Точки (координаты x-расстояние от начала координат вдоль оси вращения, координаты y-радиус) 
        /// </param>
        /// <param name="origin">
        /// Начало оси вращения.
        /// </param>
        /// <param name="direction">
        /// Направление оси вращения.
        /// </param>


        public List<Vector2> points { get; set; } = new List<Vector2>  {
            new Vector2(0, 0.5f) , new Vector2(0.5f, 0.5f) , 
            new Vector2(0.3f, 0.5f)//, new Vector2(0.34f, 0.1f),
            //new Vector2(0.4f, 0.14f), new Vector2(0.5f, 0.5f),
            /*new Vector2(0.7f, 0.56f), new Vector2(1, 0.46f) */};
        public Vector3 origin { get; set; } = new Vector3(0, 0, 0);
        public Vector3 direction { get; set; } = new Vector3(0, 1, 0);


        public override void Update()
        {
            
            //points = Check_Mistakes.strException(points,ErrorStr);
            direction.Normalize();

            // Найти два единичных вектора, перпендикулярных заданному направлению

            var u = FindAnyPerpendicular(direction);
            Vector3 tmp = direction;
            var v = Vector3.Cross(tmp, u);
            u.Normalize();
            v.Normalize();
            int thetaDiv = 32;
           
            Helper help = new Helper();
            var circle = help.GetCircle(thetaDiv);
            int index0 = Positions.Count;
            int n = points.Count;

            int totalNodes = (points.Count - 1) * 2 * thetaDiv;
            int rowNodes = (points.Count - 1) * 2;
            var face = new Face();
            var edge = new Edge();
            for (int i = 0; i < thetaDiv; i++)
            {
                var w = (v * circle[i].X) + (u * circle[i].Y);

                for (int j = 0; j + 1 < n; j++)
                {
                    // Add segment
                    var q1 = origin + (direction * points[j].X) + (w * points[j].Y);
                    var q2 = origin + (direction * points[j + 1].X) + (w * points[j + 1].Y);

                    // TODO: should not add segment if q1==q2 (corner point)
                    // const double eps = 1e-6;
                    // if (Point3D.Subtract(q1, q2).LengthSquared < eps)
                    // continue;
                    Positions.Add(q1);
                    Positions.Add(q2);

                    if (Normals != null)
                    {
                        var tx = points[j + 1].X - points[j].X;
                        var ty = points[j + 1].Y - points[j].Y;
                        var normal = (-direction * ty) + (w * tx);
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

                    face.Indices.Add(i1);
                    face.Indices.Add(i3);
                    face.Indices.Add(i0);
                    face.Indices.Add(i2);

                    AddEdge(i1, i3);
                    AddEdge(i0, i2);
                    AddLine(edge, i1, i3);
                    AddLine(edge, i0, i2);
                 
                }
                //}
            }
            Faces.Add(face);
            face.Edges.Add(edge);

        }
        private Vector3 FindAnyPerpendicular(Vector3 n)
        {
            n.Normalize();
            Vector3 u = Vector3.Cross(new Vector3(0, 1, 0), n);
            if (u.LengthSquared() < 1e-3)
            {
                u = Vector3.Cross(new Vector3(1, 0, 0), n);
            }

            return u;
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
    }
}
