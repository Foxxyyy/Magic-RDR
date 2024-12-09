using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Magic_RDR
{
    class NativeHashDB
    {
        const string _NativesPath = "Scripts/natives.json";
        private static Dictionary<uint, Tuple<string, string>> _db = new Dictionary<uint, Tuple<string, string>>();
        public static bool ShowNativeNamespace = false;
        private static bool _inited = false;

        static void LoadNatives()
        {
            if (!File.Exists(_NativesPath))
                return;

            var jsonContent = File.ReadAllText(_NativesPath);
            var jsonObject = JObject.Parse(jsonContent);

            _db = new Dictionary<uint, Tuple<string, string>>();
            foreach (var ns in jsonObject.Properties())
            {
                var ns_name = ns.Name;
                var systemObject = (JObject)ns.Value;

                foreach (var item in systemObject.Properties())
                {
                    string key = item.Name;
                    if (key.StartsWith("0x"))
                        key = key.Substring(2);

                    string name = item.Value["name"]?.ToString() ?? "";

                    if (name.StartsWith("_0x"))
                        continue;

                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(name))
                    {
                        var key_int = 0U;
                        if (uint.TryParse(key, System.Globalization.NumberStyles.HexNumber, null, out key_int))
                            _db[key_int] = new Tuple<string, string>(ns_name.ToUpper(), name.ToUpper());

                    }
                }
            }
            _inited = true;
        }

        public static string GetName(uint hash)
        {
            if (!_inited)
                LoadNatives();

            if (_db.ContainsKey(hash))
            {
                if (ShowNativeNamespace)
                    return $"{_db[hash].Item1}::{_db[hash].Item2}";
                return _db[hash].Item2;
            }
            return $"UNK_0x{hash:X8}";
        }
    }
}
