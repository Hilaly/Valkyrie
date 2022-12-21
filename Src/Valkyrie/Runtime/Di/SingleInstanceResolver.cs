namespace Valkyrie.Di
{
    class SingleInstanceResolver : BaseResolver
    {
        private object _createdInstance;

        public SingleInstanceResolver(IRegistrationInfo registrationInfo) : base(registrationInfo)
        {
        }

        public override object Resolve(ResolvingArguments args) => _createdInstance ??= base.Resolve(args);
    }
}