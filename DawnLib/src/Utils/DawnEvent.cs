using System;
using System.Collections.Generic;

public abstract class DawnEventBase
{
    private readonly List<object> Listeners = [];
    public bool HasListeners()
    {
        return GetListenerCount() != 0;
    }
    public int GetListenerCount()
    {
        return Listeners.Count;
    }

    public void AddListener(Delegate listener)
    {
        Listeners.Add(listener);
    }

    public void RemoveListener(Delegate listener)
    {
        Listeners.Remove(listener);
    }

    //safely cast object list to remove any unexpected types
    internal List<T> CastListenersTo<T>()
    {
        List<T> result = [];

        foreach (object listener in Listeners)
        {
            if (listener is T casted)
                result.Add(casted);
        }

        return result;
    }
}

public class DawnEvent : DawnEventBase
{
    public void Invoke()
    {
        foreach(Action action in CastListenersTo<Action>())
        {
            action?.Invoke();
        }
    }
}

public class DawnEvent<T> : DawnEventBase
{
    public void Invoke(T param)
    {
        foreach (Action<T> action in CastListenersTo<Action<T>>())
        {
            action?.Invoke(param);
        }
    }
}

public class DawnEvent<T1, T2> : DawnEventBase
{
    public void Invoke(T1 param1, T2 param2)
    {
        foreach (Action<T1, T2> action in CastListenersTo<Action<T1, T2>>())
        {
            action?.Invoke(param1, param2);
        }
    }
}

public class DawnEvent<T1, T2, T3> : DawnEventBase
{
    public void Invoke(T1 param1, T2 param2, T3 param3)
    {
        foreach (Action<T1, T2, T3> action in CastListenersTo<Action<T1, T2, T3>>())
        {
            action?.Invoke(param1, param2, param3);
        }
    }
}

public class DawnEvent<T1, T2, T3, T4> : DawnEventBase
{
    public void Invoke(T1 param1, T2 param2, T3 param3, T4 param4)
    {
        foreach (Action<T1, T2, T3, T4> action in CastListenersTo<Action<T1, T2, T3, T4>>())
        {
            action?.Invoke(param1, param2, param3, param4);
        }
    }
}

public class DawnEvent<T1, T2, T3, T4, T5> : DawnEventBase
{
    public void Invoke(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
    {
        foreach (Action<T1, T2, T3, T4, T5> action in CastListenersTo<Action<T1, T2, T3, T4, T5>>())
        {
            action?.Invoke(param1, param2, param3, param4, param5);
        }
    }
}

//DawnEventBase can be expanded further from the 6 different examples above
//Requires a delegate provided via AddListener that can be invoked from a list after a successful cast