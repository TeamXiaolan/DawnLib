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

    public enum Style
    {
        InsertAll,
        InsertFirst,
        InsertLast,
        ReplaceAll,
        ReplaceFirst,
        ReplaceLast,
    }

    public ModifyDisplayText(string keyword, IProvider<string> textToAdd, string textToLookFor = "", Style replaceStyle = Style.InsertLast)
    {
        Word = keyword;
        AddedText = textToAdd;
        TextToLookFor = textToLookFor;
        ModifyStyle = replaceStyle;

        DawnDisplayTextInserts.Add(this);
    }

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