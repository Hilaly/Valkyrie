using System.Collections.Generic;
using Utils;
using Valkyrie.Language.Description.Utils;

namespace Valkyrie
{
    public class WindowModelInfo
    {
        public string Name { get; set; }

        public List<InfoGetter> Bindings = new();
        private List<WindowHandler> Handlers = new();

        public string ClassName => $"{Name}Window";

        public void Write(FormatWriter sb)
        {
            sb.AppendLine($"[{typeof(BindingAttribute).FullName}]");
            sb.BeginBlock($"public partial class {ClassName} : ProjectWindow");
            foreach (var getter in Bindings)
                sb.AppendLine(
                    $"[{typeof(BindingAttribute).FullName}] public {getter.Type} {getter.Name} => {getter.Code};");
            sb.AppendLine();
            foreach (var handler in Handlers)
            {
                sb.BeginBlock($"[{typeof(BindingAttribute).FullName}] public async void {handler.Name}()");
                handler.Write(sb);
                sb.EndBlock();
            }

            sb.EndBlock();
        }

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