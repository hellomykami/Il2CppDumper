using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Il2CppDumper
{

    public partial class FilterJson
    {
        [JsonProperty("filters")]
        public List<Filter> Filters = new List<Filter>();
    }

    public partial class Filter
    {
        [JsonProperty("namespace")]
        public string Namespace;
        [JsonProperty("class")]
        public string Class;
        [JsonProperty("methods")]
        public List<string> Methods;
        [JsonProperty("type")]
        public string Type;
    }
}
