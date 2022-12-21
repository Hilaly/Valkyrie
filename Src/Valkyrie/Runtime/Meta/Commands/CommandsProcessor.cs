using System;
using System.Threading.Tasks;
using Meta;
using Newtonsoft.Json;
using UnityEngine;

namespace Valkyrie.Meta.Commands
{
    class CommandsProcessor : ICommandsProcessor
    {
        private readonly ISaveDataStorage _saveData;
        private readonly CommandArgsResolver _argsResolver;

        public CommandsProcessor(CommandArgsResolver argsResolver, ISaveDataStorage saveDataStorage)
        {
            _argsResolver = argsResolver;
            _saveData = saveDataStorage;
        }

        public async Task Execute<TCommand>(TCommand command) where TCommand : ICommand
        {
            try
            {
                Debug.Log($"[CMD]: start processing {typeof(TCommand).Name} {JsonConvert.SerializeObject(command)}");

                var ctx = _argsResolver.Create();
                
                await command.Execute(ctx);

                ctx.PlayerInfoProvider.Info.Updated = DateTime.UtcNow;

                Debug.Log($"[CMD]: {typeof(TCommand).Name} {JsonConvert.SerializeObject(command)} processed, saving...");

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
    }
}