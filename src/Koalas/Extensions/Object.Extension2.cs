// #nullable enable
// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.Linq;
// using System.Security.Cryptography;
// using System.Text;
// using Newtonsoft.Json;
// using static System.FormattableString;

// namespace Microsoft.ProgramSynthesis.Utils.Extensions;

// public static partial class ObjectExtensions
// {
//     public static string ToLiteral(this object obj, Dictionary<object, int> identityCache = null)
//     {
//         if (obj == null)
//             return "null";

//         if (obj is bool)
//         {
//             var literal = (bool)obj;
//             return literal ? "true" : "false";
//         }

//         if (obj is byte)
//         {
//             var literal = (byte)obj;
//             return Invariant($"{literal}");
//         }

//         if (obj is sbyte)
//         {
//             var literal = (sbyte)obj;
//             return Invariant($"{literal}");
//         }

//         if (obj is short)
//         {
//             var literal = (short)obj;
//             return Invariant($"{literal}");
//         }

//         if (obj is ushort)
//         {
//             var literal = (ushort)obj;
//             return Invariant($"{literal}");
//         }

//         if (obj is int)
//         {
//             var literal = (int)obj;
//             return Invariant($"{literal}");
//         }

//         if (obj is uint)
//         {
//             var literal = (uint)obj;
//             return Invariant($"{literal}U");
//         }

//         if (obj is long)
//         {
//             var literal = (long)obj;
//             return Invariant($"{literal}L");
//         }

//         if (obj is ulong)
//         {
//             var literal = (ulong)obj;
//             return Invariant($"{literal}UL");
//         }

//         if (obj is double)
//         {
//             var literal = (double)obj;
//             return Invariant($"{literal:R}");
//         }

//         if (obj is float)
//         {
//             var literal = (float)obj;
//             return Invariant($"{literal}F");
//         }

//         if (obj is decimal)
//         {
//             var literal = (decimal)obj;
//             return Invariant($"{literal}M");
//         }

//         if (obj is char)
//         {
//             var literal = (char)obj;
//             return literal == '\\' ? "'\\\\'"
//                 : literal == '\n' ? "'\\n'"
//                 : literal == '\'' ? "'\\''"
//                 : Invariant($"'{literal}'");
//         }

//         return obj.NonPrimitiveToLiteral(identityCache);
//     }

//     private static string NonPrimitiveToLiteral(
//         this object obj,
//         Dictionary<object, int> identityCache
//     )
//     {
//         var str = obj as string;
//         if (str != null)
//         {
//             return ObjectDisplay.FormatLiteral(str);
//         }

//         if (obj is Regex)
//             return Invariant($"/{obj.ToString().ToLiteral().Slice(1, -1)}/");

//         if (obj is DateTime dt)
//         {
//             return dt.ToString(
//                 dt == dt.Date ? "yyyy-MM-dd"
//                     : dt is { Second: 0, Millisecond: 0 } ? "yyyy-MM-ddTHH:mm"
//                     : dt.Millisecond == 0 ? "yyyy-MM-ddTHH:mm:ss"
//                     : "yyyy-MM-ddTHH:mm:ss.fff",
//                 CultureInfo.InvariantCulture
//             );
//         }

//         var renderableLiteral = obj as IRenderableLiteral;
//         if (renderableLiteral != null)
//         {
//             return renderableLiteral.RenderHumanReadable();
//         }

//         Type t = obj.GetType();
//         int? arity = t.GetRecordArity();
//         if (arity.HasValue)
//         {
//             object[] recordItems = Enumerable
//                 .Range(0, arity.Value)
//                 .Select(obj.GetRecordItem)
//                 .ToArray();
//             return recordItems.DumpCollection(
//                 openDelim: "(",
//                 closeDelim: ")",
//                 identityCache: identityCache
//             );
//         }

//         if (obj is IDictionary)
//             return obj.ToEnumerable()
//                 .DumpCollection(identityCache: identityCache, openDelim: "{", closeDelim: "}");
//         if (obj is IEnumerable)
//             return obj.ToEnumerable().DumpCollection(identityCache: identityCache);

//         var optional = obj as IOptional;
//         if (optional != null)
//             return optional.HasValue ? Invariant($"Some {optional.Value.ToLiteral()}") : "Nothing";

//         if (
//             obj.GetType().GetTypeInfo().IsGenericType
//             && (obj.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
//         )
//         {
//             string key = obj.GetPropertyValue(nameof(KeyValuePair<int, int>.Key))
//                 .ToLiteral(identityCache);
//             string value = obj.GetPropertyValue(nameof(KeyValuePair<int, int>.Value))
//                 .ToLiteral(identityCache);
//             return Invariant($"{key}: {value}");
//         }

//         return obj.InternedFormat(identityCache);
//     }
// }
