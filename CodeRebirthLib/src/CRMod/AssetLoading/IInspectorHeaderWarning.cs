
namespace CodeRebirthLib.CRMod;

public interface IInspectorHeaderWarning
{
    bool TryGetHeaderWarning(out string? message);
}