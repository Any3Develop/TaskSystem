using Zenject;

namespace TaskSystem
{
    public class TaskSystemInstaller : Installer<TaskSystemInstaller>
    {
        public override void InstallBindings()
        {

            Container
                .Bind<TaskExecuteStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<TaskStorage>()
                .AsSingle()
                .NonLazy();
        }
    }
}