namespace Dawn.Preloader.Interfaces;

[InjectInterface("ScanNodeProperties")]
public interface ICRScanNode
{
    object RectTransformInfo { get; set; } // if we wanted to do this instead of ScanNodeAdditionalData for the dictionary.
}