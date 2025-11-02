namespace Dusk;
static class DuskModConstants
{
    internal const string MenuName = "DawnLib";
    
    // Scriptable Objects
    internal const string Definitions = $"{MenuName}/Definitions";
    internal const string Achievements = $"{MenuName}/Definitions/Achievements";
    internal const string TerminalPredicates = $"{MenuName}/Terminal Predicates";
    internal const string DuskPredicates = $"{MenuName}/Dusk Predicates";
    internal const string PricingStrategies = $"{MenuName}/Pricing Strategies";

    // Components
    internal const string ProgressiveComponents = $"{MenuName}/Progressive";
    
    internal const int PredicateOrder = -50;
    internal const int PricingStrategyOrder = -70;
    internal const int DuskModInfoOrder = -10;
}