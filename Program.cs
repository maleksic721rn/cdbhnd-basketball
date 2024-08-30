using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MABasketballTournament
{
	class Program
	{
		private const int ROUND_COUNT = 3;
		
		// Utility function for finding a team across all groups
		private static Team? FindTeam(List<TeamGroup> Groups, ISOCode Code)
		{
			return Groups.Select(x => x.FindByCode(Code)).FirstOrDefault(x => x != null, null);
		}
		
		static void Main(string[] Args)
		{
			try
			{
				string GroupsJson = File.ReadAllText("groups.json", Encoding.UTF8);
				
				JsonSerializerOptions JsonOptions = new JsonSerializerOptions();
				JsonOptions.IncludeFields = true;
				
				List<TeamGroup>? Groups = JsonSerializer.Deserialize<Dictionary<string, List<Team>>>(GroupsJson, JsonOptions)?.Select(x => new TeamGroup(x.Key, x.Value)).ToList();
				if(Groups == null)
					throw new Exception("Incorrect group format");
				
				// File name isn't a typo
				if(File.Exists("exibitions.json"))
				{
					string ExhibitionsJson = File.ReadAllText("exibitions.json", Encoding.UTF8);
					
					Dictionary<string, List<PartialGameResult>>? Exhibitions = JsonSerializer.Deserialize<Dictionary<string, List<PartialGameResult>>>(ExhibitionsJson, JsonOptions);
					if(Exhibitions == null)
						throw new Exception("Incorrect exhibition format");
					
					// Simulates finished exhibition matches before the group phase
					foreach(var KVP in Exhibitions)
					{
						foreach(PartialGameResult P in KVP.Value)
						{
							Team? T1 = FindTeam(Groups, (ISOCode)Enum.Parse(typeof(ISOCode), KVP.Key));
							if(T1 == null)
								continue;
							
							Team? T2 = FindTeam(Groups, P.Code);
							if(T2 == null)
								continue;
							
							GameResult Res = new GameResult(T1, T2, P);
							
							T1.AdjustForm(Res);
							T2.AdjustForm(Res);
						}
					}
				}
				
				List<GroupStanding> GroupStandings = new List<GroupStanding>();
				
				Console.WriteLine("Group phase:");
				
				foreach(TeamGroup CGroup in Groups)
				{
					Console.WriteLine();
					Console.WriteLine($"Group {CGroup.Name}:");
					Console.WriteLine();
					
					List<Team> GroupResult = CGroup.PlayAllGames();
					
					for(int i = 0; i < GroupResult.Count; i++)
					{
						for(int j = i + 1; j < GroupResult.Count; j++)
						{
							Team T1 = GroupResult[i];
							Team T2 = GroupResult[j];
							
							Console.WriteLine(CGroup.TeamInfo[T1].MatchHistory[T2].ToString());
						}
					}
					
					Console.WriteLine();
					Console.WriteLine("Final standings:");
					Console.WriteLine();
					
					for(int i = 0; i < GroupResult.Count; i++)
					{
						Team Tm = GroupResult[i];
						GroupRecord Rec = CGroup.TeamInfo[Tm];
						Console.WriteLine($"{i + 1}. {Tm.Name.PadRight(20, ' ')} {Rec.Wins} / {Rec.Losses} / {Rec.Points} / {Rec.BasketsScored} / {Rec.BasketsReceived} / {Rec.BasketsDiff}");
					}
					
					GroupStandings.Add(new GroupStanding(GroupResult, CGroup));
					Console.WriteLine();
				}
				
				Console.WriteLine("Pots:");
				
				var Pots = GameCalculator.MakePots(GroupStandings, ROUND_COUNT);
				
				for(int i = 0; i < Pots.Count; i += 2)
				{
					Console.WriteLine();
					Console.WriteLine($"Pot {(i / 2) + 1}:");
					Console.WriteLine();
					Console.WriteLine($"{Pots[i].Item1.Name}");
					Console.WriteLine($"{Pots[i + 1].Item1.Name}");
				}
				
				Console.WriteLine();
				Console.WriteLine("Elimination Phase:");
				Console.WriteLine();
				
				var CurrentRound = GameCalculator.PairTeams(Pots);
				int RoundsRemaining = ROUND_COUNT;
				
				Team? First = null, Second = null, Third = null;
				
				while(RoundsRemaining > 0)
				{
					Console.WriteLine();
					switch(RoundsRemaining)
					{
						case 3:
							Console.WriteLine("Quarterfinals:");
							break;
						case 2:
							Console.WriteLine("Semifinals:");
							break;
						case 1:
							Console.WriteLine("Finals:");
							break;
						default:
							Console.WriteLine($"Round {ROUND_COUNT - RoundsRemaining + 1}:");
							break;
					}
					Console.WriteLine();
					
					List<GameResult> RoundResults = new List<GameResult>();
					
					foreach(var Pair in CurrentRound)
					{
						GameResult Res = GameCalculator.Play(Pair.Item1, Pair.Item2);
						Console.WriteLine(Res.ToString());
						RoundResults.Add(Res);
					}
					
					CurrentRound = new List<Tuple<Team, Team>>();
					
					if(RoundsRemaining > 1)
						for(int i = 0; i < RoundResults.Count; i += 2)
							CurrentRound.Add(Tuple.Create(RoundResults[i].Winner, RoundResults[i + 1].Winner));
					
					if(RoundsRemaining == 2)
					{
						Console.WriteLine();
						Console.WriteLine("Third place game:");
						Console.WriteLine();
						
						Team T1 = RoundResults[0].Loser;
						Team T2 = RoundResults[1].Loser;
						
						GameResult Res = GameCalculator.Play(T1, T2);
						Console.WriteLine(Res.ToString());
						Third = Res.Winner;
					}
					else if(RoundsRemaining == 1)
					{
						Second = RoundResults[0].Loser;
						First = RoundResults[0].Winner;
					}
					
					RoundsRemaining--;
					Console.WriteLine();
				}
				
				Console.WriteLine();
				Console.WriteLine("Medals:");
				Console.WriteLine();
				
				Console.WriteLine($"1. {First?.Name}");
				Console.WriteLine($"2. {Second?.Name}");
				Console.WriteLine($"3. {Third?.Name}");
			}
			catch(Exception Ex)
			{
				Console.WriteLine(Ex.Message);
			}
		}
	}
}