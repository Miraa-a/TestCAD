using System;
using SharpDX;
using System.Collections.Generic;
using System.Text;

namespace TestCAD
{
    class Sphere_Model : BaseModel
    {
        public float Radius { get; set; } = 1;
        /// <summary>
        /// Добавление сферы.
        /// </summary>
        /// <param name="с">
        /// Центр сферы.
        /// </param>
        /// <param name="Radius">
        /// Радиус сферы.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the ellipsoid.
        /// </param>
        /// <param name="phiDiv">
        /// The number of divisions from top to bottom of the ellipsoid.
        /// </param>
        public override void Update()
        {
            //Очитска
            Positions.Clear();
            Indices.Clear();
            Normals.Clear();

            var c = new Vector3(1,1,1);
            int thetaDiv = 32; int phiDiv = 32;
            int index0 = this.Positions.Count;
            var dt = 2 * Math.PI / thetaDiv;
            var dp = Math.PI / phiDiv;

            for (int pi = 0; pi <= phiDiv; pi++)
            {
                var phi = pi * dp;

                for (int ti = 0; ti <= thetaDiv; ti++)
                {
                    var theta = ti * dt;
                    var x = Math.Cos(theta) * Math.Sin(phi);
                    var y = Math.Sin(theta) * Math.Sin(phi);
                    var z = Math.Cos(phi);

                    var p = new Vector3((float)(c.X + (Radius * x)), (float)(c.Y + (Radius * y)), (float)(c.Z + (Radius * z)));
                    Positions.Add(new Vector3(p.X, p.Y, p.Z));

                    if (Normals != null)
                    {
                        var n = new Vector3((float)x, (float)y, (float)z);
                        Normals.Add(n);
                    }
                }
            }

            this.AddIndices(index0, phiDiv + 1, thetaDiv + 1, true);
        }
        /// <summary>
        /// Добавление индексов треугольников.
        /// </summary>
        /// <param name="index0">
        /// Смещение индекса.
        /// </param>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// Количество строк.
        /// </param>
        /// <param name="isSpherical">
        /// если флаг в значение true, то создатуся треугольники сверху и снизу (сферическая сетка).
        /// </param>
        public void AddIndices(int index0, int rows, int columns, bool isSpherical = false)
        {
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < columns - 1; j++)
                {
                    int ij = (i * columns) + j;
                    if (!isSpherical || i > 0)
                    {
                        Indices.Add(index0 + ij);
                        Indices.Add(index0 + ij + 1 + columns);
                        Indices.Add(index0 + ij + 1);
                    }

                    if (!isSpherical || i < rows - 2)
                    {
                        Indices.Add(index0 + ij + 1 + columns);
                        Indices.Add(index0 + ij);
                        Indices.Add(index0 + ij + columns);
                    }
                }
            }

        }
    }
}
