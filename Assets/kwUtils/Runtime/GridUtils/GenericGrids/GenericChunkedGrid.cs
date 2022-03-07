using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KWUtils
{
    public class GenericChunkedGrid<T> : GenericGrid<T>
    where T : struct
    {
        public GenericChunkedGrid(in int2 mapSize, int cellSize, Func<int2, T> createGridObject) : base(in mapSize, cellSize, createGridObject)
        {
                
        }
        
    }
}