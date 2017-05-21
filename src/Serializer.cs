using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Copyright (C) Dominic Masters - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * 
 * Written by Dominic Masters <dominic@domsplace.com>, November 2016
*/
namespace JsonSharp {
    public class Serializer {
        public static string serialize(ISerializable serializable) {
            return serialize(serializable.serialize());
        }

        public static string serialize(Dictionary<String, Object> data) {
            StringBuilder builder = new StringBuilder();
            serialize(builder, data);
            return builder.ToString();
        }

        private static void serialize(StringBuilder builder, Dictionary<String, Object> data) {
            builder.Append("{");

            int i = 0;
            foreach (String key in data.Keys) {
                object value = data[key];
                builder.Append("\"" + escape(key) + "\"" + ":");
                stringify(builder, value);

                i++;
                if (i < data.Keys.Count) builder.Append(",");
            }

            builder.Append("}");
        }

        private static void stringify(StringBuilder builder, object value) {
            if (value == null) {
                //Null
                builder.Append("null");
            } else if(value is bool) {
                builder.Append((bool)value ? "true" : "false");
            } else if(value is int) {
                builder.Append((int)value);
            } else if(value is byte) {
                builder.Append((byte)value);
            } else if(value is short) {
                builder.Append((short)value);
            } else if(value is long) {
                builder.Append((long)value);
            } else if(value is double) {
                builder.Append((double) value);
            } else if (value is object[]) {
                object[] array = (object[]) value;
                builder.Append("[");
                for(int i = 0; i < array.Length; i++) {
                    stringify(builder, array[i]);
                    if(i < array.Length-1) builder.Append(",");
                }
                builder.Append("]");
            } else if(value is string) {
                String strVal = ((string)value);
                //Escape our strings
                builder.Append("\"");
                builder.Append(escape(strVal));
                builder.Append("\"");
            } else if(value is Dictionary<String, Object>) {
                serialize(builder, (Dictionary<String, Object>)value);
            } else if(value is ISerializable) {
                ISerializable s = (ISerializable)value;
                stringify(builder, s.serialize());
            } else if(value is Guid) {
                stringify(builder, ((Guid)value).ToString());
            } else {
                stringify(builder, (string)value.ToString());
            }
        }

        private static string escape(String val) {
            return val
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\b", "\\b")
                .Replace("\0", "\\0")
                .Replace("\a", "\\a")
                .Replace("\f", "\\f")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\v", "\\v")
            ;
        }
    }
}
