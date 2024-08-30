using System;

namespace MABasketballTournament
{
	class GameResult
	{
		public readonly Team Team1;
		public readonly Team Team2;
		public readonly DateOnly Date;
		
		private readonly Tuple<int, int> Score;
		private readonly SurrenderState Surrender;
		
		public Team Winner
		{
			get
			{
				if(GetScore(Team1) > GetScore(Team2))
					return Team1;
				return Team2;
			}
		}
		
		public Team Loser
		{
			get
			{
				if(GetScore(Team1) < GetScore(Team2))
					return Team1;
				return Team2;
			}
		}
		
		public override string ToString()
		{
			string BaseString = $"{Team1.Name} VS {Team2.Name} ({GetScore(Team1)} : {GetScore(Team2)})";
			string SurrenderString = "";
			
			if(Surrender == SurrenderState.Team1)
				SurrenderString = $" [{Team1.Name} surrendered]";
			else if(Surrender == SurrenderState.Team2)
				SurrenderString = $" [{Team2.Name} surrendered]";
			
			if(SurrenderString.Length > 0)
				return BaseString + SurrenderString;
			return BaseString;
		}
		
		public GameResult(Team Team1, Team Team2, DateOnly Date, Tuple<int, int> Score, SurrenderState Surrender)
		{
			this.Team1 = Team1;
			this.Team2 = Team2;
			this.Date = Date;
			this.Score = Score;
			this.Surrender = Surrender;
		}
		
		public GameResult(Team Team1, Team Team2, PartialGameResult PG) : this(Team1, Team2, PG.Date, PG.Score, SurrenderState.Neither) { }
		
		public int GetScore(Team Tm)
		{
			if(Tm.Equals(Team1))
				return Score.Item1;
			else if(Tm.Equals(Team2))
				return Score.Item2;
			return -1;
		}
		
		public bool TeamSurrendered(Team Tm)
		{
			return (Tm.Equals(Team1) && Surrender == SurrenderState.Team1) || (Tm.Equals(Team2) && Surrender == SurrenderState.Team2);
		}
	}
}