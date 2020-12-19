using System;
using System.Collections.Generic;
using System.Text;
using HelixToolkit.SharpDX.Core;

namespace TestCAD
{
    /// <summary>
    /// Базовый класс для грани каждой фигуры.
    /// Содержит в себе коллекцию индексов каждой грани и набор ребер для определенной грани. 
    /// Зачем нам нужны ребра здесь, если у нас есть они в Edge? Нужны они тут для того, чтобы была возможность выделить одну грань.
    /// </summary>
    public class Face
    {
        /// <value>Возвращает коллекцию индексов граней фигуры.</value>
        public IntCollection Indices { get; } = new IntCollection();

        /// <value>Возвращает набор ребер грани.</value>
        public List<Edge> Edges { get; } = new List<Edge>();
    }
}