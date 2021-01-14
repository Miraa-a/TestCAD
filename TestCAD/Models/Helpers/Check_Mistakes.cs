using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace TestCAD
{
    class Check_Mistakes
    {
        //public static string strDoublePointsError = "Некорректный контур: Две точки имеют одинаковые координаты";
        //public static string strIntersectionError = "Некорректный контур: Контур пересекает сам себя";
        //public static string strNoPointsError = "Некорректный контур: Нет точек области";
        //public string mistake_message;
        //public static IEnumerable<Vector2> repeatPoint;

        public static bool check = false;
        public static bool closed { get; set; }
        public static List<Vector2> strException(List<Vector2> p, ref string ErrorStr)
        {
            if (p[0] != p[p.Count - 1]) //если не замкнут, то замыкаем
            {
                closed = false;
               
            }
            var copy = p.Skip(1).Take(p.Count - 1);
            List<int> index = new List<int>();
            List<int> tmp = new List<int>();
            copy = p;
            Dictionary<Vector2, int> repeatPoint = Repeat(copy);
            //проверяем на повторы первую часть группы без последнего элемента
            if (check)
            {
                index.Add((-1));
                foreach (var v in repeatPoint)
                {
                    for (int i = 0; i < p.Count; i++)
                    {
                        if (p[i] == v.Key)
                        {
                            index.Add(i);
                        }

                    }

                    index.Add(-1);
                }
                for (int i = index.Count - 2; i < 0; i++)
                {
                    if (index[i] != -1)
                    {
                        tmp.Add(index[i]);

                    }
                    else
                    {
                        for (int j = 0; j < tmp.Count - 1; j++)
                        {
                            p.RemoveAt(tmp[j + 1]);
                        }

                        tmp.Clear();
                    }
                }

            }
            
            check = false;
            
            if (p.Count < 3)//проверка контура на количество точек
            {
                ErrorStr = String.Format("Сейчас точек {0} недостаточно {1} точек", p.Count,
                    4 - p.Count);
                //throw new ArgumentException(String.Format("Сейчас точек {0} недостаточно {1} точек", p.Count,
                //    4 - p.Count));
            }

            if (areCrossing(p))
            {
                ErrorStr = "Контур пересекается";
                //throw new ArgumentException(String.Format("Контур пересекается"));
            }
            
            return p;
        }

        public static string CheckAngel(List<Vector2> copy, double Angle)
        {
            string str = "";
            Dictionary<float, float> coordinateX = new Dictionary<float, float>();
            Dictionary<float, float> coordinateY = new Dictionary<float, float>();

            List<float> cX = new List<float>();
            List<float> cY = new List<float>();
            int r = 0; int z = 0;
            for (int i = 0; i < copy.Count; i++)
            {
                if (!cX.Contains(copy[i].X))
                {
                    cX.Add(copy[i].X);
                    coordinateX.Add(r, copy[i].X);
                    r++;
                }
                if (!cY.Contains(copy[i].Y))
                {
                    cY.Add(copy[i].Y);
                    coordinateY.Add(z, copy[i].Y);
                    z++;
                }
            }
            z = 0;
            r = 0;
            if (Angle < 0)
            {
                cX.Sort();
                cY.Sort();
                foreach (var k in coordinateX)
                {
                    if (z == k.Key)
                    {
                        if (cX[z] != k.Value)
                            str = "Не верный угол, ребра пересекаются";
                    }

                    z++;
                }
                foreach (var k in coordinateY)
                {
                    if (r == k.Key)
                    {
                        if (cY[r] != k.Value)
                            str = "Не верный угол, ребра пересекаются";
                    }

                    r++;
                }
            }
            if (Angle > 0)
            {
                cX.Sort();
                cX.Reverse();
                cY.Sort();
                cX.Reverse();
                foreach (var k in coordinateX)
                {
                    if (z == k.Key)
                    {
                        if (cX[z] != k.Value)
                            str = "Не верный угол, ребра пересекаются";
                    }

                    z++;
                }
                foreach (var k in coordinateY)
                {
                    if (r == k.Key)
                    {
                        if (cY[r] != k.Value)
                            str = "Не верный угол, ребра пересекаются";
                    }

                    r++;
                }
            }
            return str;
        }
        private static Dictionary<Vector2, int> Repeat(IEnumerable <Vector2> p)
        {
            // bool check;
            var repeatPoint = p.GroupBy(x => x)
              .Where(g => g.Count() > 1)
              .ToDictionary(x => x.Key, y => y.Count());
            //группируем элементы на основе их значения, затем выбираем представителя группы, если в группе более одного элемента
            if (repeatPoint.Count != 0)//в группе есть повторы
            {
                check = true;
            }
            else//группа пустая, повторов нет
            {
                check = false;
            }
            return repeatPoint;
        }

        private static float Crossing(float a, float b, float c, float d)
        {
            return a * d - c * b;
        }
        private static double vector_mult(Vector2 a, Vector2 b) //векторное произведение
        {
            return a.X * b.Y - b.X * a.Y;
        }
        private static bool areCrossing(List<Vector2> p)//проверка на пересечение
        {
            List<float> vectormulry = new List<float>();
            for (int i = 0; i < p.Count - 3; i += 4)
            {
                vectormulry.Add(Crossing(p[i + 3].X - p[i + 2].X,
                    p[i + 3].Y - p[i + 2].Y,
                    p[i].X - p[i + 2].X, p[i].Y - p[i + 2].Y));

                vectormulry.Add(Crossing(p[i + 3].X - p[i + 2].X,
                    p[i + 3].Y - p[i + 2].Y,
                    p[i + 1].X - p[i + 2].X, p[i + 1].Y - p[i + 2].Y));

                vectormulry.Add(Crossing(p[i + 1].X - p[i].X,
                    p[i + 1].Y - p[i].Y,
                    p[i + 2].X - p[i].X, p[i + 2].Y - p[i].Y));

                vectormulry.Add(Crossing(p[i + 1].X - p[i].X,
                    p[i + 1].Y - p[i].Y,
                    p[i + 3].X - p[i].X, p[i + 3].Y - p[i].Y));

                if (vectormulry[0] * vectormulry[1] < 0 && vectormulry[2] * vectormulry[3] < 0)
                {
                    return true;
                }


                vectormulry.Clear();
            }

            return false;

            
        }
    }
}
