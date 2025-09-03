namespace Dawn;
public interface IPredicate
{
    bool Evaluate();
}

public class ConstantPredicate : IPredicate
{
    public static readonly ConstantPredicate True = new(true);
    public static readonly ConstantPredicate False = new(false);

    private bool _value;
        
    private ConstantPredicate(bool value)
    {
        _value = value;
    }


    public bool Evaluate()
    {
        return _value;
    }
}