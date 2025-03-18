using AI.Combat.Contexts;

namespace Interfaces.AI.UBS.BaseInterfaces.Get
{
    public interface IGetContext<TContext> 
        where TContext : AIEnemyContext
    {
        public TContext GetContext();
    }
}