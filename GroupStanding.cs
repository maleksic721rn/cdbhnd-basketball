using System;
using System.Collections.Generic;

namespace MABasketballTournament 
{
	class GroupStanding
	{
		public readonly List<Team> Rankings;
		public readonly TeamGroup GroupInfo;
		
		public GroupStanding(List<Team> Rankings, TeamGroup GroupInfo)
		{
			this.Rankings = Rankings;
			this.GroupInfo = GroupInfo;
		}
		
		public GroupRecord GetRecordByTeamPlacement(int Placement)
		{
			if(Placement < 0 || Placement >= Rankings.Count)
				throw new Exception("Invalid placement");
			
			return GroupInfo.TeamInfo[Rankings[Placement]];
		}
	}
}