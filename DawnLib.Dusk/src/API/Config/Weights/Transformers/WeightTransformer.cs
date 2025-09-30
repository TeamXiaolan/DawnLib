using System;
using System.Globalization;
using Dawn;
using Dawn.Internal;

namespace Dusk.Weights.Transformers;

[Serializable]
public abstract class WeightTransformer
{
    public abstract string ToConfigString();
    public abstract void FromConfigString(string config);
    public abstract float GetNewWeight(float currentWeight);
    public abstract string GetOperation();

    public float DoOperation(float currentValue, string previousValueWithOperation)
    {
        Debuggers.Weights?.Log($"OperationWithValue: {previousValueWithOperation}");
        string operation = previousValueWithOperation[0..1];
        Debuggers.Weights?.Log($"Operation: {operation}");
        if (float.TryParse(operation, NumberStyles.Float, CultureInfo.InvariantCulture, out float previousValue))
        {
            previousValue = float.Parse(previousValueWithOperation);
            return currentValue + previousValue;
        }

        if (!float.TryParse(previousValueWithOperation[1..], NumberStyles.Float, CultureInfo.InvariantCulture, out previousValue))
        {
            DuskPlugin.Logger.LogWarning($"Invalid operation: {previousValueWithOperation} with WeightTransformer {this}.");
            return currentValue;
        }

        if (operation == "+")
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

    public string Operation(string operation)
    {
        if (operation == "+" || operation == "*" || operation == "/" || operation == "-")
        {
            return operation;
        }
        else if (float.TryParse(operation, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            return "+";
        }
        else
        {
            return string.Empty;
        }
    }
}