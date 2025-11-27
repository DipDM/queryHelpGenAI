using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace queryHelp.Settings
{
    public class AiSettings
    {
        public string Provider { get; set; } = "OpenAI"; 
        public string ApiKey { get; set; } = null!;
        public string Model { get; set; } = "gpt-4o-mini"; 
        public int MaxTokens { get; set; } = 800;
        public double Temperature { get; set; } = 0.0; // low-temperature for deterministic SQL
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    }
}