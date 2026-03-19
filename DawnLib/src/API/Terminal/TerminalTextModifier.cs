using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dawn.Internal;
using Dawn.Utils;

namespace Dawn;

/// <summary>
/// The match "index" for non-regex text modifications
/// </summary>
/// <remarks>
/// First finds the index of the first match.
/// Last finds the index of the last match.
/// All finds every index of the match.
/// </remarks>
public enum MatchIndex
{
    First,
    Last,
    All
}

/// <summary>
/// The match "insert style" for non-regex text modifications
/// </summary>
/// <remarks>
/// ReplaceMatch will just replace the matching text with the specified text.
/// Before will insert the specified text before the matching text.
/// After will insert the specified text after the matching text.
/// </remarks>
[Flags]
public enum MatchInsert
{
    ReplaceMatch = 0,
    Before = 1 << 0,
    After = 1 << 1
}

/// <summary>
/// This class is used to modify TerminalNode display text after Terminal.TextPostProcess, which is run every time a node is loaded.
/// It will safely modify the resulting display text without modifying the node's displaytext directly.
/// </summary>
public class TerminalTextModifier
{
    // required for this class
    private string TextToFind;
    private IProvider<string> AddedTextProvider;

    // optional stuff, defaults to replace every matching text in every node (null NodeToProcess & NodeKeyword), not using Regex
    private bool RegexPattern = false;
    private TerminalNode? NodeToProcess;
    private string? NodeKeyword;
    private MatchIndex IndexStyle = MatchIndex.All;
    private MatchInsert InsertStyle = MatchInsert.ReplaceMatch;


    /// <summary>
    /// Create a TerminalTextModifier instance. It will automatically be subscribed to the event that runs during Terminal.TextPostProcess
    /// </summary>
    /// <param name="textToFind">The specific text string you wish to find and modify</param>
    /// <param name="additionalTextProvider">The string provider that will be used during text modification. If unsure what kind of provider to use, use <see cref="SimpleProvider{T}"/></param>
    public TerminalTextModifier(string textToFind, IProvider<string> additionalTextProvider)
    {
        TextToFind = textToFind;
        AddedTextProvider = additionalTextProvider;
        TerminalPatches.OnProcessNodeText += Process;
    }

    /// <summary>
    /// Use this method to change the string you are parsing for at runtime.
    /// </summary>
    /// <param name="textToFind">The specific string you wish to find and modify</param>
    public TerminalTextModifier ChangeTextToFind(string textToFind)
    {
        TextToFind = textToFind;
        return this;
    }

    /// <summary>
    /// Use this method to change the AddedTextProvider at runtime.
    /// </summary>
    /// <param name="addedTextProvider">The string provider that will be used during text modification</param>
    /// <returns></returns>
    public TerminalTextModifier ChangeAddedTextProvider(IProvider<string> addedTextProvider)
    {
        AddedTextProvider = addedTextProvider;
        return this;
    }

    /// <summary>
    /// Set a MatchInsert style for this text modifier, the default MatchInsert style will replace the matching text completely.
    /// </summary>
    /// <param name="style">
    /// ReplaceMatch (default) completely replaces the matching text with your added content.
    /// Before will insert your added content before the matching text.
    /// After will insert your added content after the matching text.
    /// </param>
    /// <remarks>
    /// NOTE: You can set this value to both Before and After to insert your content before and after the matching content (at the specified MatchIndex)
    /// Also, MatchInsert is not used with Regex Pattern matching.
    /// </remarks>
    public TerminalTextModifier SetInsertStyle(MatchInsert style)
    {
        InsertStyle = style;
        return this;
    }

    /// <summary>
    /// Set the MatchIndex style for this text modifier. This will determine which matching text should be modified.
    /// </summary>
    /// <param name="indexStyle">The expected style which can be: First, Last, All</param>
    /// <remarks>
    /// NOTE: MatchIndex is not used with Regex Pattern matching.
    /// </remarks>
    public TerminalTextModifier SetIndexStyle(MatchIndex indexStyle)
    {
        IndexStyle = indexStyle;
        return this;
    }

    /// <summary>
    /// Set the TerminalNode this modifier should perform text post processing directly to a given TerminalNode
    /// </summary>
    /// <param name="node">The TerminalNode text post processing should be performed on</param>
    /// <remarks>
    /// NOTE: Ensure you are resetting this any time the node is destroyed and recreated!
    /// </remarks>
    public TerminalTextModifier SetNodeDirect(TerminalNode node)
    {
        NodeToProcess = node;
        return this;
    }

    /// <summary>
    /// Set TerminalNode this modifier should perform text post processing on by it's keyword
    /// </summary>
    /// <param name="keyword">The keyword typed into the terminal that returns the expected terminal node</param>
    public TerminalTextModifier SetNodeFromKeyword(string keyword)
    {
        NodeKeyword = keyword;
        return this;
    }

    /// <summary>
    /// Set your TerminalTextModifier to use Regex pattern matching and replacing methods.
    /// </summary>
    /// <param name="value">True = Enabled, False = Disabled</param>
    /// <remarks>
    /// NOTE: Regex is less performant and should only be used for more complex pattern matching/replacements.
    /// Also, in order to use the matching value in your replacement text use a regex pattern in the AddedTextProvider like <![CDATA[$&]]>
    /// </remarks>
    public TerminalTextModifier UseRegexPattern(bool value)
    {
        RegexPattern = value;
        return this;
    }

    // called from event that is invoked after TextPostProcess
    // most likely should not be public
    internal void Process(ref string currentText, TerminalNode terminalNode)
    {
        // get node from keyword and assign it, only if node is null to not run every textpostprocess
        if (!string.IsNullOrEmpty(NodeKeyword) && NodeToProcess == null)
            NodeToProcess = GetNodeFromWord(NodeKeyword);

        // only skip processing when NodeToProcess is assigned and does not match
        if (NodeToProcess != null && terminalNode != NodeToProcess)
            return;

        // in case the provider tracks the amount of times it's been run, we only run it once
        string textToAdd = AddedTextProvider.Provide();

        // uses regex to replace the text rather than our simple string methods
        if (RegexPattern)
        {
            Regex regex = new(TextToFind);

            // uncomment below if any issues are encountered with text modification via regex
            /* DawnPlugin.Logger.LogDebug($"""
            Processing text modifier (regex) on node - {terminalNode}
            Regex ({TextToFind}) matches found {regex.Matches(TextToFind).Count}
            AddedText - {textToAdd} (before regex format conversion)
            """); */

            currentText = regex.Replace(currentText, textToAdd);
            return;
        }

        // uncomment below if any issues are encountered with text modification

        /* DawnPlugin.Logger.LogDebug($"""
            Processing text modifier (non-regex) on node - {terminalNode}
            IndexStyle - {IndexStyle}
            TextToFind - {TextToFind}
            AddedText - {textToAdd}
            """); */

        currentText = currentText.TextModify(IndexStyle, InsertStyle, TextToFind, textToAdd);
    }

    // maybe worthy of a terminal extension in the future, keeping private for now
    private static TerminalNode GetNodeFromWord(string word)
    {
        if (TerminalRefs.Instance.TryGetKeyword(word, out TerminalKeyword? terminalKeyword))
        {
            if (terminalKeyword.specialKeywordResult == null)
            {
                CompatibleNoun compatibleNoun = terminalKeyword.defaultVerb.compatibleNouns.FirstOrDefault(k => k.noun == terminalKeyword);
                if (compatibleNoun != null)
                    return compatibleNoun.result;
            }
            else
            {
                return terminalKeyword.specialKeywordResult;
            }
        }

        return null!;
    }
}
