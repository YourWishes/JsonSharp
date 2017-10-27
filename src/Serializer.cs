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
                builder.Append("\""+escape(key)+"\""+":");
                stringify(builder, value);

                i++;
                if (i<data.Keys.Count) builder.Append(",");
            }

            builder.Append("}");
        }

        private static void stringify(StringBuilder builder, object value) {
            if (value==null) {
                //Null
                builder.Append("null");
            } else if (value is bool) {
                builder.Append((bool)value ? "true" : "false");
            } else if (value is int) {
                builder.Append((int)value);
            } else if (value is byte) {
                builder.Append((byte)value);
            } else if (value is short) {
                builder.Append((short)value);
            } else if (value is long) {
                builder.Append((long)value);
            } else if (value is float) {
                builder.Append((float)value);
            } else if (value is double) {
                builder.Append((double)value);
            } else if (value is uint) {
                builder.Append((uint)value);
            } else if (value is ushort) {
                builder.Append((ushort)value);
            } else if (value is ulong) {
                builder.Append((ulong)value);
            } else if (value is Array) {
                object[] array = (object[])value;
                builder.Append("[");
                for (int i = 0; i<array.Length; i++) {
                    stringify(builder, array[i]);
                    if (i<array.Length-1) builder.Append(",");
                }
                builder.Append("]");
            } else if(value is System.Collections.IList) {
                //Treat as generic list.
                System.Collections.IList e = (System.Collections.IList)value;
                builder.Append("[");
                foreach(object o in e) {
                    stringify(builder, o);
                    builder.Append(",");
                }
                builder.Length--;//Remove the comma too many.
                builder.Append("]");
            } else if (value is string) {
                String strVal = ((string)value);
                //Escape our strings
                builder.Append("\"");
                builder.Append(escape(strVal));
                builder.Append("\"");
            } else if (value is Dictionary<String, Object>) {
                serialize(builder, (Dictionary<String, Object>)value);
            } else if (value is ISerializable) {
                ISerializable s = (ISerializable)value;
                stringify(builder, s.serialize());
            } else if (value is Guid) {
                stringify(builder, ((Guid)value).ToString());
            } else {
                stringify(builder, (string)value.ToString());
            }
        }

        private static string escape(String val) {
            return val
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
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

        private static string unescape(String val) {
            return val
                .Replace("\\v", "\v")
                .Replace("\\t", "\t")
                .Replace("\\r", "\r")
                .Replace("\\f", "\f")
                .Replace("\\a", "\a")
                .Replace("\\0", "\0")
                .Replace("\\b", "\b")
                .Replace("\\n", "\n")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
            ;
        }

        private static bool isSpecialCharacter(char c) {
            return c=='\n'||c=='\b'||c=='\0'||c=='\a'||c=='\f'||c=='\r'||c=='\t'||c=='\v';
        }

        private static bool isWhitespaceCharacter(char c) {
            return c == ' ' || c == '\n' || c == '\r' || c == '\t';//Tab delimiter was causing some issues.
        }

        public static Dictionary<string, object> unserialize(string data) {
            if (data.Length<2) throw new Exception("Not a valid JSON.");

            int index = skipIfWhitespace(0, data);
            if (data[index]!='{') throw new Exception("Invalid JSON, expected \"{\" at index " + index);
            index++;
            Dictionary<string, object> jsonObject = unserializeObject(ref index, data);

            return jsonObject;
        }

        //Returns the index of the next non space character
        private static int skipWhitespace(int currentIndex, string data) {
            int indexStart = currentIndex+1;
            if (indexStart>=data.Length||indexStart<0) throw new Exception();
            for (int i = indexStart; i<data.Length; i++) {
                if (isWhitespaceCharacter(data[i])) continue;
                return i;
            }
            return data.Length;
        }

        private static int skipIfWhitespace(int currentIndex, string data) {
            if (currentIndex>=data.Length||currentIndex<0) throw new Exception();
            if (!isWhitespaceCharacter(data[currentIndex])) return currentIndex;
            return skipWhitespace(currentIndex, data);
        }

        private static Dictionary<string, object> unserializeObject(ref int index, string data) {
            Dictionary<string, object> jsonObject = new Dictionary<string, object>();
            index=skipIfWhitespace(index, data);
            if (data[index]=='}') {
                index++;
                return jsonObject;
            }
            while (true) {
                index=skipIfWhitespace(index, data);
                string key = unserializeString(ref index, data);//Get Key
                index=skipIfWhitespace(index, data);

                if (data[index]!=':') throw new Exception("Invalid JSON, expected \":\" at index " + index);//Make sure key has a value
                index++;

                index=skipIfWhitespace(index, data);
                object value = unserializeValue(ref index, data);//Get value

                jsonObject.Add(key, value);//Append Value
                index=skipIfWhitespace(index, data);
                if (index>=data.Length) throw new Exception("Invalid JSON");//Validate

                if (data[index]==',') {//Another element in the object
                    index++;
                    continue;
                }
                if (data[index]=='}') {//End of the object.
                    index++;
                    break;
                }
                throw new Exception("Invalid JSON");//Something else?
            }
            return jsonObject;
        }

        private static object unserializeValue(ref int index, string data) {
            //Determine the type...
            if (data[index]=='"') {
                //Probably a string
                return unescape(unserializeString(ref index, data, true));
            } else if (getChars(index, 4, data)=="null") {
                //null
                index+=4;
                return null;
            } else if (getChars(index, 4, data)=="true") {
                //true
                index+=4;
                return true;
            } else if (getChars(index, 5, data)=="false") {
                //false
                index+=5;
                return false;
            } else if (data[index]=='{') {
                //Probably an object
                index++;
                return unserializeObject(ref index, data);
            } else if (data[index]=='[') {
                //Probably an array
                index++;
                return unserializeArray(ref index, data);
            }

            //Another type, we need the full data for
            string untilNext = "";//Basically we are going to read the data until we reach the next "," "}" "]" or "\""
            for (int h = index; h<data.Length; h++) {
                char c = data[h];
                if (isSpecialCharacter(c)||c==','||c=='}'||c==']') break;
                untilNext+=c;
            }
            index+=untilNext.Length;

            //Determine the type now? starting from shortest first
            byte b;
            if (byte.TryParse(untilNext, out b)) {
                return b;
            }

            short s;
            if (short.TryParse(untilNext, out s)) {
                return s;
            }

            ushort us;
            if (ushort.TryParse(untilNext, out us)) {
                return us;
            }

            int i;
            if (int.TryParse(untilNext, out i)) {
                return i;
            }

            uint ui;
            if (uint.TryParse(untilNext, out ui)) {
                return ui;
            }

            long l;
            if (long.TryParse(untilNext, out l)) {
                return l;
            }

            ulong ul;
            if (ulong.TryParse(untilNext, out ul)) {
                return ul;
            }

            float f;
            if (float.TryParse(untilNext, out f)) {
                return f;
            }

            double d;
            if (double.TryParse(untilNext, out d)) {
                return d;
            }

            throw new Exception("Invalid type.");
        }

        private static string getChars(int start, int count, string data) {
            string buff = "";
            for (int i = start; i<start+count; i++) {
                if (i>=data.Length) break;
                buff+=data[i];
            }
            return buff;
        }

        private static string unserializeString(ref int index, string data, bool allowSpecialChars = false) {
            if (data[index]!='"') throw new Exception("Invalid string, expected \"\"\" at index " + index);
            string key = "";
            index++;
            for (int i = index; i<data.Length; i++) {
                char c = data[i];
                if (c=='"') {
                    break;
                }
                if (!allowSpecialChars&&isSpecialCharacter(c)) throw new Exception("Illegal special character \""+c+"\" at index " + index);
                key+=c;
                if(c == '\\') {
                    key += data[i+1];
                    i+=1;
                }
            }
            index+=key.Length;
            if (data[index]!='"') throw new Exception("String missing last quotation.");
            index++;
            return key;
        }

        private static object[] unserializeArray(ref int index, string data) {
            //Very similar to unserializeObject
            List<object> values = new List<object>();//Lists are easier
            index=skipIfWhitespace(index, data);
            if (data[index]==']') {
                index++;
                return values.ToArray();
            }
            while (true) {
                index=skipIfWhitespace(index, data);

                object value = unserializeValue(ref index, data);//Get value
                values.Add(value);//Append Value

                index=skipIfWhitespace(index, data);
                if (index>=data.Length) throw new Exception("Invalid JSON");//Validate

                if (data[index]==',') {//Another element in the array
                    index++;
                    continue;
                }
                if (data[index]==']') {//End of the array.
                    index++;
                    break;
                }
                throw new Exception("Invalid JSON");//Something else?
            }
            return values.ToArray();
        }
    }
}
