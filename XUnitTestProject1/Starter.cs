using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using TestCAD;
using Xunit;

namespace XUnitTestProject1
{
    class Starter
    {
        public static void Show(BaseModel m)
        {
            Run(() =>
            {
                var w = new MainWindow();
                w.VisualizeFigure(m);
                w.Show();
                return w;
            });
        }

        public static void Run(Func<Window> act)
        {
            Act = act;
            Thread t = new(ThreadStartingPoint);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }

        private static Func<Window>? Act;
        private static void ThreadStartingPoint()
        {
            Debug.Assert(Act != null, nameof(Act) + " != null");

            var app = new Application();
            app.MainWindow = Act();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.Run();
            
            //var disp = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            //disp.
            //System.Windows.Threading.Dispatcher.Run();
        }
    }
}