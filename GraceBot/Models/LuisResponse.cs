using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GraceBot.Models
{
    internal class LuisResponse
    {
        public string query { get; set; }
        public Intent topScoringIntent { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
        public Dialog dialog { get; set; }
    }

    internal class Action
    {
        public bool triggered { get; set; }
        public string name { get; set; }
        public Parameter[] parameters { get; set; }
    }

    internal class Parameter
    {
        public string name { get; set; }
        public string type { get; set; }
        public bool required { get; set; }
        public Value[] value { get; set; }
    }

    internal class Value
    {
        public string entity { get; set; }
        public string type { get; set; }
        public Resolution resolution { get; set; }
    }

    internal class Resolution
    {
    }

    internal class Dialog
    {
        public string contextId { get; set; }
        public string status { get; set; }
    }

    internal class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
        public Action[] actions { get; set; }
    }

    internal class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
        public Resolution resolution { get; set; }
    }
}