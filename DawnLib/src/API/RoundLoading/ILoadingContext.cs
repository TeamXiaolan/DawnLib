using UnityEngine;

namespace Dawn;

public interface ILoadingContext
{
    void SetText(string text);
    void SetColor(Color color);
}