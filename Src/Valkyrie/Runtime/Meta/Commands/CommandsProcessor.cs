using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Valkyrie.Profile;

namespace Meta.Commands
{
    public class CommandsProcessor : ICommandsProcessor
    {
        private readonly DbContext _dbContext;
        private readonly Dictionary<Type, object> _cached = new();
        private readonly List<ICommandHandler> _handlers;

        public CommandsProcessor(IEnumerable<ICommandHandler> commandHandlers, DbContext dbContext)
        {
            _dbContext = dbContext;
            _handlers = new List<ICommandHandler>(commandHandlers);
        }

        ICommandHandler<TCommand> GetHandler<TCommand>()
        {
            if (_cached.TryGetValue(typeof(TCommand), out var oHandler))
            {
                return (ICommandHandler<TCommand>)oHandler;
            }

            var handler = (ICommandHandler<TCommand>)_handlers.FirstOrDefault(x => x is ICommandHandler<TCommand>);
            if (handler == null)
                throw new Exception($"Handler for {typeof(TCommand).Name} is not registered");

            _cached.Add(typeof(TCommand), handler);
            return handler;
        }

        public async Task Execute<TCommand>(TCommand command)
        {
            try
            {
                Debug.Log(
                    $"[CMD]: start processing {typeof(TCommand).Name} {JsonConvert.SerializeObject(command)}");

                await Execute(command, GetHandler<TCommand>());

                Debug.Log(
                    $"[CMD]: {typeof(TCommand).Name} {JsonConvert.SerializeObject(command)} processed, saving...");

                await _dbContext.SaveAsync();

                Debug.Log(
                    $"[CMD]: saved");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CMD]: failed, {e}");
                Debug.LogException(e);
                throw;
            }
        }

        async Task Execute<TCommand>(TCommand command, ICommandHandler<TCommand> handler)
        {
            var ctx = new CommandContext() { };
            await handler.Execute(ctx, command);
        }
    }
}