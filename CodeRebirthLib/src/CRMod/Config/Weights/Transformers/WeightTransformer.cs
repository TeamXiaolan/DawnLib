using System;

namespace CodeRebirthLib.CRMod;
[Serializable]
public abstract class WeightTransformer
{
    public abstract string ToConfigString();
    public abstract void FromConfigString(string config);
    public abstract float GetNewWeight(float currentWeight);
    public abstract string GetOperation();

    public float DoOperation(float currentValue, string previousValueWithOperation)
    {
        // first character is the operation, get that as string?
        string operation = previousValueWithOperation[..1];
        float previousValue = int.Parse(previousValueWithOperation[1..]);
        // parse everything else as int
        if (int.TryParse(operation, out _)) // if no operation provided, default to `+`
        {
            previousValue = int.Parse(previousValueWithOperation);
            return currentValue + previousValue;
        }
        else if (operation == "+")
        {
            return currentValue + previousValue;
        }
        else if (operation == "*")
        {
            return currentValue * previousValue;
        }
        else if (operation == "-")
        {
            return currentValue - previousValue;
        }
        else if (operation == "/")
        {
            if (previousValue == 0)
            {
                return 0;
            }
            return currentValue / previousValue;
        }
        else
        {
            CodeRebirthLibPlugin.Logger.LogError($"Unknown operation: {operation} with WeightTransformer {this}.");
            throw new NotImplementedException();
        }
    }
}