using System.Threading.Tasks;

namespace Valkyrie.Meta.Commands
{
    public interface ICommandsProcessor
    {
        Task Execute<T>(T command) where T : ICommand;
    }
}