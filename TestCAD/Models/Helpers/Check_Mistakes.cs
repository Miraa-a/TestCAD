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
        public static List<Vector2> strException(List<Vector2> p)
        {
            if (p[0] != p[p.Count - 1]) //если не замкнут, то замыкаем
            {
                closed = false;

            }
            var copy = p.Skip(1).Take(p.Count - 1);
            //bool check = false;
            List<int> index = new List<int>();
            //Dictionary<Vector2, int> repeatPoint = Repeat(copy);//проверяем на повторы вторую часть группы без первого элемента
            List<int> tmp = new List<int>();
            
            //if (check)
            //{

            //    index.Add((-1));
            //    foreach (var v in repeatPoint)
            //    {
            //        for (int i = 0; i < p.Count; i++)
            //        {
            //            if (p[i] == v.Key)
            //            {
            //                index.Add(i);
            //            }

            //        }

            //        index.Add(-1);
            //    }

            //    for (int i = 1; i < index.Count; i++)
            //    {
            //        if (index[i] != -1)
            //        {
            //            tmp.Add(index[i]);

            //        }
            //        else
            //        {
            //            for (int j = 0; j < tmp.Count - 1; j++)
            //            {
            //                p.RemoveAt(tmp[j + 1]);
            //            }

            //            tmp.Clear();
            //        }
            //    }
            //}

            //index.Clear();
            //check = false;
            copy = p/*.Take(p.Count - 1)*/;
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
                throw new ArgumentException(String.Format("Сейчас точек {0} недостаточно {1} точек", p.Count,
                    4 - p.Count));
            }

            if (areCrossing(p))
            {
                throw new ArgumentException(String.Format("Контур пересекается"));
            }
            
            return p;
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
        private static double vector_mult(Vector2 a, Vector2 b) //векторное произведение
        {
            return a.X * b.Y - b.X * a.Y;
        }
        private static bool areCrossing(List<Vector2> p)//проверка на пересечение
        {
            bool cross = false;
            List<double> mult = new List<double>();
            List<Vector2> Lines = new List<Vector2>();
            for (int i=0; i < p.Count-1; i++)
            {
                Lines.Add(new Vector2(p[i+1].X-p[i].X, p[i+1].Y-p[i].Y));
                
            }
            for (int i = 0; i < Lines.Count - 1; i++)
            {
                mult.Add((vector_mult(Lines[i],Lines[i+1])));
            }

            for (int i = 0; i < mult.Count - 1; i++)
            {
                if ((mult[i] * mult[i + 1]) < 0)
                {
                    cross = true;
                }
            }

            return cross;
        }
    }
}
