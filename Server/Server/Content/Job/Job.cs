namespace Server.Content.Job;

public interface IJob
{
    void Execute();
}

public class Job : IJob
{
    private Action _action;

    public Job(Action action) => _action = action;

    public void Execute() => _action.Invoke();
}

public class Job<T> : IJob
{
    private Action<T> _action;
    private T _param;

    public Job(Action<T> action, T param)
    {
        _action = action;
        _param = param;
    }

    public void Execute() => _action.Invoke(_param);
}

public class Job<T1, T2> : IJob
{
    private Action<T1, T2> _action;
    private T1 _param1;
    private T2 _param2;

    public Job(Action<T1, T2> action, T1 param1, T2 param2)
    {
        _action = action;
        _param1 = param1;
        _param2 = param2;
    }

    public void Execute() => _action.Invoke(_param1, _param2);
}

public class Job<T1, T2, T3> : IJob
{
    private Action<T1, T2, T3> _action;
    private T1 _param1;
    private T2 _param2;
    private T3 _param3;

    public Job(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
    {
        _action = action;
        _param1 = param1;
        _param2 = param2;
        _param3 = param3;
    }

    public void Execute() => _action.Invoke(_param1, _param2, _param3);
}
