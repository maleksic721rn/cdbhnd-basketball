using System;
using System.Collections.Generic;

namespace MABasketballTournament
{
	class GroupRecord
	{
		public int Wins { get; private set; } = 0;
		public int Losses { get; private set; } = 0;
		public int Points { get; private set; } = 0;
		public int BasketsScored { get; private set; } = 0;
		public int BasketsReceived { get; private set; } = 0;
		
		public int BasketsDiff { get { return BasketsScored - BasketsReceived; } }
		
		public Dictionary<Team, GameResult> MatchHistory { get; private set; } = new Dictionary<Team, GameResult>();
		
		public void Record(int Scored, int Received, bool Surrender, Team Opponent, GameResult MatchRecord)
		{
			if(Surrender)
				Losses++;
			else if(Scored < Received)
			{
				Losses++;
				Points++;
			}
			else if(Scored > Received)
			{
				Wins++;
				Points += 2;
			}
			
			BasketsScored += Scored;
			BasketsReceived += Received;
			
			MatchHistory.Add(Opponent, MatchRecord);
		}
	}
}