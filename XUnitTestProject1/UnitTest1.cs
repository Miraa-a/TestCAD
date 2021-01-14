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
            var ex = new Extrusion_with_angle();
            //ex.points = new()
            //{
            //    new(3, 0),
            //    new Vector2(3, 2),
            //    new Vector2(2, 2),
            //    new Vector2(2, 1),
            //    new Vector2(1, 1),
            //    new Vector2(1, 2),
            //    new Vector2(0, 2),
            //    new Vector2(0, 0)
            //};
            ex.points  = new() { new(0, 0), new Vector2(0, 1), new Vector2(2, 1), new Vector2(2, 0), };
            //ex.points  = new() { new(0, 0), new Vector2(3, 1), new Vector2(0, 1), new Vector2(4, 0), };
            ex.Angle = -5;
            ex.Length = 5;
            Starter.Show(ex);
        }



        
    }
}
