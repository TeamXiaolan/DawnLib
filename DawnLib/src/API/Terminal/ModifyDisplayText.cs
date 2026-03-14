using System.Collections.Generic;
using System.Linq;
using Dawn.Internal;
using Dawn.Utils;

namespace Dawn;
public class ModifyDisplayText
{
    private static List<ModifyDisplayText> DawnDisplayTextInserts = [];

    public string Word = string.Empty;
    public string TextToLookFor = string.Empty;
    private string _textActuallyAdded = string.Empty;
    internal TerminalNode ResultNode = null!;
    public TerminalNode OriginalNode { get; private set; }
    public IProvider<string> AddedText;

    public Style ModifyStyle = Style.InsertLast;

    /// <summary>This determines your text modification style. (see remarks for more information)</summary>
    /// <remarks>
    /// InsertAll - This will INSERT your AddedText at all locations that match your TextToLookFor property.
    /// InsertFirst - This will INSERT your AddedText at the FIRST location that matches your TextToLookFor property.
    /// InsertLast - This will INSERT your AddedText at the LAST location that matches your TextToLookFor property.
    /// ReplaceAll - This will REPLACE all matching instances of your TextToLookFor property with your AddedText property.
    /// ReplaceFirst - This will REPLACE the FIRST matching instance of your TextToLookFor property with your AddedText property.
    /// ReplaceAll - This will REPLACE the LAST matching instance of your TextToLookFor property with your AddedText property.
    /// </remarks>
    public enum Style
    {
        InsertAll,
        InsertFirst,
        InsertLast,
        ReplaceAll,
        ReplaceFirst,
        ReplaceLast,
    }

    /// <summary>Primary constructor for ModifyDisplayText. Takes a given keyword and modifies the resulting node's displaytext</summary>
    /// <param name="keyword">This is the keyword a user enters to display the TerminalNode</param>
    /// <param name="textToAdd">This is the text you plan to add to the resulting TerminalNode. Can be any type of IProvider<string></param>
    /// <param name="textToLookFor">(Optional) This is the text you are wishing to insert after OR replace, depending on the next parameter</param>
    /// <param name="replaceStyle">(Optional) This determines your modification style. Options are defined under ModifyDisplayText.Style</param>
    /// <remarks>Text modification is done one time at Terminal Start. This does not replace or add to the Terminal.TextPostProcess method</remarks>
    public ModifyDisplayText(string keyword, IProvider<string> textToAdd, string textToLookFor = "", Style replaceStyle = Style.InsertLast)
    {
        Word = keyword;
        AddedText = textToAdd;
        TextToLookFor = textToLookFor;
        ModifyStyle = replaceStyle;

        DawnDisplayTextInserts.Add(this);
    }

    /// <summary>Secondary constructor for ModifyDisplayText. Takes a given TerminalNode and modifies the resulting node's displaytext</summary>
    /// <param name="terminalNode">This is the TerminalNode you wish to modify.</param>
    /// <param name="textToAdd">This is the text you plan to add to the resulting TerminalNode. Can be any type of IProvider<string></param>
    /// <param name="textToLookFor">(Optional) This is the text you are wishing to insert after OR replace, depending on the next parameter</param>
    /// <param name="replaceStyle">(Optional) This determines your modification style. Options are defined under ModifyDisplayText.Style</param>
    /// <remarks>
    /// Text modification is done one time at Terminal Start. This does not replace or add to the Terminal.TextPostProcess method.
    /// When utilizing this secondary constructor, if the TerminalNode is deleted and then re-created your text modifications will not take place.
    /// You may utilize ManualResultNode(TerminalNode terminalNode) to manually update your resulting terminal node if you do not wish to use the primary constructor.
    /// </remarks>
    public ModifyDisplayText(TerminalNode terminalNode, IProvider<string> textToAdd, string textToLookFor = "", Style replaceStyle = Style.InsertLast)
    {
        ResultNode = terminalNode;
        Word = string.Empty;
        AddedText = textToAdd;
        TextToLookFor = textToLookFor;
        ModifyStyle = replaceStyle;

        DawnDisplayTextInserts.Add(this);
    }

    /// <summary>This allows you to disable or re-enable your text modification</summary>
    /// <remarks>
    /// If the text has already been modified this will not remove your modifications at runtime.
    /// This simply removes your text modification from the list of modifications to run at Terminal Start.
    /// </remarks>
    public void ToggleTextInsert(bool value)
    {
        if (value)
        {
            if (!DawnDisplayTextInserts.Contains(this))
                DawnDisplayTextInserts.Add(this);
        }
        else
        {
            DawnDisplayTextInserts.Remove(this);
        }
    }

    /// <summary>This allows you to manually set your Result TerminalNode</summary>
    /// <remarks>
    /// Setting this manually to a non-null result will disable keyword to result node matching.
    /// NOTE: Changing this after Terminal Start has no affect on the result node.
    /// </remarks>
    public void ManualResultNode(TerminalNode terminalNode)
    {
        ResultNode = terminalNode;
    }

    /// <summary>This allows you to manually remove your added text during runtime</summary>
    /// <remarks>
    /// Running this method is not necessary unless there is a desire to remove the modifications before lobby reset.
    /// Text modifications are automatically removed at lobby reset by default.
    /// </remarks>
    public void RemoveAddedTextNow()
    {
        ResetNodeText();
    }

    /// <summary>This allows you to manually refresh your added text during runtime</summary>
    /// <remarks>
    /// Running this method is not necessary unless there is a desire to refresh the text modifications before lobby reset.
    /// Text modifications are automatically removed at lobby reset by default and refreshed at lobby load.
    /// </remarks>
    public void RefreshAddedTextNow()
    {
        ResetNodeText();
        AddText();
    }

    internal static void AddAllTextInserts()
    {
        //remove any duplicates from list
        DawnDisplayTextInserts = [.. DawnDisplayTextInserts.Distinct()];

        foreach(ModifyDisplayText insert in DawnDisplayTextInserts)
        {
            insert.AddText();
        }
    }

    //reset terminal nodes to original status
    internal static void RemoveAllTextInserts()
    {
        foreach (ModifyDisplayText insert in DawnDisplayTextInserts)
        {
            insert.ResetNodeText();
        }
    }

    private void AddText()
    {
        if (ResultNode == null)
        {
            if (!TryGetResultNode(out ResultNode))
                return;
        }

        if (CheckForChanges())
            return;

        _textActuallyAdded = AddedText.Provide();

        if (string.IsNullOrWhiteSpace(TextToLookFor))
        {
            //make sure we're at least adding the new line if someone forgot it
            if (!_textActuallyAdded.EndsWith('\n'))
                _textActuallyAdded += "\n";

            DawnPlugin.Logger.LogMessage($"Appending text to end of {ResultNode.name} - {_textActuallyAdded}");
            ResultNode.displayText += AddedText;
        }
        else
        {
            if (ResultNode.displayText.Contains(TextToLookFor))
            {
                //insert text based on style defined
                InsertTextWithStyle();
            }
            else
            {
                //result displayText does not contain our expected text
                DawnPlugin.Logger.LogWarning($"Unable to insert text after {TextToLookFor} for {ResultNode.name}, provided TextToLookFor does not exist!");
            }
            
        }
    }

    private void InsertTextWithStyle()
    {
        switch (ModifyStyle)
        {
            case Style.InsertAll:
                ResultNode.displayText = ResultNode.displayText.Replace(TextToLookFor, TextToLookFor + _textActuallyAdded);
                DawnPlugin.Logger.LogMessage($"{ResultNode.name} has successfully replaced all instances of {TextToLookFor} with [{TextToLookFor + _textActuallyAdded}]");
                break;

            case Style.InsertFirst:
                ResultNode.displayText = ResultNode.displayText.ReplaceAtFirstIndexOf(TextToLookFor, TextToLookFor + _textActuallyAdded);
                DawnPlugin.Logger.LogMessage($"{ResultNode.name} has successfully replaced the FIRST instance of {TextToLookFor} with [{TextToLookFor + _textActuallyAdded}]");
                break;

            case Style.InsertLast:
                ResultNode.displayText = ResultNode.displayText.ReplaceAtLastIndexOf(TextToLookFor, TextToLookFor + _textActuallyAdded);
                DawnPlugin.Logger.LogMessage($"{ResultNode.name} has successfully replaced the LAST instance of {TextToLookFor} with [{TextToLookFor + _textActuallyAdded}]");
                break;

            case Style.ReplaceAll:
                ResultNode.displayText = ResultNode.displayText.Replace(TextToLookFor, _textActuallyAdded);
                DawnPlugin.Logger.LogMessage($"{ResultNode.name} has successfully replaced all instances of {TextToLookFor} with [{_textActuallyAdded}]");
                break;

            case Style.ReplaceFirst:
                ResultNode.displayText = ResultNode.displayText.ReplaceAtFirstIndexOf(TextToLookFor, _textActuallyAdded);
                DawnPlugin.Logger.LogMessage($"{ResultNode.name} has successfully replaced the FIRST instance of {TextToLookFor} with [{TextToLookFor + _textActuallyAdded}]");
                break;

            case Style.ReplaceLast:
                ResultNode.displayText = ResultNode.displayText.ReplaceAtLastIndexOf(TextToLookFor, _textActuallyAdded);
                DawnPlugin.Logger.LogMessage($"{ResultNode.name} has successfully replaced the LAST instance of {TextToLookFor} with [{TextToLookFor + _textActuallyAdded}]");
                break;
        }
    }

    private void ResetNodeText()
    {
        // result doesn't exist anymore, no need to reset anything
        // OR no text has been added
        if (ResultNode == null || string.IsNullOrEmpty(_textActuallyAdded))
            return;

        DawnPlugin.Logger.LogMessage($"Removing inserted text [{_textActuallyAdded}] from node - {ResultNode.name}");
        ResultNode.displayText = ResultNode.displayText.Replace(_textActuallyAdded, string.Empty);
    }

    private bool CheckForChanges()
    {
        if (string.IsNullOrWhiteSpace(_textActuallyAdded))
            return false;

        return ResultNode.displayText.Contains(_textActuallyAdded);
    }

    private bool TryGetResultNode(out TerminalNode result)
    {
        result = null!;
        if (TerminalRefs.Instance.TryGetKeyword(Word, out TerminalKeyword terminalKeyword))
        {
            if (terminalKeyword.specialKeywordResult == null)
            {
                CompatibleNoun compatibleNoun = terminalKeyword.defaultVerb.compatibleNouns.FirstOrDefault(k => k.noun == terminalKeyword);
                if (compatibleNoun == null)
                    return false;

                result = compatibleNoun.result;
            }
            else
            {
                result = terminalKeyword.specialKeywordResult;
            }
        }

        return result != null;
    }
}