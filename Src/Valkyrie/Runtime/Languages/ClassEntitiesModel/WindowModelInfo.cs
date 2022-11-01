using System.Collections.Generic;

namespace Valkyrie
{
    public class WindowModelInfo
    {
        public string Name { get; set; }

        public List<InfoGetter> Bindings = new();
        public List<WindowHandler> Handlers = new();

        public string ClassName => $"{Name}Window";

        public WindowModelInfo AddInfo(string type, string name, string code)
        {
            Bindings.Add(new InfoGetter()
            {
                Code = code,
                Name = name,
                Type = type
            });
            return this;
        }

        public string GetButtonEvent(string buttonName)
        {
            return $"On{buttonName}ButtonAt{Name}Clicked";
        }

        public WindowHandler AddHandler(string name)
        {
            var r = new WindowHandler() { Name = name };
            Handlers.Add(r);
            return r;
        }

        public WindowHandler DefineButton(string buttonName, EventEntity evType)
        {
            var r = AddHandler($"On{buttonName}Clicked");
            r.RaiseOp(evType);
            return r;
        }
    }
}