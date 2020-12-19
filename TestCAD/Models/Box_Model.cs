using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCAD
{
    /// <summary>
    /// Класс коробки.
    /// Содержит в себе набор стандартных свойств для коробки (длину,ширину и высоту) и переопредленный метод для ее построения.
    /// </summary>
    class Box_Model : BaseModel
    {
        /// <value>Возвращает и задает значение длины коробки по Х.</value>
        public float Length { get; set; } = 1; // по X

        /// <value>Возвращает и задает значение длины коробки по Y.</value>
        public float Width { get; set; } = 2; // по Y

        /// <value>Возвращает и задает значение длины коробки по Z.</value>
        public float Height { get; set; } = 3; // по Z

        /// <summary>
        /// Добавляет коробку с заданными гранями, выровненными по заданным осям.
        /// </summary>
        /// <param name="c">Точка центра куба.</param>
        /// <param name="Length">Длина коробки вдоль оси X.</param>
        /// <param name="Width">Длина коробки вдоль оси Y.</param>
        /// <param name="Height">Длина коробки вдоль оси Z.</param>

        public override void Update()
        {
            //Очистка
            Positions.Clear();
            Indices.Clear();
            Normals.Clear();

            var c = new Vector3(Length, Width, Height) / 2;

            AddCubeFace(c, Vector3.UnitX, Vector3.UnitZ, Length, Width, Height);
            AddCubeFace(c, -Vector3.UnitX, Vector3.UnitZ, Length, Width, Height);
            AddCubeFace(c, -Vector3.UnitY, Vector3.UnitZ, Width, Length, Height);
            AddCubeFace(c, Vector3.UnitY, Vector3.UnitZ, Width, Length, Height);
            AddCubeFace(c, Vector3.UnitZ, Vector3.UnitY, Height, Length, Width);
            AddCubeFace(c, -Vector3.UnitZ, Vector3.UnitY, Height, Length, Width);

            // добавление ребер
            AddEdge(0, 1);
            AddEdge(1, 2);
            AddEdge(2, 3);
            AddEdge(3, 0);

            AddEdge(4, 5);
            AddEdge(5, 6);
            AddEdge(6, 7);
            AddEdge(7, 4);

            AddEdge(0, 5);
            AddEdge(1, 4);
            AddEdge(2, 7);
            AddEdge(3, 6);
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
        /// Добавление грани куба.
        /// </summary>
        /// <param name="center">
        /// Центр Куба.
        /// </param>
        /// <param name="normal">
        /// Вектор нормали для грани.
        /// </param>
        /// <param name="up">
        /// Вектор вверх для грани.
        /// </param>
        /// <param name="dist">
        /// Расстояние от центра куба до грани.
        /// </param>
        /// <param name="width">
        /// Ширина грани.
        /// </param>
        /// <param name="height">
        /// Высота грани.
        /// </param>
        void AddCubeFace(Vector3 center, Vector3 normal, Vector3 up, float dist, float width, float height)
        {
            var right = Vector3.Cross(normal, up);
            var n = normal * dist / 2;
            up *= height / 2;
            right *= width / 2;
            var p1 = center + n - up - right;
            var p2 = center + n - up + right;
            var p3 = center + n + up + right;
            var p4 = center + n + up - right;

            int i0 = Positions.Count;
            Positions.Add(p1);
            Positions.Add(p2);
            Positions.Add(p3);
            Positions.Add(p4);
            if (Normals != null)
            {
                Normals.Add(normal);
                Normals.Add(normal);
                Normals.Add(normal);
                Normals.Add(normal);
            }

            Indices.Add(i0 + 2);
            Indices.Add(i0 + 1);
            Indices.Add(i0 + 0);
            Indices.Add(i0 + 0);
            Indices.Add(i0 + 3);
            Indices.Add(i0 + 2);

            // добавление граней
            var face = new Face();
            face.Indices.Add(i0 + 2);
            face.Indices.Add(i0 + 1);
            face.Indices.Add(i0 + 0);
            face.Indices.Add(i0 + 0);
            face.Indices.Add(i0 + 3);
            face.Indices.Add(i0 + 2);

            Faces.Add(face);

            // добавление ребер к граням
            var edge = new Edge();
            AddLine(edge, i0 + 0, i0 + 1);
            AddLine(edge, i0 + 1, i0 + 2);
            AddLine(edge, i0 + 2, i0 + 3);
            AddLine(edge, i0 + 3, i0 + 0);
            //AddLine(edge, i0 + 2, i0 + 1);
            face.Edges.Add(edge);
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