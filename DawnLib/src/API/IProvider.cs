namespace Dawn;
public interface IProvider<out T>
{
    T Provide();
}

public class SimpleProvider<T>(T value) : IProvider<T>
{
    public T Provide() => value;
}