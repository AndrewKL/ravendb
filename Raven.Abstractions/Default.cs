using Raven.Imports.Newtonsoft.Json;
using Raven.Abstractions.Json;

namespace Raven.Abstractions
{
	public static class Default
	{
		public static readonly string[] OnlyDateTimeFormat = new[] {"yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff"};
		public static readonly string[] DateTimeFormatsToRead = new[] { "o", "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff", "yyyy-MM-ddTHH:mm:ss.fffffffzzz", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK" };
		public static readonly string DateTimeOffsetFormatsToWrite = "o";
		public static readonly string DateTimeFormatsToWrite = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff";

		public static readonly JsonConverter[] Converters = new JsonConverter[]
		{
			new JsonEnumConverter(),
			new JsonToJsonConverter(),
			new JsonDateTimeISO8601Converter(),
			new JsonDateTimeOffsetConverter(),
			new JsonDictionaryDateTimeKeysConverter(),
		};
	}
}