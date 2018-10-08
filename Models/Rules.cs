using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RuleEngineApi.Models
{
    public class Rules
    {
        public List<Rule> RuleSet { get; set; }

    }
}