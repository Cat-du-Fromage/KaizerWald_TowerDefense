using UnityEngine;

namespace TowerDefense
{
    public interface IBaseSystem
    {
        public void Notify(ISubSystem subSystem);
    }

    public interface ISubSystem
    {
        public IBaseSystem MainSystem { set; }
        public void AttachSubSystemTo(IBaseSystem baseSystem)
        {
            MainSystem = baseSystem;
        }
    }
}