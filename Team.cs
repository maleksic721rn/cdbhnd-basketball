using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace MABasketballTournament
{
	class Team : IEquatable<Team>
	{
		private const double FORM_ADJUSTMENT = 0.007;
		
		[JsonPropertyName("Team")]
		public readonly string Name;
		
		[JsonConverter(typeof(ISOCodeJsonConverter))]
		[JsonPropertyName("ISOCode")]
		public readonly ISOCode Code;
		
		[JsonInclude]
		[JsonPropertyName("FIBARanking")]
		public int Rank { get; private set; }
		
		[JsonIgnore]
		private Dictionary<Team, double> Form = new Dictionary<Team, double>();
		
		// This function always needs to be called before form is updated
		private void CheckForm(Team Tm)
		{	
			if(!Form.ContainsKey(Tm))
				Form.Add(Tm, FORM_ADJUSTMENT * (Tm.Rank - Rank));
		}
		
		public override bool Equals(object? Obj)
		{
			if(Obj == null)
				return false;
			
			return Equals(Obj as Team);
		}
		
		public override int GetHashCode()
		{
			return Code.GetHashCode();
		}
		
		public override string ToString()
		{
			return $"{Name} ({Code}) - {Rank}";
		}
		
		public bool Equals(Team? Other)
		{
			if(Other == null)
				return false;
			
			return Code == Other.Code;
		}
		
		[JsonConstructor]
		public Team(string Name, ISOCode Code, int Rank)
		{
			this.Name = Name;
			this.Code = Code;
			this.Rank = Rank;
		}
		
		public double GetForm(Team Tm)
		{
			CheckForm(Tm);
			return Form[Tm];
		}
		
		// Changes the odds of victory against a specific team
		// Losing to a lower-ranked team will decrease them more substantially and vice-versa
		public void AdjustForm(GameResult Result)
		{
			Team Opponent;
			if(Result.Team1.Code == this.Code)
				Opponent = Result.Team2;
			else
				Opponent = Result.Team1;
			
			int OwnScore = Result.GetScore(this);
			int OpponentScore = Result.GetScore(Opponent);
			
			if(OwnScore == -1)
				throw new Exception("Cannot adjust results based on a game the team didn't participate in");
			
			int VictoryCoefficient = Result.Winner.Equals(this) ? 1 : -1;
			
			CheckForm(Opponent);
			
			if((Rank > Opponent.Rank && VictoryCoefficient == 1) || (Rank < Opponent.Rank && VictoryCoefficient == -1))
				Form[Opponent] += FORM_ADJUSTMENT * VictoryCoefficient;
			else
				Form[Opponent] += FORM_ADJUSTMENT * VictoryCoefficient * Math.Abs(Rank - Opponent.Rank);
		}
	}
}