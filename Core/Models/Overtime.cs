using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace Core.Models
{
	public class Overtime : ObservableObject
	{
		private int _number;

		/// <summary>
		/// Score of the team that started the match as CT
		/// </summary>
		private int _scoreTeamCt;

		/// <summary>
		/// Score of the team that started the match as T
		/// </summary>
		private int _scoreTeamT;

		[JsonProperty("number")]
		public int Number
		{
			get { return _number; }
			set { Set(ref _number, value); }
		}

		[JsonProperty("score_team_ct")]
		public int ScoreTeamCt
		{
			get { return _scoreTeamCt; }
			set { Set(ref _scoreTeamCt, value); }
		}

		[JsonProperty("score_team_t")]
		public int ScoreTeamT
		{
			get { return _scoreTeamT; }
			set { Set(ref _scoreTeamT, value); }
		}
	}
}
