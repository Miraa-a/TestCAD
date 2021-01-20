using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using HelixToolkit.SharpDX.Core;

namespace TestCAD
{
    /// <summary>
    /// Класс отлова возможных ошибок контура.
    /// Содержит в себе методы: корректировки контура, проверки пересечения, проверка количества точек,
    /// проверка на повторы точек и проверка допустимости введенного угла.
    /// </summary>
    class CatchingErrors
    {

        /// <summary>
        /// Корректирует введенный контур, удаляя лишние повторы.
        /// </summary>
        /// <param name="p">
        /// Двумерные точки контура
        /// </param>
        /// <param name="ErrorStr">
        /// Сообщение о возможной ошибке, которое вернется пользователю.
        /// </param>
       

        public static List<Vector3> Correct_Contour(List<Vector2> p, ref string ErrorStr)
        {
            p = Correct_Repeat(p);
            if (Check_CountPoints(p))//проверка контура на количество точек
            {
                ErrorStr = String.Format("Сейчас точек {0} недостаточно {1} точек", p.Count,
                    4 - p.Count);
               
            }

            if (Check_Crossing(p))
            {
                ErrorStr = "Контур пересекается";
                
            }
            
            return (p.Select(t=>t.ToVector3())).ToList();
        }
        /// <summary>
        /// Осуществляет проверку контура на количество точек.
        /// </summary>
        /// <param name="p">
        /// Двумерные точки контура
        /// </param>
        /// <returns>
        /// Количество точек достаточно - False, если не достаточно - True
        /// </returns>
        private static bool Check_CountPoints(List<Vector2> p)
        {
            if (p.Count < 3)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Осуществляет проверку допустимости введенного угла. Проверяет все ребра на возможное пересечение.
        /// </summary>
        /// <param name="p">
        /// Двумерные точки контура
        /// </param>
        /// <returns>
        /// Строку с сообщение об ошибке
        /// </returns>
        public static string Check_Angel(List<Vector2> p)
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
        /// <summary>
        /// Осуществляет проверку точек на лишние повторы и удаляет все повторы, кроме первого вхождения.
        /// </summary>
        /// <param name="p">
        /// Двумерные точки контура
        /// </param>
        /// <returns>
        /// Правильный двумерный набор точек контура
        /// </returns>
        private static List<Vector2> Correct_Repeat(List <Vector2> p)
        {
            bool check;
            var repeatPoint = p.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .ToDictionary(x => x.Key, y => y.Count());
            //группируем элементы на основе их значения, затем выбираем представителя группы, если в группе более одного элемента
            if (repeatPoint.Count != 0)//в группе есть повторы
            {
                foreach (var v in repeatPoint)
                {
                    int count_Repeat = v.Value;
                    while (count_Repeat != 1)
                    {
                        p.RemoveAt(p.LastIndexOf(v.Key));
                        count_Repeat--;
                    }
                }
            }
            return p;
        }

        private static float Crossing(float a, float b, float c, float d)
        {
            return a * d - c * b;
        }

        /// <summary>
        /// Осуществляет проверку на пересечение. Проверяет все ребра на возможное пересечение.
        /// </summary>
        /// <param name="p">
        /// Двумерные точки контура
        /// </param>
        /// <returns>
        /// Если контур пересекается - True, иначе - False
        /// </returns>
        private static bool Check_Crossing(List<Vector2> p)//проверка на пересечение
        {

            Vector2 tmp = new Vector2();
            Vector2 tmp1 = new Vector2();
            List<float> vectormulry = new List<float>();
            var copy = p;
            for (int i = 0; i < copy.Count - 2; i += 2)
            {
                tmp = copy[i];
                tmp1 = copy[i + 1];
                for (int j = i + 1; j < copy.Count - 1; j++)
                {
                    vectormulry.Add(Crossing(copy[j + 1].X - copy[j].X, copy[j + 1].Y - copy[j].Y, tmp.X - copy[j].X, tmp.Y - copy[j].Y));
                    vectormulry.Add(Crossing(copy[j + 1].X - copy[j].X, copy[j + 1].Y - copy[j].Y, tmp1.X - copy[j].X, tmp1.Y - copy[j].Y));
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
