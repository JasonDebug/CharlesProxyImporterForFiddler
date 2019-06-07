namespace CharlesProxy
{
    using Newtonsoft.Json;
    using System;
    using System.Text;

    public class CharlesSession
    {
        [JsonProperty("request")]
        public HttpSession Request { get; set; }

        [JsonProperty("response")]
        public HttpSession Response { get; set; }

        [JsonProperty("tunnel")]
        public bool Tunnel { get; set; }

        [JsonProperty("status")]
        public SessionStatus? Status { get; set; }

        public static CharlesSession[] FromJson(string json) => JsonConvert.DeserializeObject<CharlesSession[]>(json);
    }

    public class HttpSession
    {
        [JsonProperty("sizes")]
        public SizeInfo SizeInfo { get; set; }

        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("body")]
        public Body Body { get; set; }

        [JsonProperty("contentEncoding")]
        public ContentEncoding? ContentEncoding { get; set; }
    }

    public class SizeInfo
    {
        [JsonProperty("body")]
        public int body { get; set; }

        [JsonProperty("headers")]
        public int headers { get; set; }
    }

    public class Header
    {
        [JsonProperty("firstLine")]
        public string FirstLine { get; set; }

        [JsonProperty("headers")]
        public HeaderElement[] Headers { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(FirstLine) && Headers.Length == 0)
            {
                return null;
            }

            return $"{FirstLine}\r\n{string.Join<HeaderElement>("\r\n", Headers)}";
        }
    }

    public class HeaderElement
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Value}";
        }
    }

    public class Body
    {
        [JsonProperty("encoding", NullValueHandling = NullValueHandling.Ignore)]
        public string Encoding { get; set; }

        [JsonProperty("encoded", NullValueHandling = NullValueHandling.Ignore)]
        public string Encoded { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }
    }

    public enum ContentEncoding
    {
        gzip,
        brotli
    }

    public enum SessionStatus
    {
        UNKNOWN,
        SUCCESS,
        COMPLETE,
        EXCEPTION,
        RECEIVING_RESPONSE_BODY,
        RECEIVING_REQUEST_BODY,
        RECEIVED_REQUEST_BODY,
        RECEIVED_RESPONSE_BODY
    }
}