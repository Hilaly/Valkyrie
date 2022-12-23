using System;
using System.Collections.Generic;

namespace Prototype.TryEvents
{
    public interface IEventHandler
    {
        string EventName { get; }
        
        bool Test(string eventName, object[] args);
    }
    
    class EventHandler : IEventHandler
    {
        private readonly List<string> _matches = new();
        private readonly Action _callbackAction;
        
        public string EventName { get; }

        public EventHandler(string eventName, Action callbackAction)
        {
            EventName = eventName;
            _callbackAction = callbackAction;
        }

        public EventHandler AddArgument(string argName, string argValue)
        {
            _matches.Add($"{argName}={argValue}");
            return this;
        }

        public bool Test(string eventName, object[] args)
        {
            if (!eventName.StartsWith(eventName))
                return false;
            for (var index = 0; index < _matches.Count; index++)
                if (!eventName.Contains(_matches[index]))
                    return false;

            _callbackAction.Invoke();
            return true;
        }
    }
}