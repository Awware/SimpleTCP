using Newtonsoft.Json;
using System;

namespace CommonTypess
{
    [Serializable]
    public class AnswerType
    {
        public AnswerType(string data, int answerint) { SomeData = data; AnswerInt = answerint; }
        [JsonProperty("data")]
        public string SomeData { get; }
        [JsonProperty("aint")]
        public int AnswerInt { get; }
    }
}
