using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Valkyrie.Cheats
{
    abstract class CheatDescription
    {
        public CheatAttribute Attribute;
    }

    class StaticPropertyCheat : CheatDescription
    {
        public PropertyInfo PropertyInfo;
    }

    public class CheatsService
    {
        static List<Type> GetAllSubTypes(Type aBaseClass, Func<Type, bool> where)
        {
            var result = new List<Type>
            {
                aBaseClass
            };
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var assemblyTypes = assembly.GetTypes();
                    var selectedTypes = assemblyTypes
                        .Where(typ => typ.IsSubclassOf(aBaseClass) || aBaseClass.IsAssignableFrom(typ)).ToArray();
                    result.AddRange(selectedTypes);
                }
                catch
                {
                    //Do nothing if we got to assembly that probably not from this project
                }
            }

            return where != null ? result.Where(where).ToList() : result;
        }

        List<CheatDescription> CollectAllStaticCheats()
        {
            var descriptions = new List<CheatDescription>();

            var allTypes = GetAllSubTypes(typeof(object), x => true);
            foreach (var type in allTypes)
            {
                var allStaticGetSetProperties = type.GetProperties(BindingFlags.Static |
                                                                   BindingFlags.DeclaredOnly |
                                                                   BindingFlags.GetProperty |
                                                                   BindingFlags.SetProperty);
                foreach (var propertyInfo in allStaticGetSetProperties)
                {
                    var attributes = propertyInfo.GetCustomAttributes<CheatAttribute>();
                    foreach (var cheatAttribute in attributes)
                    {
                        descriptions.Add(new StaticPropertyCheat()
                            { Attribute = cheatAttribute, PropertyInfo = propertyInfo });
                    }
                }
            }

            return descriptions;
        }
    }
}