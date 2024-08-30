using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MABasketballTournament
{
	class PairJsonConverter : JsonConverter<Tuple<int, int>>
	{
		public override Tuple<int, int> Read(ref Utf8JsonReader Reader, Type TypeToConvert, JsonSerializerOptions Options)
		{
			string[]? SPair = Reader.GetString()?.Split("-");
			if(SPair == null)
				throw new Exception("JSON format error");
			
			return Tuple.Create(int.Parse(SPair[0]), int.Parse(SPair[1]));
		}
		
		public override void Write(Utf8JsonWriter Writer, Tuple<int, int> Value, JsonSerializerOptions Options)
		{
			throw new NotImplementedException();
		}
	}
	
	class MyDateJsonConverter : JsonConverter<DateOnly>
	{
		public override DateOnly Read(ref Utf8JsonReader Reader, Type TypeToConvert, JsonSerializerOptions Options)
		{
			return DateOnly.ParseExact(Reader.GetString() ?? "", "dd/MM/yy", CultureInfo.InvariantCulture);
		}
		
		public override void Write(Utf8JsonWriter Writer, DateOnly Value, JsonSerializerOptions Options)
		{
			throw new NotImplementedException();
		}
	}
	
	class ISOCodeJsonConverter : JsonConverter<ISOCode>
	{
		public override ISOCode Read(ref Utf8JsonReader Reader, Type TypeToConvert, JsonSerializerOptions Options)
		{
			return (ISOCode)Enum.Parse(typeof(ISOCode), Reader.GetString() ?? "");
		}
		
		public override void Write(Utf8JsonWriter Writer, ISOCode Value, JsonSerializerOptions Options)
		{
			throw new NotImplementedException();
		}
	}
}