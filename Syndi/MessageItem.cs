namespace Syndi
{
    internal class MessageItem(string channelID, string messageID)
    {
        public string ChannelID { get; private set; } = channelID;
        public string MessageID { get; private set; } = messageID;
    }
}
