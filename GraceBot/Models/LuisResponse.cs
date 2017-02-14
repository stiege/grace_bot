using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GraceBot.Models
{
    public class LuisResponse
    {
        public string Query { get; set; }
        public Intent TopScoringIntent { get; set; }
        public Intent[] Intents { get; set; }
        public Entity[] Entities { get; set; }
        public Dialog Dialog { get; set; }
    }

    public class Action
    {
        public bool Triggered { get; set; }
        public string Name { get; set; }
        public Parameter[] Parameters { get; set; }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public Value[] Value { get; set; }
    }

    public class Value
    {
        public string Entity { get; set; }
        public string Type { get; set; }
        public Resolution Resolution { get; set; }
    }

    public class Resolution
    {
    }

    public class Dialog
    {
        public string ContextId { get; set; }
        public string Status { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
        public Action[] actions { get; set; }
    }

    public class Entity
    {
        [JsonProperty("entity")]
        public string Name { get; set; }

        public string Type { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public float Score { get; set; }
        public Resolution Resolution { get; set; }
    }
}