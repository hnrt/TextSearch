﻿using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class UpdatePreferenceRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public UpdatePreferenceRequest()
        {
        }

        public UpdatePreferenceRequest(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
