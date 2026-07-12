using System.Collections.Generic;

namespace Dawn;

public interface IAdditionalResolvableKeys
{
    IEnumerable<NamespacedKey> AdditionalResolvableKeys();
}