using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCAD
{
    public class Face
    {
        public IntCollection Indices { get; } = new IntCollection();//коллекция индексов граней фигуры
        public List<Edge> Edges { get; } = new List<Edge>();//набор ребер грани
    }
}
