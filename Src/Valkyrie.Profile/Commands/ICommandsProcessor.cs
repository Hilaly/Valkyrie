using System.Threading.Tasks;

namespace Valkyrie.Profile.Commands.Commands
{
    public interface ICommandsProcessor
    {
        Task Execute<T>(T command);
    }
}