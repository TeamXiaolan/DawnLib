using System;

namespace Dawn;

public class VanillaTerminalEvents
{
    public static Action<Terminal, TerminalNode> SetUpTerminalEvent => SetUpTerminal;
    public static Action<Terminal, TerminalNode> Cheat_ResetCreditsEvent => Cheat_ResetCredits;
    public static Action<Terminal, TerminalNode> SwitchCameraEvent => SwitchCamera;
    public static Action<Terminal, TerminalNode> EjectPlayersEvent => EjectPlayers;

    private static void SetUpTerminal(Terminal terminal, TerminalNode terminalNode)
    {
        ES3.Save("HasUsedTerminal", true, "LCGeneralSaveData");
    }

    private static void Cheat_ResetCredits(Terminal terminal, TerminalNode node)
    {
        if ((GameNetworkManager.Instance.localPlayerController.playerUsername == "Zeekerss" || GameNetworkManager.Instance.localPlayerController.playerUsername == "Blueray" || GameNetworkManager.Instance.localPlayerController.playerUsername == "Puffo") && GameNetworkManager.Instance.localPlayerController.IsServer)
        {
            terminal.useCreditsCooldown = true;
            terminal.groupCredits = 2500;
            terminal.SyncGroupCreditsServerRpc(terminal.groupCredits, terminal.numberOfItemsInDropship);
        }
    }

    private static void SwitchCamera(Terminal terminal, TerminalNode node)
    {
        StartOfRound.Instance.mapScreen.SwitchRadarTargetForward(true);
    }

    private static void EjectPlayers(Terminal terminal, TerminalNode node)
    {
        if (terminal.IsServer && !StartOfRound.Instance.isChallengeFile && StartOfRound.Instance.inShipPhase && !StartOfRound.Instance.firingPlayersCutsceneRunning)
        {
            StartOfRound.Instance.ManuallyEjectPlayersServerRpc();
        }
    }
}