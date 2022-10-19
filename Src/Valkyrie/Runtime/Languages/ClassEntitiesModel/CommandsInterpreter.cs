using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie
{
    public interface ICommandsInterpreter
    {
        IDisposable Register(string commandName, Func<object[], Task> commandExecutor);
        Task Execute(string command, params object[] args);
    }

    class CommandArgsException : ArgumentException
    {
    }

    public static class CommandsInterpreterExtensions
    {
        public static IDisposable Register(this ICommandsInterpreter interpreter, string name, Func<Task> func)
        {
            return interpreter.Register(name, args =>
            {
                if (args.Length != 0)
                    throw new CommandArgsException();
                return func();
            });
        }
        public static IDisposable Register<T0>(this ICommandsInterpreter interpreter, string name, Func<T0, Task> func)
        {
            return interpreter.Register(name, args =>
            {
                if (args.Length != 1 || args[0] is not T0)
                    throw new CommandArgsException();
                return func((T0)args[0]);
            });
        }
        public static IDisposable Register<T0,T1>(this ICommandsInterpreter interpreter, string name, Func<T0, T1, Task> func)
        {
            return interpreter.Register(name, args =>
            {
                if (args.Length != 2 || args[0] is not T0 || args[1] is not T1)
                    throw new CommandArgsException();
                return func((T0)args[0], (T1)args[1]);
            });
        }
    }

    public class CommandsInterpreter : Singleton<CommandsInterpreter>, ICommandsInterpreter
    {
        private readonly Dictionary<string, List<Func<object[], Task>>> _commands = new();

        public IDisposable Register(string commandName, Func<object[], Task> commandExecutor)
        {
            if (!_commands.TryGetValue(commandName, out var list))
                _commands.Add(commandName, list = new List<Func<object[], Task>>());
            list.Add(commandExecutor);
            Debug.Log($"[GEN]: command '{commandName}' registered");
            return new ActionDisposable(() =>
            {
                list.Remove(commandExecutor);
                Debug.Log($"[GEN]: command '{commandName}' unregistered");
            });
        }

        public async Task Execute(string command, params object[] args)
        {
            if (_commands.TryGetValue(command, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var c = list[i];
                    try
                    {
                        await c(args);
                        return;
                    }
                    catch (CommandArgsException)
                    {
                    }
                }
            }

            throw new Exception($"[GEN]: Command {command} is not registered");
        }
    }
}