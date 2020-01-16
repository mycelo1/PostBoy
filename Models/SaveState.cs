using System;

namespace PostBoy.Models
{
    [Serializable]
    public sealed class SaveState
    {
        public Config? config { get; set; }
        public Transaction? request { get; set; }
        public Transaction? response { get; set; }

        public sealed class Config
        {
            public string? version { get; set; }
            public int? timeout { get; set; }
            public string? proxy_address { get; set; }
        }

        public sealed class Transaction
        {
            public string? method { get; set; }
            public string? url { get; set; }
            public string? content_type { get; set; }
            public string? charset { get; set; }
            public string? header { get; set; }
            public string? body { get; set; }
        }
    }
}