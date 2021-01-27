using System.Windows;
using System.Windows.Documents;
using SharpDX;
using TestCAD;
using TestCAD.Models;
using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var ex = new Extrusion_with_angle { Angle = -5, Length = 5 };
            ex.points = new() { new(0, 0), new Vector2(0, 1), new Vector2(2, 1), new Vector2(2, 0), };
           
            Starter.Show(ex);
        }



        [Fact]
        public void Test2()
        {
            var ex = new Extrusion_with_angle { Angle = -5, Length = 5 };
            ex.points = new() { new(1, 0), new Vector2(1, 2), new Vector2(0, 2), new Vector2(0, 0), };

           
            Assert.Equal(0, actual: ex.Edges.Count);
            Starter.Show(ex);
        }
        [Fact]
        public void TestTriangle3()
        {
            var ex = new Extrusion_with_angle(){ Angle = -5, Length = 5 };
            ex.points = new() { new(0, 0), new Vector2(3, 1), new Vector2(0, 1) };

            //Assert.Equal(9, ex.Edges.Count);
            Starter.Show(ex);
        }
        [Fact]
        public void Test4()
        {
            var ex = new Extrusion_with_angle();
            ex.points = new()
            {
                new(3, 0),
                new Vector2(3, 2),
                new Vector2(2, 2),
                new Vector2(2, 1),
                new Vector2(1, 1),
                new Vector2(1, 2),
                new Vector2(0, 2),
                new Vector2(0, 0)
            };
            ex.Angle = -5;
            ex.Length = 5;
            Starter.Show(ex);
        }
        [Fact]
        public void TestExtruEx2()
        {
            // контур с непересекающимися боковыми гранями
            var ex = new Extrusion_with_angle() { Angle = -15, Length = 5 };
            ex.points = new()
            {
                new(3, 0),
                new Vector2(3, 2),
                new Vector2(1.5f, 1),
                new Vector2(0, 2),
                new Vector2(0, 0)
            };
            Starter.Show(ex);
        }

        [Fact]
        public void TestExtruEx3()
        {
            //ошибка переворота контура
            var ex = new Extrusion_with_angle() { Angle = -5, Length = 5 };
            ex.points = new()
            {
                new(3, 0),
                new Vector2(3, 2),
                new Vector2(1f, 1),
                new Vector2(0, 2),
                new Vector2(0, 0)
            };
            Starter.Show(ex);
        }
        [Fact]
        public void TestExtruEx4()
        {
            //ошибка переворота контура
            var ex = new Extrusion_with_angle() { Angle = 0, Length = 5 };
            ex.points = new()
            {
                new Vector2(0, 0),
                new Vector2(1, 1),
                new Vector2(2, 1),
                new Vector2(2, 0.5f),
                new Vector2(0, 1)
                //new Vector2(1, 1),
                //new Vector2(1, 4),
                //new Vector2(4, 4),
                //new Vector2(4, 1),
            };
            Starter.Show(ex);
        }
        [Fact]
        public void TestExtruEx5()
        {
            //ошибка переворота контура
            var ex = new Extrusion_with_hole() { Angle = -1, Length = 5, Deltha2 = 0.2f, Deltha1 = 6f };
            ex.points = new()
            {
                new Vector2(1, 0),
                new Vector2(1, 3),
                new Vector2(2, 3),
                new Vector2(2, 2),
                new Vector2(3, 2),
                new Vector2(3, 3),
                new Vector2(4, 3),
                new(4, 0),

                //new Vector2(1, 1),
                //new Vector2(1, 5),
                //new Vector2(4, 1),

                //new Vector2(1, 1),
                //new Vector2(1, 5),
                //new Vector2(5, 5),
                //new Vector2(3, 3),
                //new Vector2(5, 1)

                //new Vector2(-1, -1),
                //new Vector2(-1, 4),
                //new Vector2(4, 4),
                //new Vector2(4, -1)

                //new Vector2(2, 2),
                //new Vector2(2, 3),
                //new Vector2(2, 2),
                //new Vector2(3, 3),
                //new Vector2(2, 2),
                //new Vector2(2, 2),
                //new Vector2(3, 2),
            };
            Starter.Show(ex);
        }
        [Fact]
        public void TestExtruEx6()
        {
            //исходный контур пересекается
            var ex = new Extrusion_with_hole() { Angle = 0, Length = 5, Deltha2 = 1f, Deltha1 = 1f };
            ex.points = new()
            {
                new Vector2(0, 0),
                new Vector2(1, 1),
                new Vector2(2, 1),
                new Vector2(2, 0.5f),
                new Vector2(0, 1)
            };
            Starter.Show(ex);
        }
        [Fact]
        public void TestExtruEx7()
        {
            //ванна
            var ex = new Extrusion_with_hole() { Angle = -15, Length = 5, Deltha2 = 2f, Deltha1 = 3f };
            ex.points = new()
            {

                new Vector2(-1, -1),
                new Vector2(-1, 4),
                new Vector2(4, 4),
                new Vector2(4, -1)
            };
            Starter.Show(ex);
        }
    }
}
