using System;
using System.Collections.Generic;
using System.Data;

using Microsoft.Win32;

namespace Windows_Registry_Handler
{
    public class RegistryEntry
    {
        public static RegistryEntry Initialize()
        {
            return new();
        }
        
        public RegistryEntry WithRegistryKey(RegistryKey p_key)
        {
            if ( RegistryKey is not null ) throw new InvalidOperationException("RegistryKey has already been set on this object.");
            RegistryKey = p_key;
            return this;
        }

        public RegistryEntry WithRegistryKey(string p_key)
        {
            if ( RegistryKey is not null ) throw new InvalidOperationException("RegistryKey has already been set on this object.");
            RegistryKey = GetRegistryKeyFromString(p_key);
            return this;
        }

        public RegistryEntry WithSubKey(string p_subKey)
        {
            if ( !string.IsNullOrEmpty(RegistrySubKey) ) throw new InvalidOperationException("RegistrySubKey has already been set on this object.");
            RegistrySubKey = p_subKey;
            return this;
        }

        public RegistryEntry WithValueName(string p_valueName)
        {
            if ( !string.IsNullOrEmpty(ValueName) ) throw new InvalidOperationException("ValueName has already been set on this object.");
            ValueName = p_valueName;
            return this;
        }

        public RegistryEntry WithValue(object p_value)
        {
            Value = p_value;
            return this;
        }

        public static RegistryEntry CreateFromPath(string p_path, string p_valueName)
        {
            var splitCharacter = p_path.Contains('/') ? '/' : '\\';
        
            p_path = p_path.TrimStart($@"Computer{splitCharacter}".ToCharArray());            
            
            var registryKey = GetRegistryKeyFromString(p_path.Split(splitCharacter)[0]);
            
            var subKey = p_path.Substring(p_path.IndexOf(splitCharacter) + 1);
        
            return Initialize().WithRegistryKey(registryKey).WithSubKey(subKey).WithValueName(p_valueName);
        }

        private static RegistryKey GetRegistryKeyFromString(string p_registryKeyRoot)
        {
            return p_registryKeyRoot.ToUpper() switch
                   {
                       "HKEY_LOCAL_MACHINE" or "HKLM"  => Registry.LocalMachine,
                       "HKEY_CURRENT_USER" or "HKCU"   => Registry.CurrentUser,
                       "HKEY_USERS" or "HKU"           => Registry.Users,
                       "HKEY_CLASSES_ROOT" or "HKCR"   => Registry.ClassesRoot,
                       "HKEY_CURRENT_CONFIG" or "HKCC" => Registry.CurrentConfig,
                       _ => throw new ArgumentOutOfRangeException(
                                                                  $"Unknown registry key root string '{p_registryKeyRoot}' encountered in GetRegistryKeyFromString().")
                   };
        }

        public RegistryKey       RegistryKey    { get; private set; }
        public string            RegistrySubKey { get; private set; }
        public string            ValueName      { get; private set; }
        public RegistryValueKind ValueKind      => GetRegistryValueKind();
        public object            Value          { get; private set; }
        private bool IsFullyInitialized =>
            RegistryKey is not null && !string.IsNullOrEmpty(RegistrySubKey) && !string.IsNullOrEmpty(ValueName);
        private bool IsReadyToWrite => IsFullyInitialized && Value is not null;
        
        private RegistryValueKind GetRegistryValueKind()
        {
            if ( Value is null ) return RegistryValueKind.Unknown;
            
            return Value switch
                   {
                       string                          => RegistryValueKind.String,
                       string[] or IEnumerable<string> => RegistryValueKind.MultiString,
                       int                             => RegistryValueKind.DWord,
                       float or double or decimal      => RegistryValueKind.QWord,
                       byte[]                          => RegistryValueKind.Binary,
                       _ => throw new
                                ArgumentOutOfRangeException($"Unknown value type '{Value.GetType()}' encountered in GetRegistryValue().")
                   };
        }

        public void SetValue(object p_value)
        {
            Value = p_value;
        }

        public void GetValueFromMachineRegistry()
        {
            if ( !IsFullyInitialized )
            {
                throw new
                    InvalidOperationException("The registry entry is not fully initialized. RegistryKey, RegistrySubKey, and ValueName must all be initialized before this operation can execute.");
            }

            Value = Registry.GetValue($@"{RegistryKey}\{RegistrySubKey}", ValueName, null);

            if ( Value is null ) throw new DataException($"The value of '{ValueName}' could not be retrieved from the system registry. A value with this name likely does not exist.");
        }

        public void WriteValueToMachineRegistry()
        {
            if ( !IsReadyToWrite )
            {
                throw new
                    InvalidOperationException("The registry entry is not ready to write. RegistryKey, RegistrySubKey, ValueName, and Value must all be initialized before this operation can execute.");
            }
            
            Registry.SetValue($@"{RegistryKey}\{RegistrySubKey}", ValueName, Value, ValueKind);
        }

        public void RemoveValueFromMachineRegistry()
        {
            if ( !IsFullyInitialized )
            {
                throw new
                    InvalidOperationException("The registry entry is not fully initialized. RegistryKey, RegistrySubKey, and ValueName must all be initialized before this operation can execute.");
            }

            using var openKey = RegistryKey.OpenSubKey(RegistrySubKey, true);

            if ( openKey is null ) throw new DataException("The requested registry path does not exist.");

            openKey.DeleteValue(ValueName);
        }

        public string GetRegistryPath()
        {
            return $@"{RegistryKey}\{RegistrySubKey}\{ValueName}";
        }
    }
}