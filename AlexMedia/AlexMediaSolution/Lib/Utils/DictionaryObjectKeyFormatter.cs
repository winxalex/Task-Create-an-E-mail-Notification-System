using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;

namespace AlexMedia.Utils
{
    /// <summary>
    /*
     Example:
     
     var data3 = new
    {
    person = new Dictionary<object, object>
    {
        {"first_name", "Mile"},
        {"last_name", "Doe"},
        {
            "other", new Dictionary<object, object>
            {
                {"somekey", "somevalue"}
            }
        }
    },
    npc = new Dictionary<string, object>
    {
        {"first_name", "John"},
        {"last_name", "Doe"}
    }
                
    };

    
    var format = "{person:dict_oo(first_name)}-{person:dict_oo(other):{somekey}}-{npc.first_name}";
    */
    /// </summary>
    public class DictionaryObjectKeyFormatter : IFormatter
    {
        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            if (formattingInfo.CurrentValue is Dictionary<object, object> dict)
            {
                var format = formattingInfo.Format;

                if (format == null) return false;

                var key = formattingInfo.FormatterOptions.ToString();

                if (dict.ContainsKey(key))
                {
                    var dictValue = dict[key];
                    if (dictValue is Dictionary<object, object>)
                        formattingInfo.FormatAsChild(format, dictValue);
                    else
                        formattingInfo.Write(dictValue != null ? dictValue.ToString() : string.Empty);
                    return true;
                }
            }

            return false;
        }

        public string Name { get; set; } = "dict_oo";
        public bool CanAutoDetect { get; set; } = true;
    }
}