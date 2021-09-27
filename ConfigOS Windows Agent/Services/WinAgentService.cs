using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows_Registry_Handler;
using WindowsAgentService;
using Grpc.Core;
using Microsoft.Win32;
using Serilog;

namespace ConfigOS_Windows_Agent.Services
{
    public class WinAgentService : WinAgent.WinAgentBase
    {
        public override async Task RequestTask(G_TaskRequest                       request,
                                               IServerStreamWriter<G_TaskResponse> responseStream,
                                               ServerCallContext                   context)
        {
            var deserializedPolicy = GetDeserializedPolicy(request);

            switch ( request.TaskType )
            {
                case G_TaskRequest.Types.TaskType.Scan:
                    await RunScan(deserializedPolicy, responseStream);
                    break;
                case G_TaskRequest.Types.TaskType.Remediate:
                    break;
                case G_TaskRequest.Types.TaskType.Rollback:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static PolicyContents GetDeserializedPolicy(G_TaskRequest request)
        {
            var fileStream = new MemoryStream(Encoding.ASCII.GetBytes(request.PolicyText));

            var deserializer = new XmlSerializer(typeof(PolicyContents));

            return (PolicyContents) deserializer.Deserialize(fileStream);
        }

        private static async Task RunScan(PolicyContents p_deserializedPolicy, IAsyncStreamWriter<G_TaskResponse> p_responseStream)
        {
            var registryControls = p_deserializedPolicy.Items.Controls.Where(p_control => p_control.Where.StartsWith("HK"));
            
            Console.WriteLine($"Found {registryControls.Count()} registry controls.");
            
            foreach ( var control in registryControls )
            {
                Log.Information($"Processing control {control.GroupId}...");
            
                var registryEntry = RegistryEntry.CreateFromPath(control.Where, control.Applied);
            
                try
                {
                    registryEntry.GetValueFromMachineRegistry();
            
                    var passed = ValueIsValid(registryEntry, control.Value);

                    await p_responseStream.WriteAsync(new G_TaskResponse
                                                      {
                                                          ControlData = new G_ControlEvaluationCompletedResponse()
                                                                        {
                                                                            ControlStatus = passed ? G_ControlEvaluationCompletedResponse.Types.ControlStatus.FoundAndPassed : G_ControlEvaluationCompletedResponse.Types.ControlStatus.FoundAndFailed,
                                                                            CurrentValue  = registryEntry.Value.ToString(),
                                                                            ExpectedValue = control.Value,
                                                                            RuleId        = control.RuleId,
                                                                            Comment       = string.Empty
                                                                        }
                                                      });
                }
                catch ( DataException )
                {
                    await p_responseStream.WriteAsync(new G_TaskResponse
                                                      {
                                                          ControlData = new G_ControlEvaluationCompletedResponse()
                                                                        {
                                                                            ControlStatus = G_ControlEvaluationCompletedResponse.Types.ControlStatus.NotFoundAndFailed,
                                                                            CurrentValue  = "null",
                                                                            ExpectedValue = control.Value,
                                                                            RuleId        = control.RuleId,
                                                                            Comment       = string.Empty
                                                                        }
                                                      });
                }
            }
        }

        private object GetFormattedControlValue(PolicyControl p_control)
        {
            var valueType = GetValueType(p_control.Type);

            return ConvertControlValueToCorrectType(GetDefaultValue(p_control.Value), valueType);
        }

        private string GetDefaultValue(string p_controlValue)
        {
            return p_controlValue.Contains('[') ? p_controlValue.Split(',')[1] : p_controlValue;
        }

        private RegistryValueKind GetValueType(string p_controlType)
        {
            return p_controlType switch
                   {
                       "REG_DWORD" => RegistryValueKind.DWord,
                       "REG_SZ" => RegistryValueKind.String,
                       "REG_MULTI_SZ" => RegistryValueKind.MultiString
                   };
        }

        private static bool ValueIsValid(RegistryEntry p_registryEntry, string p_controlValue)
        {
            return p_controlValue switch
                   {
                       _ when p_controlValue.ToUpper().Contains("MAX") => ValueMatchesMax(p_registryEntry.Value,
                                                                                          GetMinMaxValue(p_controlValue)),
                       _ when p_controlValue.ToUpper().Contains("MIN") => ValueMatchesMin(p_registryEntry.Value,
                                                                                          GetMinMaxValue(p_controlValue)),
                       _ when p_controlValue.ToUpper().Contains("...") => ValueIsInRange(p_registryEntry.Value,
                                                                                         GetValueRange(p_controlValue)),
                       _ when p_controlValue.ToUpper().Contains("|") => ValueIsAnyOf(p_registryEntry.Value, GetValidValues(p_controlValue)),
                       _                                             => ValueMatches(p_registryEntry, p_controlValue)
                   };
        }

        private static bool ValueMatches(RegistryEntry p_registryEntry, string p_controlValue)
        {
            return p_registryEntry.Value.Equals(ConvertControlValueToCorrectType(p_controlValue, p_registryEntry.ValueKind));
        }

        private static bool ValueMatchesMax(object p_registryEntryValue, int p_maxValue)
        {
            return Convert.ToInt32(p_registryEntryValue) <= p_maxValue;
        }

        private static bool ValueMatchesMin(object p_registryEntryValue, int p_minValue)
        {
            return Convert.ToInt32(p_registryEntryValue) >= p_minValue;
        }

        private static int GetMinMaxValue(string p_controlValue)
        {
            var tryParse = int.TryParse(p_controlValue.Split(',')[0].TrimStart('[').TrimEnd(']').Split("...")[1], out var intValue);

            return tryParse ? intValue : throw new DataException($"Could not parse int value from control value '{p_controlValue}'");
        }

        private static bool ValueIsInRange(object p_registryEntryValue, IEnumerable<int> p_validRange)
        {
            return p_validRange.Contains(Convert.ToInt32(p_registryEntryValue));
        }

        private static IEnumerable<int> GetValueRange(string p_controlValue)
        {
            var numbers = p_controlValue.Split(',')[0].TrimStart('[').TrimEnd(']').Split("...");

            var lowNumber = int.Parse(numbers[0]);

            var highNumber = int.Parse(numbers[1]);

            if ( lowNumber >= highNumber )
            {
                throw new
                    DataException($"The low number ({lowNumber}) was equal to or greater than the high number ({highNumber}) in the given range.");
            }

            var validNumbers = new int[highNumber - lowNumber + 1];

            for ( var i = 0; i < validNumbers.Length; ++i )
            {
                validNumbers[i] = lowNumber + i;
            }

            return validNumbers;
        }

        private static bool ValueIsAnyOf(object p_registryEntryValue, IEnumerable<string> p_validRange)
        {
            return p_validRange.Contains(p_registryEntryValue);
        }

        private static IEnumerable<string> GetValidValues(string p_controlValue)
        {
            var values           = p_controlValue.Split(',')[0].TrimStart('[').TrimEnd(']').Split("|");
            
            var validValuesArray = new string[values.Length];
            
            for ( var i = 0; i < values.Length; ++i )
            {
                validValuesArray[i] = values[i].Trim();
            }
            
            return validValuesArray;
        }

        private static object ConvertControlValueToCorrectType(string p_controlValue, RegistryValueKind p_registryEntryValueKind)
        {
            return p_registryEntryValueKind switch
                   {
                       RegistryValueKind.String => p_controlValue,
                       RegistryValueKind.MultiString => p_controlValue.Split(' '),
                       RegistryValueKind.DWord  => int.Parse(p_controlValue)
                   };
        }
    }
}