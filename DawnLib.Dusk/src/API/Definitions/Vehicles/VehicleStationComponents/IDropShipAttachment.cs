using UnityEngine;

namespace Dusk;

public interface IDropShipAttachment
{
    Transform[] RopeAttachmentEndPoints { get; }

    int RealLength { get; }
}