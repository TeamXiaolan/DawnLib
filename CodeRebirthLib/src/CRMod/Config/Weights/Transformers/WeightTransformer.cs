using System;
using System.Globalization;
using CodeRebirthLib.Internal;

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
        Debuggers.Weights?.Log($"Operation: {previousValueWithOperation}");
        string operation = previousValueWithOperation[..1];
        // parse everything else as int
        if (float.TryParse(operation, NumberStyles.Float, CultureInfo.InvariantCulture, out float previousValue)) // if no operation provided, default to `+`
        {
            previousValue = float.Parse(previousValueWithOperation);
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