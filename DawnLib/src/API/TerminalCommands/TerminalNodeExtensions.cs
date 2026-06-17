using System;
using Dawn.Interfaces;

namespace Dawn;

public static class TerminalNodeExtensions
{
    extension(TerminalNode terminalNode)
    {
        public DawnTerminalCommandInfo DawnInfo
        {
            get => terminalNode.GetDawnInfoCore();
            set => terminalNode.SetDawnInfoCore(value);
        }

        [Obsolete("Use TerminalNode.DawnInfo instead")]
        public DawnTerminalCommandInfo GetDawnInfo()
        {
            return terminalNode.GetDawnInfoCore();
        }

        [Obsolete("Use TerminalNode.DawnInfo instead")]
        public void SetDawnInfo(DawnTerminalCommandInfo terminalNodeInfo)
        {
            terminalNode.SetDawnInfoCore(terminalNodeInfo);
        }

        private DawnTerminalCommandInfo GetDawnInfoCore()
        {
            return ((ITerminalNodeDawnObject)terminalNode).DawnInfo;
        }

        private void SetDawnInfoCore(DawnTerminalCommandInfo terminalNodeInfo)
        {
            ((ITerminalNodeDawnObject)terminalNode).DawnInfo = terminalNodeInfo;
        }

        public string GetDisplayText()
        {
            DawnTerminalCommandInfo? commandInfo = terminalNode.DawnInfo;
            if (commandInfo != null && commandInfo.InputCommandInfo != null)
            {
                return commandInfo.InputCommandInfo.DynamicInputTextResult.Invoke(DawnInputCommandInfo.GetLastUserInput());
            }

            if (((ITerminalNode)terminalNode).DynamicDisplayText == null)
            {
                return terminalNode.displayText;
            }
            return ((ITerminalNode)terminalNode).DynamicDisplayText.Invoke();
        }

        internal void SetDynamicDisplayText(Func<string> func)
        {
            ((ITerminalNode)terminalNode).DynamicDisplayText = func;
        }
    }
}