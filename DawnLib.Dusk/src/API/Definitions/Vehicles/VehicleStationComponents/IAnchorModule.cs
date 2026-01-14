using System.Collections;

namespace Dusk;
public interface IAnchorModule
{
    IEnumerator EnableAnchor(IVehicle vehicle, IStation station);
    IEnumerator DisableAnchor(IVehicle vehicle, IStation station);
}