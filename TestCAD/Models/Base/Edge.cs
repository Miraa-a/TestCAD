using System;
using System.Collections.Generic;
using System.Text;
using HelixToolkit.SharpDX.Core;

namespace TestCAD
{
    /// <summary>
    /// Базовый класс для ребра каждой фигуры.
    /// Содержит в себе коллекцию индексов каждого ребра. 
    /// Нужен для того, чтобы выделять грани. Ребро хранится клочками линий.
    /// </summary>
    public class Edge
    {
        /// <value>Возвращает значение коллекции идексов ребра оперделенной фигуры.</value>
        public IntCollection Indices { get; } = new IntCollection();
    }
}