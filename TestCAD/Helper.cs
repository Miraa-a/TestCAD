using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCAD
{
   
   
    class Helper
    {
        private Dictionary<int, IList<Vector2>> CircleCache { get; set; } = new Dictionary<int, IList<Vector2>>();
        private Dictionary<int, IList<Vector2>> ClosedCircleCache { get; set; } = new Dictionary<int, IList<Vector2>>();
        // public IList<Vector2> circle { get; set; }/*= new List<Vector2>();*/

        /// <summary>
        /// Функция для получения сегмента с окружностью.
        /// </summary>
        /// <param name="thetaDiv">
        /// Число делений.
        /// </param>
        /// <param name="closed">
        /// Замкнут ли круг?
        /// Если true, то последняя точка не будет находиться в том же положении, что и первая.
        /// </param>
        /// <returns>
        /// Окружность.
        /// </returns>
        public IList<Vector2> GetCircle(int thetaDiv, bool closed = false)
        {
            
            IList<Vector2> circle = new List<Vector2>();
            // Если круг не может быть найден в одном из двух словарей (кешэй)
            if ((!closed && !CircleCache.TryGetValue(thetaDiv, out circle)) ||
                (closed && !ClosedCircleCache.TryGetValue(thetaDiv, out circle)))
            {
                circle = new List<Vector2>();
                // Добавить в кеш
                if (!closed)
                {
                    CircleCache.Add(thetaDiv, circle);
                }
                else
                {
                    ClosedCircleCache.Add(thetaDiv, circle);
                }
                // Определение угловых шагов
                var num = closed ? thetaDiv : thetaDiv - 1;
                for (int i = 0; i < thetaDiv; i++)
                {
                    var theta = Math.PI * 2 * ((float)i / num);
                    circle.Add(new Vector2((float)Math.Cos(theta), (float)-Math.Sin(theta)));
                }
            }
            IList<Vector2> result = new List<Vector2>();
            foreach (var point in circle)
            {
                result.Add(new Vector2((float)point.X, (float)point.Y));
            }
            return result;
        }
    }
}

