using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MABasketballTournament
{
	class TeamGroup
	{
		public readonly string Name;
		
		public Dictionary<Team, GroupRecord> TeamInfo { get; private set; } = new Dictionary<Team, GroupRecord>();
		
		public TeamGroup(string Name, IEnumerable<Team> AllTeams) : this(Name, AllTeams.ToArray()) { }
		
		public TeamGroup(string Name, params Team[] AllTeams)
		{
			this.Name = Name;
			foreach(Team Tm in AllTeams)
				TeamInfo.Add(Tm, new GroupRecord());
		}
		
		// Sorts teams by points
		private int CompareByPoints(Team T1, Team T2)
		{
			return -(TeamInfo[T1].Points.CompareTo(TeamInfo[T2].Points));
		}
		
		// Sorts a list of teams based on scores from encounters only within the same list of teams (followed by the remaining FIBA criteria in order)
		private List<Team> FormCircle(List<Team> Teams)
		{
			if(Teams.Count < 2)
				return Teams;
			
			if(Teams.Count == 2)
			{
				GameResult Encounter = TeamInfo[Teams[0]].MatchHistory[Teams[1]];
				
				if(Encounter.GetScore(Teams[0]) < Encounter.GetScore(Teams[1]))
				{
					Team Tmp = Teams[0];
					Teams[0] = Teams[1];
					Teams[1] = Tmp;
				}
			}
			else
			{
				Dictionary<Team, GroupRecord> LocalizedTeamInfo = new Dictionary<Team, GroupRecord>();
				
				foreach(Team Tm in Teams)
					LocalizedTeamInfo.Add(Tm, new GroupRecord());
					
				foreach(Team Tm in Teams)
				{
					var History = TeamInfo[Tm].MatchHistory;
					foreach(Team EncounteredTeam in History.Where(x => LocalizedTeamInfo.ContainsKey(x.Key)).Select(x => x.Key))
					{
						GameResult Res = History[EncounteredTeam];
						LocalizedTeamInfo[Tm].Record(Res.GetScore(Tm), Res.GetScore(EncounteredTeam), Res.TeamSurrendered(Tm), EncounteredTeam, Res);
					}
				}
				
				Teams.Sort((T1, T2) => {
					if(LocalizedTeamInfo[T1].Points != LocalizedTeamInfo[T2].Points)
						return -(LocalizedTeamInfo[T1].Points.CompareTo(LocalizedTeamInfo[T2].Points));
					else if(LocalizedTeamInfo[T1].BasketsDiff != LocalizedTeamInfo[T2].BasketsDiff)
						return -(LocalizedTeamInfo[T1].BasketsDiff.CompareTo(LocalizedTeamInfo[T2].BasketsDiff));
					else if(LocalizedTeamInfo[T1].BasketsScored != LocalizedTeamInfo[T2].BasketsScored)
						return -(LocalizedTeamInfo[T1].BasketsScored.CompareTo(LocalizedTeamInfo[T2].BasketsScored));
					else if(TeamInfo[T1].BasketsDiff != TeamInfo[T2].BasketsDiff)
						return -(TeamInfo[T1].BasketsDiff.CompareTo(TeamInfo[T2].BasketsDiff));
					else
						return -(TeamInfo[T1].BasketsScored.CompareTo(TeamInfo[T2].BasketsScored));
				});
			}
			
			return Teams;
		}
		
		// Sorts teams which have the same amount of points
		private List<Team> Tiebreak(List<Team> Teams)
		{
			return Teams.Aggregate(
				new List<List<Team>>(),
				(L, V) => {
					if(L.Count == 0 || (TeamInfo[V].Points != TeamInfo[L.Last()[0]].Points))
						L.Add(new List<Team>());
					L.Last().Add(V);
					return L;
				}
			).SelectMany(x => FormCircle(x)).ToList();
		}
		
		public Team? FindByCode(ISOCode Code)
		{
			return TeamInfo.Where(x => x.Key.Code == Code).Select(x => (Team?)(x.Key)).FirstOrDefault();
		}
		
		public List<Team> PlayAllGames()
		{
			List<Team> AllTeams = TeamInfo.Keys.ToList();
			
			for(int i = 0; i < AllTeams.Count; i++)
			{
				for(int j = i + 1; j < AllTeams.Count; j++)
				{
					Team T1 = AllTeams[i];
					Team T2 = AllTeams[j];
					
					GameResult Res = GameCalculator.Play(T1, T2);
					
					TeamInfo[T1].Record(Res.GetScore(T1), Res.GetScore(T2), Res.TeamSurrendered(T1), T2, Res);
					TeamInfo[T2].Record(Res.GetScore(T2), Res.GetScore(T1), Res.TeamSurrendered(T2), T1, Res);
				}
			}
			
			AllTeams.Sort(CompareByPoints);
			return Tiebreak(AllTeams);
		}
	}
}