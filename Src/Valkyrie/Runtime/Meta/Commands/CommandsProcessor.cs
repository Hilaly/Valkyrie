using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meta.PlayerInfo;
using Newtonsoft.Json;
using UnityEngine;
using Valkyrie.Di;
using Valkyrie.Profile;

namespace Meta.Commands
{
    class CommandsProcessor : ICommandsProcessor
    {
        [Inject] private readonly IPlayerInfoProvider _playerInfoProvider;

        private readonly ISaveDataStorage _saveData;
        private readonly Dictionary<Type, object> _cached = new();
        private readonly List<ICommandHandler> _handlers;
        private readonly CommandArgsResolver _argsResolver;

        public CommandsProcessor(CommandArgsResolver argsResolver, IEnumerable<ICommandHandler> commandHandlers,
            ISaveDataStorage saveDataStorage)
        {
            _argsResolver = argsResolver;
            _saveData = saveDataStorage;
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

                _playerInfoProvider.Info.Updated = DateTime.UtcNow;

                Debug.Log(
                    $"[CMD]: {typeof(TCommand).Name} {JsonConvert.SerializeObject(command)} processed, saving...");

                await _saveData.SaveAsync();

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
            var ctx = _argsResolver.Create();
            await handler.Execute(ctx, command);
        }
    }
}