using System.Threading.Tasks;

namespace Meta.Commands
{
    public interface ICommand
    {
        Task Execute(CommandContext context);
    }
}