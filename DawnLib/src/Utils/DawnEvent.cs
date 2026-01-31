namespace Dawn;

//delegates for each class type defined here
public delegate void Event();
public delegate void Event<T>(T param);
public delegate void Event<T1, T2>(T1 param1, T2 param2);
public delegate void Event<T1, T2, T3>(T1 param1, T2 param2, T3 param3);
public delegate void Event<T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4);
public delegate void Event<T1, T2, T3, T4, T5>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5);
public delegate void Event<T1, T2, T3, T4, T5, T6>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6);

public class DawnEvent
{
    public event Event OnInvoke = null!;

    public void Invoke()
    {
        OnInvoke?.Invoke();
    }
}

public class DawnEvent<T>
{
    public event Event<T> OnInvoke = null!;

    public void Invoke(T param)
    {
        OnInvoke?.Invoke(param);
    }
}

public class DawnEvent<T1, T2>
{
    public event Event<T1, T2> OnInvoke = null!;

    public void Invoke(T1 param1, T2 param2)
    {
        OnInvoke?.Invoke(param1, param2);
    }
}

public class DawnEvent<T1, T2, T3>
{
    public event Event<T1, T2, T3> OnInvoke = null!;

    public void Invoke(T1 param1, T2 param2, T3 param3)
    {
        OnInvoke?.Invoke(param1, param2, param3);
    }
}

public class DawnEvent<T1, T2, T3, T4>
{
    public event Event<T1, T2, T3, T4> OnInvoke = null!;

    public void Invoke(T1 param1, T2 param2, T3 param3, T4 param4)
    {
        OnInvoke?.Invoke(param1, param2, param3, param4);
    }
}

public class DawnEvent<T1, T2, T3, T4, T5>
{
    public event Event<T1, T2, T3, T4, T5> OnInvoke = null!;

    public void Invoke(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
    {
        OnInvoke?.Invoke(param1, param2, param3, param4, param5);
    }
}

public class DawnEvent<T1, T2, T3, T4, T5, T6>
{
    public event Event<T1, T2, T3, T4, T5, T6> OnInvoke = null!;

    public void Invoke(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
    {
        OnInvoke?.Invoke(param1, param2, param3, param4, param5, param6);
    }
}