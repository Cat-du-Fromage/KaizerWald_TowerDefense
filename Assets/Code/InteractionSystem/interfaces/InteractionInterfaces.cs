using System;
using UnityEngine;

namespace TowerDefense
{
    public interface IInteractionSystem : IBaseSystem
    {
        public void SelectionNotification(SelectionEvent eventType);
    }

    public interface IInteractionSubSystem<out T> : ISubSystem
    where T : Enum
    {
        public T EventType { get; }
    }
}