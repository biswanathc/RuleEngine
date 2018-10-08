using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RuleEngineApi.Models;
using Newtonsoft.Json;
using System.IO;
using System.Data;
using System.Configuration;
using System.Web.Hosting;

namespace RuleEngineApi.Controllers
{
    public class ValuesController : ApiController
    {

        /// <summary>
        /// API endpoint for signal processing
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<SignalDetails> Get()
        {
            var junkInput = new List<SignalDetails>();
            string ruleFilePath = HostingEnvironment.MapPath(@"~\App_Data\RuleSet.json");
            string inputFilePath = HostingEnvironment.MapPath(@"~\App_Data\raw_data.json");
            var ruleSet = GetRulesFromFile(ruleFilePath);
            var inputSignal = GetSignalsFromFile(inputFilePath);
            return ProcessSignal(inputSignal, ruleSet);
        }

        /// <summary>
        /// Method that collects input signal and ruleset to find dirty signals
        /// </summary>
        /// <param name="inputSignal">Input Signal</param>
        /// <param name="ruleSet">Rule Set</param>
        /// <returns>List of dirty signals</returns>
        private List<SignalDetails> ProcessSignal(List<SignalDetails> inputSignal, Rules ruleSet)
        {
            var junkInput = new List<SignalDetails>();
            foreach (SignalDetails signal in inputSignal)
            {
                var ruleByType = ruleSet.RuleSet.FirstOrDefault(n => Convert.ToString(n.ValueType).ToUpper() == signal.value_type.ToUpper());
                //Null check for rule type
                if (ruleByType != null)
                {
                    var ruleOperator = GetOperator(ruleByType.Operator.ToString());
                    string expression = string.Empty;
                    dynamic ruleValue = ruleByType.Value.ToString();
                    //Null check for rule value
                    if (string.IsNullOrWhiteSpace(ruleValue) && signal.value_type.ToUpper() != "DATETIME")
                    {
                        junkInput.Add(signal);
                        continue;
                    }
                    try
                    {
                        string signalValue;
                        switch (signal.value_type.ToUpper())
                        {
                            case "INTEGER":
                                ruleValue = Convert.ToInt32(ruleValue);
                                signalValue = signal.value;
                                ruleValue = Convert.ToString(ruleValue);
                                expression = signalValue + " " + ruleOperator + " " + ruleValue;
                                break;
                            case "DATETIME":
                                if (string.IsNullOrWhiteSpace(ruleValue))
                                {
                                    ruleValue = DateTime.Now;
                                }
                                else
                                {
                                    ruleValue = Convert.ToDateTime(ruleValue);
                                }
                                DateTime d1 = Convert.ToDateTime(signal.value);
                                DateTime d2 = Convert.ToDateTime(ruleValue);
                                int differenceInSecond = (d1 - d2).Seconds;
                                expression = differenceInSecond.ToString() + " " + ruleOperator + " " + "0";
                                break;
                            default:
                                signalValue = GetSignalStrengthKey(signal.value);
                                ruleValue = Convert.ToString(ruleValue);
                                expression = signalValue + " " + ruleOperator + " " + ruleValue;
                                break;
                        }
                    }
                    //Parsing error
                    catch (Exception ex)
                    {
                        junkInput.Add(signal);
                        continue;
                    }

                    //Rule not satisfied
                    if (!ComputeAgainstRule(expression))
                    {
                        junkInput.Add(signal);
                        continue;
                    }

                }
                //Rule not found for this type
                else
                {
                    junkInput.Add(signal);
                    continue;
                }
            }
            return junkInput;
        }

        /// <summary>
        /// Method that takes arithmetic expression and returns evaluated value
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private Boolean ComputeAgainstRule(string expression)
        {
            System.Data.DataTable table = new System.Data.DataTable();
            table.Columns.Add("", typeof(Boolean));
            table.Columns[0].Expression = expression;

            System.Data.DataRow r = table.NewRow();
            table.Rows.Add(r);
            Boolean result = (Boolean)r[0];
            table = null;
            return result;
        }

        /// <summary>
        /// This method reads rule set from file
        /// </summary>
        /// <param name="filePath">File Path</param>
        /// <returns>Rules object</returns>
        private Rules GetRulesFromFile(string filePath)
        {

            Rules ruleSet = new Rules();
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                ruleSet = JsonConvert.DeserializeObject<Rules>(json);
            }
            return ruleSet;
        }

        /// <summary>
        /// This method reads input signal from file
        /// </summary>
        /// <param name="filePath">File Path</param>
        /// <returns>List of signal object</returns>
        private List<SignalDetails> GetSignalsFromFile(string filePath)
        {
            var signalDetailsList = new List<SignalDetails>();
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                signalDetailsList = JsonConvert.DeserializeObject<List<SignalDetails>>(json);
            }
            return signalDetailsList;
        }

        /// <summary>
        /// This method reads operator name for a rule and returns symbol
        /// </summary>
        /// <param name="inputOperator">Operator Name</param>
        /// <returns>Operator Symbol</returns>
        private string GetOperator(string inputOperator)
        {
            switch (inputOperator)
            {
                case "GT":
                    return ">";
                case "GTE":
                    return ">=";
                case "LT":
                    return "<";
                case "LTE":
                    return "<=";
                case "EQ":
                    return "=";
                case "NEQ":
                    return "!=";
                default:
                    return "";
            }

        }

        /// <summary>
        /// This method receives signal strength value and returns key
        /// </summary>
        /// <param name="signalStrength">Signal strength value</param>
        /// <returns>signal strength key</returns>
        private string GetSignalStrengthKey(string signalStrength)
        {
            switch (signalStrength)
            {
                case "LOW":
                    return "0";
                case "HIGH":
                    return "1";
                default:
                    return "0";
            }
        }

    }
}
