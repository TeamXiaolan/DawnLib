using System;

namespace Dawn;
public sealed class DawnComplexQueryCommandInfo
{
    public DawnTerminalCommandInfo ParentInfo { get; internal set; }

    internal DawnComplexQueryCommandInfo()
    {
        // needs a verb terminalkeyword
        // needs a set of terminalkeyword and terminalnodes for results
        // these terminalkeywords for the nouns here need to point to this verb as default verb
        // each pair of terminalkeyword and terminalnode need a confirm/deny pair of terminalnode and terminalkeyword
        // each confirm/deny pair need to be conditioned so that you cant confirm willfully but a thing runs so that you actually CAN confirm
        // i.e. buy command
    }
}