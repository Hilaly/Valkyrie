using System.Threading.Tasks;

namespace Valkyrie.Profile.Commands.Commands
{
    public interface ICommandHandler{}

    public interface ICommandHandler<in T> : ICommandHandler
    {
        Task Execute(CommandContext context, T command);
    }
}