namespace Valkyrie.Ecs
{
    public interface IGroupBuilder
    {
        IGroupBuilder AllOf<T0, T1, T2, T3, T4>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct;

        IGroupBuilder AllOf<T0, T1, T2, T3>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct;

        IGroupBuilder AllOf<T0, T1, T2>()
            where T0 : struct
            where T1 : struct
            where T2 : struct;

        IGroupBuilder AllOf<T0, T1>()
            where T0 : struct
            where T1 : struct;

        IGroupBuilder AllOf<T0>()
            where T0 : struct;

        IGroupBuilder NotOf<T0, T1, T2, T3, T4>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct;

        IGroupBuilder NotOf<T0, T1, T2, T3>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct;

        IGroupBuilder NotOf<T0, T1, T2>()
            where T0 : struct
            where T1 : struct
            where T2 : struct;

        IGroupBuilder NotOf<T0, T1>()
            where T0 : struct
            where T1 : struct;

        IGroupBuilder NotOf<T0>()
            where T0 : struct;

        IGroupBuilder AnyOf<T0, T1, T2, T3, T4>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct;

        IGroupBuilder AnyOf<T0, T1, T2, T3>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct;

        IGroupBuilder AnyOf<T0, T1, T2>()
            where T0 : struct
            where T1 : struct
            where T2 : struct;

        IGroupBuilder AnyOf<T0, T1>()
            where T0 : struct
            where T1 : struct;

        IEcsGroup Build();
    }
}