﻿using SharpDX;
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
    class CatchingContourErrors
    {
        //public static bool er
        /// <summary>
        /// Корректирует введенный контур, удаляя лишние повторы.
        /// </summary>
        /// <param name="p">
        /// Двумерные точки контура
        /// </param>
        /// <param name="ErrorStr">
        /// Сообщение о возможной ошибке, которое вернется пользователю.
        /// </param>
       

        public static string Check_Contour(List<Vector2> p)
        {
            string ErrorCatchStr = null;
            //p = Correct_Repeat(p);
            if (Check_Repeat(p))
            {
                ErrorCatchStr = String.Format("В контуре есть повторы или несколько точек обратились в одну (слишком большое дельта 2)");
            }
            if (Check_CountPoints(p))//проверка контура на количество точек
            {
                ErrorCatchStr = String.Format("Сейчас точек {0} недостаточно {1} точек", p.Count,
                    4 - p.Count);
            }
            if (Check_Crossing(p))
            {
                ErrorCatchStr = "Контур пересекается";
            }

            return ErrorCatchStr;
        }
        public static string Check_Deltha1(float len, float deltha1)
        {
            string ErrorCatchStr = null;
            if (deltha1 > len)
            {
                ErrorCatchStr = "Недопустимая высота внутреннего контура.";
            }
            return ErrorCatchStr;
        }
        public static string Check_Deltha2(List<Vector2> p, List<Vector2> p2)
        {
            string ErrorCatchStr = null;
            if (AreaCalculate(p) == AreaCalculate(p2))
            {
                ErrorCatchStr = "Недопустимый отступ от контура. Он либо равен нулю, либо отражает фигуру зеркально. " +
                    "Если вы ввели ноль, то возможно вам следует воспользовать простым выдавливанием?";
            }
            return ErrorCatchStr;
        }
        private static float AreaCalculate(List<Vector2> p)
        {
            float result=0;
            List<float> right = new List<float>();
            List<float> left = new List<float>();
            for (int i = 0; i< p.Count - 1; i++)
            {
                right.Add(p[i].X * p[i + 1].Y);
                left.Add(p[i+1].X * p[i].Y);
            }
            right.Add(p[p.Count - 1].X * p[0].Y);
            left.Add(p[0].X * p[p.Count - 1].Y);
            var sum = right.Sum();
            foreach(var v in left)
            {
                result = sum - v;
            }
            result = Math.Abs(result * 0.5f);
            return result;
        }
        private static bool Check_Repeat(List<Vector2> p)
        {
            bool check;
            var repeatPoint = p.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .ToDictionary(x => x.Key, y => y.Count());
            //группируем элементы на основе их значения, затем выбираем представителя группы, если в группе более одного элемента
            if (repeatPoint.Count != 0)//в группе есть повторы
            {
                return true;
            }
            return false;
        }

        public static string Check_PointInOtherLine(List<Vector2> p)//Точка одного ребра лежит на другом ребре
        {
            string strError = null;
            Vector2 tmp = new Vector2();
            Vector2 tmp1 = new Vector2();
            for (int i = 0; i < p.Count - 1; i++)
            {
                tmp = p[i];
                tmp1 = p[i + 1];

                for (int j = i + 1; j < p.Count; j++)
                {

                    if ((tmp1 - tmp).Length() > p[j].Length()) return strError = "Точка одного ребра лежит на другом ребре";
                }
            }
            return strError;
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
        public static string DoEdgesCrosAfterBuildWithAngle(List<Vector2> p)
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
                    if (LineCross_(tmp.X, tmp.Y, tmp1.X, tmp1.Y, p[j].X, p[j].Y, p[j + 1].X, p[j + 1].Y))
                    {
                        str = "Угол не верный";
                        return str;
                    }
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

        private static bool LineCross_(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            var v1 = Crossing(x4 - x3, y4 - y3, x1 - x3, y1 - y3);
            var v2 = Crossing(x4 - x3, y4 - y3, x2 - x3, y2 - y3);
            var v3 = Crossing(x2 - x1, y2 - y1, x3 - x1, y3 - y1);
            var v4 = Crossing(x2 - x1, y2 - y1, x4 - x1, y4 - y1);
            if (v1 * v2 < 0 && v3 * v4 < 0)
            {
                return true;
            }
            return false;
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
            Vector2 tmp2 = new Vector2();
            Vector2 tmp3 = new Vector2();
            List<float> vectormulry = new List<float>();
            for (int i = 0; i < p.Count - 2; i += 2)
            {
                tmp = p[i];
                tmp1 = p[i + 1];
                for (int j = i + 1; j < p.Count - 1; j++)
                {
                    tmp2 = p[j];
                    tmp3 = p[j+1];
                    if (LineCross_(tmp.X, tmp.Y, tmp1.X, tmp1.Y, tmp2.X, tmp2.Y, tmp3.X, tmp3.Y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
