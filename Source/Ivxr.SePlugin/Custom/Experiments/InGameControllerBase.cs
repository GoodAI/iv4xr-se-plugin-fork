using System;
using System.Collections;
using System.Collections.Generic;
using Iv4xr.SePlugin.Custom.ChatCommands;
using Sandbox.ModAPI;
using VRage.Game.Components;

namespace Iv4xr.SePlugin.Custom.Experiments
{
    public delegate void CommandHandler(string command, string[] arguments);

    public delegate IEnumerator CommandHandlerCoroutine(string command, string[] arguments);

    public abstract class InGameControllerBase : MySessionComponentBase
    {
        private List<Command> commands = new List<Command>();
        private bool isInitialized = false;
        private string commandPrefix = "/";
        private string chatSender = "Helper";

        protected void RegisterCommand(MessagePattern pattern, CommandHandler commandHandler)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (commandHandler == null) throw new ArgumentNullException(nameof(commandHandler));

            commands.Add(new Command(pattern, WrapCommandHandler(commandHandler)));
        }

        protected void RegisterCommand(MessagePattern pattern, CommandHandlerCoroutine commandHandler)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (commandHandler == null) throw new ArgumentNullException(nameof(commandHandler));

            commands.Add(new Command(pattern, commandHandler));
        }

        private CommandHandlerCoroutine WrapCommandHandler(CommandHandler commandHandler)
        {
            return (command, arguments) => WrapCommandHandler(commandHandler, command, arguments);
        }

        private IEnumerator WrapCommandHandler(CommandHandler commandHandler, string command, string[] arguments)
        {
            commandHandler(command, arguments);
            yield break;
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();

            if (!isInitialized)
            {
                isInitialized = true;
                InitInternal();
            }
        }

        protected void ConfigureChatCommands(string commandPrefix, string chatSender)
        {
            if (commandPrefix != null)
            {
                this.commandPrefix = commandPrefix;
            }

            if (chatSender != null)
            {
                this.chatSender = chatSender;
            }
        }

        protected void ShowMessage(string message)
        {
            MyAPIGateway.Utilities.ShowMessage(chatSender, message);
        }

        protected virtual void Init()
        {

        }

        private void InitInternal()
        {
            Init();
            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
        }

        private void MessageEntered(string text, ref bool others)
        {
            foreach (var command in commands)
            {
                if (!IsCommandMatch(text, command.Pattern))
                {
                    continue;
                }

                if (command.Pattern.Type == MessagePatternType.Exact)
                {
                    CoroutineManager.Instance.StartCoroutine(command.Handler.Invoke(text, null));
                }
                else
                {
                    var prefix = commandPrefix + command.Pattern.Text;
                    var commandWithoutPrefix = text.Substring(prefix.Length);
                    commandWithoutPrefix = commandWithoutPrefix.Trim();
                    var arguments = commandWithoutPrefix.Split(' ');
                    
                    CoroutineManager.Instance.StartCoroutine(command.Handler.Invoke(text, arguments));
                }

                break;
            }
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
        }

        private bool IsCommandMatch(string text, MessagePattern pattern)
        {
            var prefix = commandPrefix + pattern.Text;

            if (pattern.Type == MessagePatternType.Exact)
            {
                return text == prefix;
            }
            else
            {
                return text.StartsWith(prefix);
            }
        }

        private class Command
        {
            public MessagePattern Pattern { get; }

            public CommandHandlerCoroutine Handler { get; }

            public Command(MessagePattern pattern, CommandHandlerCoroutine handler)
            {
                Pattern = pattern;
                Handler = handler;
            }
        }
    }
}