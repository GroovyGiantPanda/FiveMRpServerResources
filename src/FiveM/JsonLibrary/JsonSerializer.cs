using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TfJsonLibrary
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class JsonSerializable : System.Attribute
    {
        private string jsonName;

        public JsonSerializable(string JsonName)
        {
            this.jsonName = JsonName;
        }

        public string GetJsonName()
        {
            return jsonName;
        }
    }

    public static class JsonSerializer
    {
        public static string SerializeObject(object o)
        {
            var sb = new StringBuilder();
            if (o.GetType().GetTypeInfo().IsClass)
            {
                sb.Append("{");
                Dictionary<string, string> data = new Dictionary<string, string>();
                var allProps = o.GetType().GetProperties();
                foreach (PropertyInfo i in o.GetType().GetProperties())
                {
                    string jsonName;
                    if (i.GetCustomAttribute(typeof(JsonSerializable)) != null)
                    {
                        jsonName = ((JsonSerializable) i.GetCustomAttribute(typeof(JsonSerializable)))
                            .GetJsonName();
                    }
                    else
                    {
                        jsonName = i.Name;
                    }
                    Console.WriteLine(jsonName);
                    var serializedObject = _serializeObject(i.GetValue(o, null));
                    data.Add(JsonEscape(jsonName), serializedObject);
                }
                sb.Append(string.Join(",", data.Select(d => $@"{d.Key}:{d.Value}")));
                sb.Append("}");
            }
            else if (o.GetType().IsNumericType())
            {
                sb.Append(String.Format(CultureInfo.InvariantCulture, "{0}", o));
            }
            else if (o.GetType().IsGenericType && (o.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>) || o.GetType().GetGenericTypeDefinition() == typeof(List<>)))
            {
                sb.Append(_serializeObject(o));
            }
            else if (o is string)
            {
                sb.Append($@"{JsonEscape(o as string)}");
            }
            else if (o is bool)
            {
                sb.Append(o.ToString().ToLower());
            }
            else
            {
                throw new Exception($"Unexpected type {o.GetType()}");
            }
            return sb.ToString();
        }

        internal static string _serializeObject(object o)
        {
            Type type = o.GetType();
            var sb = new StringBuilder();
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    sb.Append($@"{{{string.Join(",", CastDictionaryEntries((IDictionary)o).ToDictionary(e => e.Key as string, e => e.Value as object).Select(t => $@"{JsonEscape(t.Key)}:{_serializeObject(t.Value)}"))}}}");
                }
                else if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    sb.Append($@"[{string.Join(",", ((IEnumerable)o).Cast<object>().Select(t => $@"{JsonEscape(_serializeObject(t))}"))}]");
                }
            }
            else if (o is bool)
            {
                sb.Append(o.ToString().ToLower());
            }
            else if (Extensions.IsNumericType(o))
            {
                sb.Append(String.Format(CultureInfo.InvariantCulture, "{0}", o));
            }
            else if (o is string)
            {
                sb.Append(JsonEscape(o as string));
            }
            else
            {
                throw new Exception($"Unexpected type {o.GetType()} ({Type.GetTypeCode(o.GetType()) == TypeCode.Int32})");
            }
            return sb.ToString();
        }

        private static IEnumerable<DictionaryEntry> CastDictionaryEntries(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return entry;
            }
        }

        public static string JsonEscape(string unescaped)
        {
            if (string.IsNullOrEmpty(unescaped)) return unescaped;

            StringBuilder sb = new StringBuilder(unescaped.Length + 4);
            String t;

            sb.Append('"');
            for (int i = 0; i < unescaped.Length; i += 1)
            {
                char c = unescaped[i];
                switch (c)
                {
                    case '\\':
                    case '"':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    default:
                        if (c < ' ' || c > 0x7f)
                        {
                            t = "000" + string.Format("X", c);
                            sb.Append("\\u" + t.Substring(t.Length - 4));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
