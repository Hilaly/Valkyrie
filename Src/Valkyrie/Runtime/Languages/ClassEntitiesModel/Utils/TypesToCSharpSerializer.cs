using System.Linq;
using Configs;
using Meta.Inventory;
using Utils;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie
{
    static partial class TypesToCSharpSerializer
    {
        private static string BaseProfileInterface => typeof(BaseInventoryItem).FullName;

        public static void WriteViewModelTimer(string timer, FormatWriter sb)
        {
            sb.AppendLine(
                $"[{typeof(BindingAttribute).FullName}] public bool HasTimer{timer} => Model.{timer} != null;");
            sb.AppendLine(
                $"[{typeof(BindingAttribute).FullName}] public float {timer}TimeLeft => Model.{timer}?.TimeLeft ?? 0f;");
            sb.AppendLine(
                $"[{typeof(BindingAttribute).FullName}] public float {timer}Time => {timer}FullTime - {timer}TimeLeft;");
            sb.AppendLine(
                $"[{typeof(BindingAttribute).FullName}] public float {timer}FullTime => Model.{timer}?.FullTime ?? 1f;");
            sb.AppendLine(
                $"[{typeof(BindingAttribute).FullName}] public float {timer}Progress => Mathf.Clamp01({timer}Time / {timer}FullTime);");
        }

        public static void WriteInterfaceTimer(string timer, FormatWriter sb)
        {
            sb.AppendLine($"ITimer {timer} {{ get; }}");
            sb.AppendLine($"void Start{timer}(float time);");
            sb.AppendLine($"void Stop{timer}();");
            sb.AppendLine($"bool {timer}JustFinished {{ get; }}");
        }

        public static void Write(this BaseTypeProperty property, FormatWriter sb) =>
            sb.AppendLine($"public {property.GetMemberType()} {property.Name} {{ get; set; }}");

        public static void WriteAsField(this BaseTypeProperty property, FormatWriter sb) =>
            sb.AppendLine($"public {property.GetMemberType()} {property.Name};");

        public static void WriteTypeClass(this BaseType baseType, FormatWriter sb)
        {
            var blockName = $"public partial class {baseType.Name} : {typeof(IEntity).FullName}";
            if (baseType.BaseTypes.Count > 0)
                blockName += ", " + string.Join(", ", baseType.BaseTypes.Select(x => x.Name));
            sb.BeginBlock(blockName);

            foreach (var property in baseType.GetAllProperties(true))
                property.Write(sb);
            foreach (var property in baseType.GetAllInfos(true))
                sb.AppendLine($"public {property.GetMemberType()} {property.Name} => {property.Code};");

            var timers = baseType.GetAllTimers();
            foreach (var timer in timers)
            {
                sb.AppendLine($"private EntityTimer {timer.ConvertToCamelCaseFieldName()};");
                sb.AppendLine(
                    $"public ITimer {timer} => {timer.ConvertToCamelCaseFieldName()} is {{ TimeLeft: > 0 }} ? {timer.ConvertToCamelCaseFieldName()} : {timer.ConvertToCamelCaseFieldName()} = default;");
                sb.BeginBlock($"public void Start{timer}(float time)");
                sb.AppendLine($"if ({timer} != null) throw new Exception(\"Timer {timer} already exist\");");
                sb.AppendLine($"{timer.ConvertToCamelCaseFieldName()} = new EntityTimer(time);");
                sb.EndBlock();
                sb.AppendLine($"public void Stop{timer}() => {timer.ConvertToCamelCaseFieldName()} = default;");
                sb.AppendLine($"public bool {timer}JustFinished {{ get; private set; }}");
            }

            if (timers.Any())
            {
                sb.BeginBlock("internal void AdvanceTimers(float dt)");
                foreach (var timer in timers)
                {
                    sb.AppendLine($"{timer}JustFinished = false;");
                    sb.BeginBlock($"if({timer.ConvertToCamelCaseFieldName()} != null)");
                    sb.AppendLine($"{timer.ConvertToCamelCaseFieldName()}.Advance(dt);");
                    sb.BeginBlock($"if({timer.ConvertToCamelCaseFieldName()}.TimeLeft <= 0)");
                    sb.AppendLine($"{timer.ConvertToCamelCaseFieldName()} = default;");
                    sb.AppendLine($"{timer}JustFinished = true;");
                    sb.EndBlock();
                    sb.EndBlock();
                }

                sb.EndBlock();
            }

            sb.EndBlock();

            WriteViewModels(baseType, sb);
        }

        public static void WriteTypeInterface(this BaseType baseType, FormatWriter sb)
        {
            var blockName = $"public interface {baseType.Name} : {typeof(IEntity).FullName}";
            if (baseType.BaseTypes.Count > 0)
                blockName += ", " + string.Join(", ", baseType.BaseTypes.Select(x => x.Name));
            sb.BeginBlock(blockName);

            foreach (var property in baseType.Properties)
                property.Write(sb);

            foreach (var timer in baseType.Timers)
                WriteInterfaceTimer(timer, sb);

            foreach (var info in baseType.Infos)
                sb.AppendLine($"public {info.GetMemberType()} {info.Name} {{ get; }}");

            sb.EndBlock();

            WriteViewModels(baseType, sb);
        }

        public static void WriteInventoryClass(this BaseType baseType, FormatWriter sb)
        {
            var propertyAttributes = baseType.Attributes.Contains("view")
                ? $"[{typeof(BindingAttribute).FullName}] "
                : string.Empty;
            var blockName = $"{propertyAttributes}public partial class {baseType.Name} : ";
            if (baseType.BaseTypes.Count > 0)
                blockName += baseType.BaseTypes.Select(x => x.Name).Join(", ");
            else
                blockName += BaseProfileInterface;
            sb.BeginBlock(blockName);

            foreach (var property in baseType.Properties)
                property.Write(sb);

            sb.EndBlock();
        }

        public static void WriteWindow(this WindowType baseType, FormatWriter sb)
        {
            sb.AppendLine($"[{typeof(BindingAttribute).FullName}]");
            sb.BeginBlock($"public partial class {baseType.ClassName} : ProjectWindow");
            foreach (var getter in baseType.Bindings)
                sb.AppendLine(
                    $"[{typeof(BindingAttribute).FullName}] public {getter.Type} {getter.Name} => {getter.Code};");
            sb.AppendLine();
            foreach (var handler in baseType.Handlers)
            {
                sb.BeginBlock($"[{typeof(BindingAttribute).FullName}] public async void {handler.Name}()");
                handler.Write(sb);
                sb.EndBlock();
            }

            sb.EndBlock();
        }
    }
}