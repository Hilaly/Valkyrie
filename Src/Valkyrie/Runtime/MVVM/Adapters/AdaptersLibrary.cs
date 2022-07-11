namespace Valkyrie.MVVM.Adapters
{
    /*
    class AdaptersLibrary : ILibrary
    {
        public void Register(IContainer container)
        {
            var methodInfo = GetType().GetMethod("RegisterAdapter");
            foreach (var type in typeof(IBindingAdapter).GetAllSubTypes(u => !u.IsAbstract))
                methodInfo.MakeGenericMethod(type).Invoke(this, new object[] {container});
        }

        public void RegisterAdapter<T>(IContainer container) where T : IBindingAdapter
        {
            container.Register<T>(typeof(T).FullName).AsInterfacesAndSelf().InstancePerDependency();
        }
    }
    */
}