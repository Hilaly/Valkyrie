using System.Threading.Tasks;

namespace Valkyrie
{
    public interface ICommandsInterpreter
    {
        Task Execute(string command, params object[] args);
    }
    
    public class CommandsInterpreter : Singleton<CommandsInterpreter>, ICommandsInterpreter
    {
        public Task Execute(string command, params object[] args)
        {
            throw new System.NotImplementedException();
        }
    }
}