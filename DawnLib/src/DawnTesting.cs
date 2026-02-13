using System;
using Dawn.Internal;
using Dawn.Utils;

namespace Dawn;

internal class DawnTesting
{
    internal static void TestCommands()
    {
        TerminalCommandBasicInformation versionCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibVersionCommand", "Test", "Prints the version of DawnLib!", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "version_command"), versionCommandBasicInformation, builder =>
        {
            builder.SetKeywords(["version"]);
            builder.DefineSimpleCommand(simpleBuilder =>
            {
                simpleBuilder.SetResult(() => $"DawnLib version {MyPluginInfo.PLUGIN_VERSION}");
            });
        }); // Simple command

        TerminalCommandBasicInformation lightsCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibLightsCommand", "Test", "Toggles the lights in the ship.", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "lights_command"), lightsCommandBasicInformation, builder =>
        {
            builder.SetKeywords(["lights", "dark", "vow", "help"]);
            builder.DefineSimpleCommand(simpleBuilder =>
            {
                simpleBuilder.SetResult(BasicLightsCommand);
            });
        }); // Simple Command

        TerminalCommandBasicInformation complexCommandExampleBasicInformation = new TerminalCommandBasicInformation("DawnLibComplexCommand", "Test", "Test complex command", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_complex_command"), complexCommandExampleBasicInformation, builder =>
        {
           builder.SetKeywords(["complex"]);
           builder.DefineComplexCommand(complexBuilder =>
           {
               complexBuilder.SetSecondaryKeywords(["first, second, third"]);
               complexBuilder.SetResults(["result 1", "result 2, result 3"]);
           });
        }); // Complex Command

        TerminalCommandBasicInformation inputCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibInputs", "Test", "Takes the player's input and uses it on the next screen.", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_input_command"), inputCommandBasicInformation, builder =>
        {
            builder.SetKeywords(["input"]);
            builder.DefineInputCommand(inputBuilder =>
            {
                inputBuilder.SetResult(InputCommandExample);
                inputBuilder.SetAcceptInput(true); // todo: make this automatic
            });
        });


        Action<bool> dawnEvent = new(continued =>
        {
            if (!continued)
            {
                GameNetworkManager.Instance.localPlayerController.DamagePlayer(50);
            }
        });
        TerminalCommandBasicInformation queryCommandBasicInformation = new TerminalCommandBasicInformation("DawnLibQueryCommand", "Test", "Test query command with added compatible nouns", ClearText.Result | ClearText.Query);
        DawnLib.DefineTerminalCommand(NamespacedKey<DawnTerminalCommandInfo>.From("dawn_lib", "test_query_command"), queryCommandBasicInformation, builder =>
        {
            builder.SetKeywords(["qUEry", "version"]);
            builder.DefineSimpleQueryCommand(queryCommandBuilder =>
            {
                queryCommandBuilder.SetResult(() => "You have selected, YES!\n\n");
                queryCommandBuilder.SetContinueOrCancel("This is a test query, respond [YES] or [NO]\n\n");
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
