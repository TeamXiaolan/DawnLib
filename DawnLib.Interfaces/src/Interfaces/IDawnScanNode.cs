using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(ScanNodeProperties))]
public interface IDawnScanNode
{
    object RectTransformInfo { get; set; } // if we wanted to do this instead of ScanNodeAdditionalData for the dictionary.
}