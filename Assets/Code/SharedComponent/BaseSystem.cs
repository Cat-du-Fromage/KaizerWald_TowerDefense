namespace TowerDefense
{
    public interface IBaseSystem
    {
        
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