namespace DSL.Actions
{
    interface IDslAction
    {
        
    }

    class SkipAction : IDslAction
    {
        public override string ToString() => "skip";
    }
}