using System;
using System.Collections.Generic;
using System.Linq;

namespace MABasketballTournament
{
	static class GameCalculator
	{
		private static readonly Random Rnd = new Random();
		
		public static GameResult Play(Team T1, Team T2)
		{
			int Score1 = (int)Math.Round(T1.GetForm(T2) * 50) + Rnd.Next(65, 86);
			int Score2 = (int)Math.Round(T2.GetForm(T1) * 50) + Rnd.Next(65, 86);
			
			if(Score1 == Score2)
			{
				int CoinFlip = Rnd.Next(0, 2);
				Score1 += CoinFlip;
				Score2 += 1 - CoinFlip;
			}
			
			SurrenderState Surrender = SurrenderState.Neither;
			
			if(Score1 - Score2 >= 20 && Rnd.Next(0, 100) < 5)
				Surrender = SurrenderState.Team2;
			else if(Score2 - Score1 >= 20 && Rnd.Next(0, 100) < 5)
				Surrender = SurrenderState.Team1;
			
			GameResult Res = new GameResult(T1, T2, DateOnly.FromDateTime(DateTime.Today), Tuple.Create(Score1, Score2), Surrender);
			
			T1.AdjustForm(Res);
			T2.AdjustForm(Res);
			
			return Res;
		}
		
		// Takes the rankings for a set of groups and creates pots using a certain amount of top-ranking players from each group
		// The result of this function is only meant for use with PairTeams()
		public static List<Tuple<Team, int>> MakePots(List<GroupStanding> Standings, int RoundCount)
		{
			int TeamNumber = 1 << RoundCount;
			
			if(Standings.Sum(x => x.Rankings.Count) < TeamNumber)
				throw new Exception("Invalid group standings for given round count");
			
			int CurrentRanking = 0;
			List<Tuple<Team, int>> UnpairedTeams = new List<Tuple<Team, int>>();
			
			while(UnpairedTeams.Count < TeamNumber)
			{
				List<Tuple<Team, int>> CurrentPass = new List<Tuple<Team, int>>();
				for(int i = 0; i < Standings.Count; i++)
				{
					GroupStanding CStanding = Standings[i];
					if(CurrentRanking < CStanding.Rankings.Count)
						CurrentPass.Add(Tuple.Create(CStanding.Rankings[CurrentRanking], i));
				}
				
				CurrentPass.Sort((TT1, TT2) => {
					GroupRecord T1 = Standings[TT1.Item2].GroupInfo.TeamInfo[TT1.Item1];
					GroupRecord T2 = Standings[TT2.Item2].GroupInfo.TeamInfo[TT2.Item1];
					
					if(T1.Points != T2.Points)
						return -(T1.Points.CompareTo(T2.Points));
					else if(T1.BasketsDiff != T2.BasketsDiff)
						return -(T1.BasketsDiff.CompareTo(T2.BasketsDiff));
					else
						return -(T1.BasketsScored.CompareTo(T2.BasketsScored));
				});
				
				foreach(var V in CurrentPass)
				{
					if(UnpairedTeams.Count < TeamNumber)
						UnpairedTeams.Add(V);
					else break;
				}
				
				CurrentRanking++;
			}
			
			return UnpairedTeams;
		}
		
		// Randomly pairs teams from the pots for the elimination phase, making sure no two teams from the same group are paired
		public static List<Tuple<Team, Team>> PairTeams(List<Tuple<Team, int>> UnpairedTeams)
		{
			int TeamNumber = UnpairedTeams.Count;
			
			List<Tuple<Team, Team>> PairedTeams = new List<Tuple<Team, Team>>(TeamNumber);
			
			Random Rnd = new Random();
			Exception PairingImpossible = new Exception("Pairing impossible");
			
			int NextIndex = 0;
			
			for(int i = 0; i < TeamNumber / 2; i += 2)
			{
				var VA1 = UnpairedTeams[i];
				var VA2 = UnpairedTeams[i + 1];
				var VB1 = UnpairedTeams[TeamNumber - i - 1];
				var VB2 = UnpairedTeams[TeamNumber - i - 2];
				
				Team TeamA1 = VA1.Item1;
				Team TeamA2 = VA2.Item1;
				Team TeamB1 = VB1.Item1;
				Team TeamB2 = VB2.Item1;
				
				int GroupA1 = VA1.Item2;
				int GroupA2 = VA2.Item2;
				int GroupB1 = VB1.Item2;
				int GroupB2 = VB2.Item2;
				
				int RollResult = -1;
				
				if(GroupA1 == GroupB1)
				{
					if(GroupA1 == GroupB2 || GroupA2 == GroupB1)
						throw PairingImpossible;
					else
						RollResult = 1;
				}
				else
				{
					if(GroupA1 == GroupB2)
					{
						if(GroupA2 == GroupB2)
							throw PairingImpossible;
						else
							RollResult = 0;
					}
					else
					{
						if(GroupA2 == GroupB1)
						{
							if(GroupA2 == GroupB2)
								throw PairingImpossible;
							else
								RollResult = 0;
						}
						else
						{
							if(GroupA2 == GroupB2)
								RollResult = 1;
							else
								RollResult = Rnd.Next(0, 2);
						}
					}
				}
				
				if(RollResult == 1)
				{
					PairedTeams.Insert(NextIndex, Tuple.Create(TeamA1, TeamB2));
					PairedTeams.Add(Tuple.Create(TeamA2, TeamB1));
				}
				else
				{
					PairedTeams.Insert(NextIndex, Tuple.Create(TeamA1, TeamB1));
					PairedTeams.Add(Tuple.Create(TeamA2, TeamB2));
				}
				
				NextIndex++;
			}
			
			return PairedTeams;
		}
	}
}