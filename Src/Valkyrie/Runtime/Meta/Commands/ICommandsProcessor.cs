using System.Threading.Tasks;

namespace Meta.Commands
{
    public interface ICommandsProcessor
    {
        Task Execute<T>(T command);
    }
}