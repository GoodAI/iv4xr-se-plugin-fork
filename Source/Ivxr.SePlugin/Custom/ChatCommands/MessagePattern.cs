namespace Iv4xr.SePlugin.Custom.ChatCommands
{
    public class MessagePattern
    {
        public MessagePatternType Type { get; }

        public string Text { get; }

        public MessagePattern(MessagePatternType type, string text)
        {
            Type = type;
            Text = text;
        }
    }
}