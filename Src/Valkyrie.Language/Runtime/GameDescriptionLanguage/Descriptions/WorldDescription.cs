using System.Collections.Generic;
using System.IO;
using System.Linq;
using Valkyrie.Language.Description.Utils;

namespace Valkyrie.Language.Description
{
    public class WorldDescription
    {
        public List<ComponentDescription> Components { get; } = new List<ComponentDescription>();
        public List<MethodsScope> InitMethods { get; } = new List<MethodsScope>();
        public List<ISimPart> SimulationMethods { get; } = new List<ISimPart>();
        public List<ViewScope> Views { get; } = new List<ViewScope>();

        public override string ToString()
        {
            var sb = new FormatWriter();

            sb.AppendLine("using Valkyrie.Ecs;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();

            var rootNamespace = "Test";
            
            sb.BeginBlock($"namespace {rootNamespace}");
            WriteWorldSimulation(sb);
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock($"namespace {rootNamespace}");
            sb.AppendLine("#region Components");
            sb.AppendLine();
            foreach (var componentDescription in Components)
                WriteComponent(sb, componentDescription);
            sb.AppendLine();
            sb.AppendLine("#endregion //Components");
            sb.AppendLine();
            sb.AppendLine("#region Simulation hooks");
            sb.AppendLine();
            foreach (var directive in SimulationMethods.OfType<SimulateDirective>())
                WriteSimulationInterface(sb, directive);
            sb.AppendLine();
            sb.AppendLine("#endregion //Simulation hooks");
            sb.AppendLine();
            sb.AppendLine("#region View structs");
            sb.AppendLine();
            foreach (var view in Views) 
                WriteWorldViewStruct(sb, view);
            sb.AppendLine();
            sb.AppendLine("#endregion //View structs");
            sb.EndBlock();

            return sb.ToString();
        }

        private void WriteWorldViewStruct(FormatWriter sb, ViewScope viewScope)
        {
            sb.BeginBlock($"public struct {viewScope.Name}");
            foreach (var viewProperty in viewScope.Properties)
                sb.AppendLine($"public {viewProperty.Field.Type} {viewProperty.Field.Name};");
            sb.EndBlock();
        }

        private void WriteSimulationInterface(FormatWriter sb, SimulateDirective directive)
        {
            sb.BeginBlock($"public interface {directive.GetTypeName()}");
            sb.AppendLine($"void Simulate(float dt, IEcsState state);");
            sb.EndBlock();
        }

        private void WriteComponent(FormatWriter sb, ComponentDescription componentDescription)
        {
            sb.BeginBlock($"public struct {componentDescription.GetTypeName()}");
            foreach (var field in componentDescription.Fields)
                sb.AppendLine($"public {field.Type ?? "object"} {field.Name};");
            sb.EndBlock();
        }

        private void WriteWorldSimulation(FormatWriter sb)
        {
            sb.BeginBlock($"public interface IWorldView");
            sb.AppendLine("IEcsState State { get; }");
            sb.AppendLine("IEcsGroups Groups { get; }");
            sb.AppendLine();
            foreach (var view in Views)
                sb.AppendLine($"public List<{view.Name}> Get{view.Name}(List<{view.Name}> buffer);");
            sb.EndBlock();
            sb.AppendLine();
            
            sb.BeginBlock("public interface IWorldSimulation");
            sb.AppendLine("void Initialize();");
            sb.AppendLine("void Simulate(float dt);");
            foreach (var directive in SimulationMethods.OfType<SimulateDirective>())
                sb.AppendLine($"void Register({directive.GetTypeName()} simulationPart);");
            sb.EndBlock();
            sb.AppendLine();

            sb.BeginBlock("public class WorldSimulation : IWorldSimulation, IWorldView");
            //Fields
            sb.AppendLine($"private readonly IEcsWorld _ecsWorld;");
            //Properties
            sb.AppendLine($"public IEcsState State => _ecsWorld.State;");
            sb.AppendLine($"public IEcsGroups Groups => _ecsWorld.Groups;");
            //Ctor
            sb.AppendLine();
            sb.BeginBlock("public WorldSimulation(IEcsWorld ecsWorld)");
            sb.AppendLine("_ecsWorld = ecsWorld;");
            sb.EndBlock();
            //Methods
            //Init
            sb.AppendLine();
            sb.BeginBlock("public void Initialize()");
            foreach (var methodsScope in InitMethods)
                WriteMethod(sb, methodsScope);
            foreach (var directive in SimulationMethods.OfType<SimulateDirective>())
            {
                sb.BeginBlock($"// {directive.Name}");
                sb.BeginBlock($"for(var index = 0; index < {directive.GetListName()}.Count; ++index)");
                sb.AppendLine($"if({directive.GetListName()}[index] is Valkyrie.Ecs.IEcsInitSystem initValue) initValue.Init();");
                sb.EndBlock();
                sb.EndBlock();
            }
            sb.EndBlock();
            //Simulate
            sb.AppendLine();
            sb.BeginBlock("public void Simulate(float dt)");
            foreach (var methodsScope in SimulationMethods)
                WriteMethod(sb, methodsScope);
            sb.AppendLine();
            sb.AppendLine("Cleanup();");
            sb.EndBlock();
            //Cleanup
            sb.AppendLine();
            sb.BeginBlock("private void Cleanup()");
            sb.AppendLine("//TODO: place clean events methods here");
            sb.EndBlock();

            //Hooks here
            sb.AppendLine("#region Simulation hooks");
            sb.AppendLine();
            foreach (var directive in SimulationMethods.OfType<SimulateDirective>())
            {
                sb.AppendLine($"private readonly List<{directive.GetTypeName()}> {directive.GetListName()} = new List<{directive.GetTypeName()}>();");
                sb.AppendLine();
                sb.BeginBlock($"public void Register({directive.GetTypeName()} simulationPart)");
                sb.AppendLine($"{directive.GetListName()}.Add(simulationPart);");
                sb.EndBlock();
            }

            sb.AppendLine();
            sb.AppendLine("#endregion //Simulation hooks");
            
            //View Methods
            sb.AppendLine("#region View getters");
            sb.AppendLine();
            foreach (var view in Views)
            {
                sb.BeginBlock($"public List<{view.Name}> Get{view.Name}(List<{view.Name}> buffer)");
                WriteViewMethod(sb, view);
                sb.AppendLine("return buffer;");
                sb.EndBlock();
            }
            sb.AppendLine();
            sb.AppendLine("#endregion //View getters");

            sb.EndBlock();
        }

        private void WriteMethod(FormatWriter sb, ISimPart scope)
        {
            switch (scope)
            {
                case MethodsScope ms:
                    WriteMethod(sb, ms);
                    break;
                case SimulateDirective sd:
                    WriteSimulateDirective(sb, sd);
                    break;
                default:
                    sb.AppendLine($"#error unsupported node: {scope.GetType().Name}");
                    break;
            }
        }

        private void WriteSimulateDirective(FormatWriter sb, SimulateDirective directive)
        {
            sb.BeginBlock($"// {directive.Name}");
            sb.BeginBlock($"for(var index = 0; index < {directive.GetListName()}.Count; ++index)");
            sb.AppendLine($"{directive.GetListName()}[index].Simulate(dt, State);");
            sb.EndBlock();
            sb.EndBlock();
        }

        private void WriteViewMethod(FormatWriter sb, ViewScope scope)
        {
            var blocksCount = WriteDependentMethodStart(sb, scope);

            sb.AppendLine($"buffer.Add(new {scope.Name} {{");
            sb.AddTab();
            var strProps = string.Join(", ", scope.Properties.Select(scopeProperty => $"{scopeProperty.Field.Name} = {scopeProperty.Op}"));
            sb.AppendLine(strProps);
            sb.RemoveTab();
            sb.AppendLine("});");

            WriteDependentMethodEnd(sb, blocksCount);
        }

        private void WriteMethod(FormatWriter sb, MethodsScope scope)
        {
            var blocksCount = WriteDependentMethodStart(sb, scope);

            //Do creation
            foreach (var method in scope.Methods)
            {
                sb.BeginBlock($"// {method.Component.GetTypeName()}");
                sb.AppendLine($"var entityId = {method.EntityIdExpr};");
                var argsGeneration = string.Empty;
                if (method.Arguments.Count > 0)
                {
                    argsGeneration += "{ ";
                    for (var index = 0; index < method.Arguments.Count; index++)
                    {
                        var argumentString = method.Arguments[index];
                        argsGeneration += method.Component.Fields[index].Name + " = " + argumentString;
                        if (index < method.Arguments.Count - 1)
                            argsGeneration += ", ";
                    }

                    argsGeneration += " }";
                }

                sb.AppendLine($"State.Add(entityId, new {method.Component.GetTypeName()}(){argsGeneration});");
                sb.EndBlock();
            }

            WriteDependentMethodEnd(sb, blocksCount);
        }

        private static void WriteDependentMethodEnd(FormatWriter sb, int blocksCount)
        {
            //Close filters
            for (var i = 0; i < blocksCount; ++i) sb.EndBlock();

            sb.EndBlock();
        }

        private static int WriteDependentMethodStart(FormatWriter sb, DependentScope scope)
        {
            sb.BeginBlock();
            //Define local variables
            foreach (var localVariable in scope.LocalVariables.Variables.Where(x => !x.DefineExternal))
                sb.AppendLine($"{localVariable.FieldDescription.Type} {localVariable.Name};");
            //Write filters
            var blocksCount = scope.Filters.Count;
            foreach (FactsFilterMethodDescription scopeFilter in scope.Filters)
            {
                sb.AppendLine($"// IsReference: {scopeFilter.IsReference}");

                if (scopeFilter.IsReference)
                {
                    foreach (var filterComponent in scopeFilter.Components)
                        sb.AppendLine($"if(!State.Has<{filterComponent.GetTypeName()}>({scopeFilter.Name})) continue;");
                    sb.BeginBlock();
                }
                else
                {
                    var collection =
                        $"Groups.Build().AllOf<{string.Join(",", scopeFilter.Components.Select(f => f.GetTypeName()))}>().Build().GetEntities(new List<int>())";
                    sb.BeginBlock(
                        $"foreach (var {scopeFilter.Name} in {collection})");
                }

                foreach (var strOp in scopeFilter.Operators)
                    sb.AppendLine(strOp);
            }

            return blocksCount;
        }

        public ComponentDescription GetOrCreateComponent(string name, int argNodesCount)
        {
            var result = Components.Find(x => x.Name == name);
            if (result == null)
            {
                Components.Add(result = new ComponentDescription() { Name = name });
                for (var i = 0; i < argNodesCount; ++i)
                    result.Fields.Add(new FieldDescription
                    {
                        Name = $"Field{i}",
                        Type = FactsCompiler.AnyName
                    });
            }
            else if (result.Fields.Count != argNodesCount)
                throw new InvalidDataException(
                    $"Component {name} has {result.Fields.Count} fields, but requested with {argNodesCount}");

            return result;
        }
    }
}