namespace CheatServer.Transports
{
    public enum TimeKeyCommand
    {
        Create,
        Redeem
    }

    public class TimeKeyRequest
    {
        public TimeKeyCommand Command { get; set; }

        public string UserId { get; set; }

        public string GameId { get; set; }

        public string Key { get; set; }

    }
}
