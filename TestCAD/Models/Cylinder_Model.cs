using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCAD
{
    /// <summary>
    /// Класс цилиндра.
    /// Содержит в себе набор стандартных свойств для цилиндра (радиус и длину) и переопределенный метод для его построения.
    /// </summary>
    class Cylinder_Model : BaseModel
    {
        /// <value>Возвращает и задает значение радиуса.</value>
        public float Radius { get; set; } = 1;

        /// <value>Возвращает и задает значение длины</value>
        public float Length { get; set; } = 10;

        /// <summary>
        /// Создание цилиндра.
        /// </summary>
        /// <param name="point1">
        /// Точка нижней окружности.
        /// </param>
        /// <param name="point2">
        /// Точка верхней окружности.
        /// </param>
        /// <param name="Radius">
        /// Радиус окружности.
        /// </param>
        /// <param name="thetaDiv">
        /// Число делений вокруг цилиндра.
        /// </param>


        public override void Update()
        {
            Vector3 point1 = new Vector3(0, 0, 0);//точка нижней окружности
            Vector3 point2 = new Vector3(0, 2, 0);//точка верхней окружности
            Vector3 n = point2 - point1;//направление
            var l = (float)Math.Sqrt(n.X * n.X + n.Y * n.Y + n.Z * n.Z);//длина
            Length = l;
            n.Normalize();
            int thetaDiv = 32;//число делений вокруг цилиндра
            var pc = new List<Vector2>();//точки начала и конца двух образующих
            pc.Add(new Vector2(0, 0));
            pc.Add(new Vector2(0, Radius));
            pc.Add(new Vector2(Length, Radius));
            pc.Add(new Vector2(Length, 0));
            n.Normalize();
            // Найти два единичных вектора, ортогональных заданному направлению
            Vector3 u = Vector3.Cross(new Vector3(0, 1, 0), n);
            if (u.LengthSquared() < 1e-3)
            {
                u = Vector3.Cross(new Vector3(1, 0, 0), n);
            }
            var v = Vector3.Cross(n, u);
            u.Normalize();
            v.Normalize();

            Helper help = new Helper();//В классе Helper сосредоточена функция для получения сегмента с окружностью.
            var circle = help.GetCircle(thetaDiv);
            int index0 = Positions.Count;
            int counter = pc.Count;
            int totalNodes = (pc.Count - 1) * 2 * thetaDiv;
            int rowNodes = (pc.Count - 1) * 2;
            for (int i = 0; i < thetaDiv; i++)
            {
                var w = (v * circle[i].X) + (u * circle[i].Y);

                for (int j = 0; j + 1 < counter; j++)
                {
                    var q1 = point1 + (n * pc[j].X) + (w * pc[j].Y);
                    var q2 = point1 + (n * pc[j + 1].X) + (w * pc[j + 1].Y);

                    Positions.Add(new Vector3((float)q1.X, (float)q1.Y, (float)q1.Z));
                    Positions.Add(new Vector3((float)q2.X, (float)q2.Y, (float)q2.Z));

                    if (Normals != null)
                    {
                        var tx = pc[j + 1].X - pc[j].X;
                        var ty = pc[j + 1].Y - pc[j].Y;
                        var normal = (-n * ty) + (w * tx);
                        normal.Normalize();

                        Normals.Add(normal);
                        Normals.Add(normal);
                    }


                    int i0 = index0 + (i * rowNodes) + (j * 2);
                    int i1 = i0 + 1;
                    int i2 = index0 + ((((i + 1) * rowNodes) + (j * 2)) % totalNodes);
                    int i3 = i2 + 1;

                    //добавление ребер
                    AddEdge(i1, i3);

                    Indices.Add(i1);
                    Indices.Add(i0);
                    Indices.Add(i2);

                    Indices.Add(i1);
                    Indices.Add(i2);
                    Indices.Add(i3);
                    //добавление граней
                    var face = new Face();
                    face.Indices.Add(i0);
                    face.Indices.Add(i3);

                    Faces.Add(face);

                    // добавление ребер к граням
                    var edge = new Edge();
                    AddLine(edge, i1, i3);

                    face.Edges.Add(edge);

                }
            }
        }
        /// <summary>
        /// Добавление ребра.
        /// </summary>
        void AddEdge(int i0, int i1)
        {
            var edge = new Edge();
            edge.Indices.Add(i0);
            edge.Indices.Add(i1);
            Edges.Add(edge);
        }
        /// <summary>
        /// Добавление ребра к определенной грани.
        /// </summary>
        static void AddLine(Edge edge, int i0, int i1)
        {
            edge.Indices.Add(i0);
            edge.Indices.Add(i1);
        }
    }
}
