using UnityEngine;

namespace Dawn;

public interface ILoadingContext
{
    void SetBackgroundColor(Color color);
    void SetMainText(string text);
    void SetSecondaryText(string text);
    void SetMainTextColor(Color startColor, Color endColor);
    void SetSecondaryTextColor(Color color);
}