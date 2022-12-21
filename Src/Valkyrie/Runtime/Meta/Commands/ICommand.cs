using System.Threading.Tasks;

namespace Valkyrie.Meta.Commands
{
    public interface ICommand
    {
        Task Execute(CommandContext context);
    }
}