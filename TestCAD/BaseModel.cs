using HelixToolkit.Wpf.SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestCAD
{
    public abstract class BaseModel
    {
        public Vector3Collection Positions { get; } = new Vector3Collection();//коллекция позиций фигуры
        public IntCollection Indices { get; } = new IntCollection();//коллекция индексов фигуры
        public Vector3Collection Normals { get; } = new Vector3Collection();//коллекция нормалей фигуры
        public List<Face> Faces { get; } = new List<Face>();//грани фигуры
        public List<Edge> Edges { get; } = new List<Edge>();//ребра фигуры

        public abstract void Update();//абстрактный метод для построения каждой из фигур, который а дальнейшем переопределяется
    }
}