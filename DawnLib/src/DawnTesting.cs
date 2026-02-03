using System.Collections.Generic;
using Dawn.Internal;
using Dawn.Utils;
using UnityEngine.Events;

namespace Dawn;

internal class DawnTesting
{
    internal static void TestCommands()
    {
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "version_command"), "DawnLibVersion", builder =>
        {
            builder.SetEnabled(new FuncProvider<bool>(ShouldAddVersion));
            builder.SetMainText(() => $"DawnLib version {MyPluginInfo.PLUGIN_VERSION}\n\n");
            builder.SetKeywords(new SimpleProvider<List<string>>(["dawn version", "version"]));
            builder.SetCategoryName("Test");
            builder.SetDescription("Prints the version of DawnLib");
            builder.SetClearTextFlags(TerminalCommandRegistration.ClearText.Result | TerminalCommandRegistration.ClearText.Query);
        });

        UnityEvent test = new();
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "lights_command"), "DawnLibLights", builder =>
        {
            builder.SetEnabled(new SimpleProvider<bool>(true));
            builder.SetMainText(BasicLightsCommand);
            builder.SetKeywords(new FuncProvider<List<string>>(LightsKeywords));
            builder.SetOverrideKeywords(true);
            builder.SetCategoryName("Test");
            builder.SetDescription("Toggles the lights in the ship.");
            builder.SetCustomBuildEvent(test);
        });
        On.Terminal.Start += (orig, self) =>
        {
            orig(self);
            DawnPlugin.Logger.LogMessage("TEST: Late custom build event!");
            test.Invoke();
        };

        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_input_command"), "DawnLibInputs", builder =>
        {
            builder.SetEnabled(new SimpleProvider<bool>(true));
            builder.SetMainText(InputCommandExample);
            builder.SetKeywords(new SimpleProvider<List<string>>(["input"]));
            builder.SetCategoryName("Test");
            builder.SetAcceptInput(true);
            builder.SetDescription("Takes the player's input and uses it on the next screen.");
        });

        DawnEvent<bool> dawnEvent = new();
        dawnEvent.OnInvoke += continued =>
        {
            if (!continued)
            {
                GameNetworkManager.Instance.localPlayerController.DamagePlayer(50);
            }
        };
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_query_command"), "DawnLibQuery", builder =>
        {
            builder.SetEnabled(new SimpleProvider<bool>(true));
            builder.SetMainText(() => "You have selected, YES!\n\n");
            builder.SetKeywords(new SimpleProvider<List<string>>(["qUEry", "version"]));
            builder.SetCategoryName("Test");
            builder.SetClearTextFlags(TerminalCommandRegistration.ClearText.Query);
            builder.SetDescription("Test query command with added compatible nouns");
            builder.DefineQueryCommand(queryCommandBuilder =>
            {
                queryCommandBuilder.SetQuery(() => "This is a test query, respond [YES] or [NO]\n\n");
                queryCommandBuilder.SetCancel(() => "You have selected, NO!\n\n");
                queryCommandBuilder.SetContinueConditions(new FuncProvider<bool>(() => GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != null));
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

    private static bool ShouldAddVersion()
    {
        return true;
    }

    private static List<string> LightsKeywords()
    {
        return ["lights", "dark", "vow", "help"];
    }
}
