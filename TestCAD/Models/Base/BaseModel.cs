using System;
using System.Collections.Generic;
using System.Text;
using HelixToolkit.SharpDX.Core;

namespace TestCAD
{
    /// <summary>
    /// Базовый абстрактный класс (интрфейс) для каждой фигуры.
    /// Содержит в себе набор стандартных полей для всех фигур и абстрактный метод для их построения, 
    /// который в дальнейшем переопределяется в каждом классе отдельной фигуры.
    /// </summary>
    public abstract class BaseModel
    {
        /// <value>Возвращает значение коллекции позиций фигуры.</value>
        public Vector3Collection Positions { get; } = new Vector3Collection();

        /// <value>Возвращает значение коллекции индексов фигуры.</value>
        public IntCollection Indices { get; } = new IntCollection();

        /// <value>Возвращает значение коллекции нормалей фигуры.</value>
        public Vector3Collection Normals { get; } = new Vector3Collection();

        /// <value>Возвращает все грани фигуры.</value>
        public List<Face> Faces { get; } = new List<Face>();

        /// <value>Возвращает все ребра фигуры.</value>
        public List<Edge> Edges { get; } = new List<Edge>();

        /// <summary>
        /// Абстрактный метод для построения каждой из фигур, 
        /// который в дальнейшем переопределяется в каждом классе отдельной фигуры.
        /// </summary>
        public abstract void Update();
    }
}