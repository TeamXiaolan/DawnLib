namespace Dawn;

public interface IOperationWithValue
{
    abstract MathOperation Operation { get; }
    abstract float Value { get; }
}