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


        public IList<Vector2> GetCircle(int thetaDiv, bool closed = false)
        {
            
            IList<Vector2> circle = new List<Vector2>();
            
            if ((!closed && !CircleCache.TryGetValue(thetaDiv, out circle)) ||
                (closed && !ClosedCircleCache.TryGetValue(thetaDiv, out circle)))
            {
                circle = new List<Vector2>();
                if (!closed)
                {
                    CircleCache.Add(thetaDiv, circle);
                }
                else
                {
                    ClosedCircleCache.Add(thetaDiv, circle);
                }
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

