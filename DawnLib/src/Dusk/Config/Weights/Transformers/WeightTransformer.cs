using System;
using System.Globalization;
using Dawn.Internal;

namespace Dawn.Dusk;
[Serializable]
public abstract class WeightTransformer
{
    public abstract string ToConfigString();
    public abstract void FromConfigString(string config);
    public abstract float GetNewWeight(float currentWeight);
    public abstract string GetOperation();

    public float DoOperation(float currentValue, string previousValueWithOperation)
    {
        Debuggers.Weights?.Log($"Operation: {previousValueWithOperation}");
        string operation = previousValueWithOperation[..1];
        if (float.TryParse(operation, NumberStyles.Float, CultureInfo.InvariantCulture, out float previousValue))
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
            DawnPlugin.Logger.LogError($"Unknown operation: {operation} with WeightTransformer {this}.");
            throw new NotImplementedException();
        }
    }
}