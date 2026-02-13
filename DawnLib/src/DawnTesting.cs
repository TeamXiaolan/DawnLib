using System;
using Dawn.Internal;
using Dawn.Utils;

namespace Dawn;

internal class DawnTesting
{
    internal static void TestCommands()
    {
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "version_command"), "DawnLibVersion", builder =>
        {
            builder.SetMainText(() => $"DawnLib version {MyPluginInfo.PLUGIN_VERSION}\n\n");
            builder.SetKeywords(["dawn version", "version"]);
            builder.SetCategoryName("Test");
            builder.SetDescription("Prints the version of DawnLib");
            builder.SetClearTextFlags(ClearText.Continue | ClearText.Query);
        });

        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "lights_command"), "DawnLibLights", builder =>
        {
            builder.SetMainText(BasicLightsCommand);
            builder.SetKeywords(["lights", "dark", "vow", "help"]);
            builder.SetCategoryName("Test");
            builder.SetDescription("Toggles the lights in the ship.");
        });
        On.Terminal.Start += (orig, self) =>
        {
            orig(self);
            DawnPlugin.Logger.LogMessage("TEST: Late custom build event!");
        };

        /*DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_input_command"), "DawnLibInputs", builder =>
        {
            builder.SetMainText(InputCommandExample);
            builder.SetKeywords(["input"]);
            builder.SetCategoryName("Test");
            builder.SetDescription("Takes the player's input and uses it on the next screen.");
            builder.SetAcceptInput(true);
        });*/

        Action<bool> dawnEvent = new(continued =>
        {
            if (!continued)
            {
                GameNetworkManager.Instance.localPlayerController.DamagePlayer(50);
            }
        });

        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_query_command"), "DawnLibQuery", builder =>
        {
            builder.SetMainText(() => "You have selected, YES!\n\n");
            builder.SetKeywords(["qUEry", "version"]);
            builder.SetCategoryName("Test");
            builder.SetClearTextFlags(ClearText.Query);
            builder.SetDescription("Test query command with added compatible nouns");
            builder.DefineQueryCommand(queryCommandBuilder =>
            {
                queryCommandBuilder.SetContinue("This is a test query, respond [YES] or [NO]\n\n");
                queryCommandBuilder.SetCancel("You have selected, NO!\n\n");
                queryCommandBuilder.SetContinueConditions(new Func<bool>(() => GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != null));
                queryCommandBuilder.SetQueryEvent(dawnEvent);
                queryCommandBuilder.SetContinueWord("yes");
                queryCommandBuilder.SetCancelWord("no");
            });
        });
    }

    private static string BasicLightsCommand()
    {
        StartOfRound.Instance.shipRoomLights.ToggleShipLights();
        if (StartOfRound.Instance.shipRoomLights.areLightsOn)
        {
            return $"Ship Lights are [ON]\n\n";
        }
        else
        {
            return $"Ship Lights are [OFF]\n\n";
        }
    }

    private static string InputCommandExample()
    {
        string cleanedText = TerminalRefs.Instance.screenText.text[^TerminalRefs.Instance.textAdded..];
        if (string.IsNullOrEmpty(TerminalRefs.Instance.GetLastCommand()))
        {
            cleanedText = string.Empty;
        }
        else
        {
            cleanedText = cleanedText.Replace(TerminalRefs.Instance.GetLastCommand(), "").Trim();
        }
        return $"This is a test command displaying user input after the command.\n\nUser input is [ {cleanedText} ]\n\n";
    }
}
