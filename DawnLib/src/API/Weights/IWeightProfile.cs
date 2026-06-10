using System;

namespace Dawn;

public interface IWeightProfile
{
    Type ValueType { get; }

    void Refresh();
}