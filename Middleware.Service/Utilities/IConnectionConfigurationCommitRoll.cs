namespace Middleware.Service.Utilities
{
    public interface IConnectionConfigurationCommitRoll
    {
        void Commit();
        void RollBack();
    }
}