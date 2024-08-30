using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MABasketballTournament
{
	class PartialGameResult
	{
		[JsonConverter(typeof(MyDateJsonConverter))]
		public readonly DateOnly Date;
		
		[JsonPropertyName("Opponent")]
		[JsonConverter(typeof(ISOCodeJsonConverter))]
		public readonly ISOCode Code;
		
		[JsonPropertyName("Result")]
		[JsonConverter(typeof(PairJsonConverter))]
		public readonly Tuple<int, int> Score;
		
		[JsonConstructor]
		public PartialGameResult(DateOnly Date, ISOCode Code, Tuple<int, int> Score)
		{
			this.Date = Date;
			this.Code = Code;
			this.Score = Score;
		}
	}
}