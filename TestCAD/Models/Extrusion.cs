using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCAD
{
    /// <summary>
    /// Класс операции выдавливания.
    /// Содержит в себе набор точек, которые определяют контур выдавливания,ось по направлению которой выдавливать,
    /// начало выдавленной поверхности, конец выдавленной поверхности и переопределенный метод для его построения.
    /// </summary>
    class Extrusion : BaseModel
    {
        List<Vector2> points { get; set; } = new List<Vector2> {
            new Vector2(1, 0), new Vector2(0, 2),new Vector2(0, 2),new Vector2(1, 2), /*new Vector2(0, 0)*//*, new Vector2(1, 0)*/
             /*new Vector2(1, 1), new Vector2(-1, 1),*/};
        Vector3 axisX { get; set; } = new Vector3(0, -1, 0);
        Vector3 p0 { get; set; } = new Vector3(2, 0, 0);
        Vector3 p1 { get; set; } = new Vector3(-2, 0, 0);

        /// <summary>
        /// Добавляет выдавленную поверхность указанной кривой.
        /// </summary>
        /// <param name="points">
        /// 2D-точки, описывающие контур для выдавливания.
        /// </param>
        /// <param name="xaxis">
        /// Ось по направлению которой выдавливать.
        /// </param>
        /// <param name="p0">
        /// Начало выдавленной поверхности.
        /// </param>
        /// <param name="p1">
        /// Конец выдавленной поверхности.
        /// </param>
        /// <remarks>
        /// axisY - Ось определяется векторным произведением между указанной осью x и вектором начала координат p1.
        /// </remarks>
        public override void Update()
        {
            //Clear
            Positions.Clear();
            Indices.Clear();
            Normals.Clear();

            points = Check_Mistakes.strException(points);
            var p10 = p1 - p0;
            var axisY = Vector3.Cross(axisX, p10);
            axisY.Normalize();
            axisX.Normalize();
            int index0 = Positions.Count;

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                var d = (axisX * p.X) + (axisY * p.Y);
                Positions.Add(p0 + d);
                Positions.Add(p1 + d);

                if (Normals != null)
                {
                    d.Normalize();
                    Normals.Add(d);
                    Normals.Add(d);
                }

            }
            var face = new Face();
            var edge = new Edge();
            int n = points.Count - 1;
            for (int i = 0; i < n; i++)
            {
                int i0 = index0 + (i * 2);
                int i1 = i0 + 1;
                int i2 = i0 + 3;
                int i3 = i0 + 2;
                int i4 = i0 + 4;
                Indices.Add(i0);
                Indices.Add(i1);
                Indices.Add(i2);

                Indices.Add(i2);
                Indices.Add(i3);
                Indices.Add(i0);
               
                face.Indices.Add(i0);
                face.Indices.Add(i3);
                face.Indices.Add(i1);
                face.Indices.Add(i2);
               


                AddEdge(i0, i3);
                AddEdge(i1, i2);
                AddEdge(i1, i0);
                AddEdge(i2, i3);
                
                AddLine(edge, i0, i3);
                AddLine(edge, i1, i2);
                AddLine(edge, i1, i0);
                AddLine(edge, i2, i3);

            }


            Faces.Add(face);
            face.Edges.Add(edge);


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
