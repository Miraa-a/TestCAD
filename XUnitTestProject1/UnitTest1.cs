using System.Windows;
using System.Windows.Documents;
using TestCAD;
using Xunit;

namespace XUnitTestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Starter.Show(new Extrusion());
        }



        
    }
}
