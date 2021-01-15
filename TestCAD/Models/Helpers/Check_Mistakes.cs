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

        public static string CheckAngel(List<Vector2> p)
        {
            string str = "";
            Vector2 tmp = new Vector2();
            Vector2 tmp1 = new Vector2();
            List<float> vectormulry = new List<float>();
            for (int i = 0; i < p.Count - 2; i += 2)
            {
                tmp = p[i];
                tmp1 = p[i + 1];
                for (int j = i + 2; j < p.Count; j += 2)
                {
                    vectormulry.Add(Crossing(p[j + 1].X - p[j].X, p[j + 1].Y - p[j].Y, tmp.X - p[j].X, tmp.Y - p[j].Y));
                    vectormulry.Add(Crossing(p[j + 1].X - p[j].X, p[j + 1].Y - p[j].Y, tmp1.X - p[j].X, tmp1.Y - p[j].Y));
                    vectormulry.Add(Crossing(tmp1.X - tmp.X, tmp1.Y - tmp.Y, p[j].X - tmp.X, p[j].Y - tmp.Y));
                    vectormulry.Add(Crossing(tmp1.X - tmp.X, tmp1.Y - tmp.Y, p[j + 1].X - tmp.X, p[j + 1].Y - tmp.Y));
                    if (vectormulry[0] * vectormulry[1] < 0 && vectormulry[2] * vectormulry[3] < 0)
                    {
                        str = "Угол не верный";
                        return str;
                    }
                    vectormulry.Clear();
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
      
       
        private static bool areCrossing(List<Vector2> p)//проверка на пересечение
        {

            Vector2 tmp = new Vector2();
            Vector2 tmp1 = new Vector2();
            List<float> vectormulry = new List<float>();
            var copy = p;
            //copy.Add(p[0]);
            for (int i = 0; i < copy.Count - 2; i += 2)
            {
                tmp = copy[i];
                tmp1 = copy[i + 1];
                for (int j = i + 1; j < copy.Count - 1; j++)
                {
                    vectormulry.Add(Crossing(copy[j + 1].X - copy[j].X, copy[j + 1].Y - copy[j].Y, tmp.X - copy[j].X, tmp.Y - copy[j].Y));
                    vectormulry.Add(
                        Crossing(copy[j + 1].X - copy[j].X, copy[j + 1].Y - copy[j].Y, tmp1.X - copy[j].X, tmp1.Y - copy[j].Y));
                    vectormulry.Add(Crossing(tmp1.X - tmp.X, tmp1.Y - tmp.Y, copy[j].X - tmp.X, copy[j].Y - tmp.Y));
                    vectormulry.Add(Crossing(tmp1.X - tmp.X, tmp1.Y - tmp.Y, copy[j + 1].X - tmp.X, copy[j + 1].Y - tmp.Y));
                    if (vectormulry[0] * vectormulry[1] < 0 && vectormulry[2] * vectormulry[3] < 0)
                    {

                        return true;
                    }

                    vectormulry.Clear();
                }
            }

            return false;
            
        }
    }
}
