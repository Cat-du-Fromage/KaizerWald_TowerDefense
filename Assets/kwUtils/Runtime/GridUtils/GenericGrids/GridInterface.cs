using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    public interface IGenericGrid<out T>
    where T : struct
    {
        public int CellSize        { get; }
        public int GridWidth       { get; } 
        public int GridHeight      { get; }
        public int2 MapWidthHeight { get; }
        public int2 GridBounds     { get; }
        
        public T[] GridArray { get; }
    }
}
