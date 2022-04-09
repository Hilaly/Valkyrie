namespace Valkyrie.Di
{
    class SingleInstanceResolver : BaseResolver
    {
        private object _createdInstance;

        public SingleInstanceResolver(IRegistrationInfo registrationInfo) : base(registrationInfo)
        {
        }

        public override object Resolve(ResolvingArguments args)
        {
            return _createdInstance ?? (_createdInstance = base.Resolve(args));
        }
    }
}