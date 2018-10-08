using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RuleEngineApi.Models
{
    public class Rule
    {
        public object ValueType { get; set; }

        public object Value { get; set; }

        public object Operator { get; set; }
    }
}