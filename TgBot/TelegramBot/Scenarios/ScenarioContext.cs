using System;
using System.Collections.Generic;

namespace TgBot.Scenarios
{
    public class ScenarioContext
    {
        public ScenarioType CurrentScenario { get; set; }

        public string? CurrentStep { get; set; }

        public Dictionary<string, object> Data { get; set; }

        public DateTime CreatedAt { get; }

        public ScenarioContext(ScenarioType scenario)
        {
            CurrentScenario = scenario;
            CurrentStep = null;
            Data = new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
        }
    }
}