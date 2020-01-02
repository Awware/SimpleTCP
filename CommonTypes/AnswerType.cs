using Newtonsoft.Json;
using System;

namespace CommonTypess
{
    [Serializable]
    public class AnswerType
    {
        public AnswerType(string data) { SomeData = data; }
        [JsonProperty("data")]
        public string SomeData { get; }
    }
}
