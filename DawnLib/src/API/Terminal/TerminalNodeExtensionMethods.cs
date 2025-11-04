using Dawn.Interfaces;

namespace Dawn
{
    public static class TerminalNodeExtensionMethods
    {
        public static void SetBuyShipIndex(this TerminalNode node, int index)
        {
            ((ITerminalNodeShipIndex)node).buyShipIndex = index;
        }
    }
}
