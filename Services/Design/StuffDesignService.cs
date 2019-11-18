using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using Services.Interfaces;

namespace Services.Design
{
	public class StuffDesignService : IStuffService
	{
		public Task<List<Stuff>> GetStuffPointListAsync(Demo demo, StuffType type)
		{
			List<Stuff> stuffs = new List<Stuff>();
			Random r = new Random();
			for (int i = 0; i < 30; i++)
			{
                var p = new Player
                {
                    Name = "Player " + r.Next(1, 10)
                };
                p.EnableUpdates();

                stuffs.Add(new Stuff
				{
					RoundNumber = r.Next(20),
					Tick = r.Next(10000),
					Type = StuffType.FLASHBANG,
					ThrowerName = "Player " + r.Next(1, 10),
					FlashedPlayers = new List<Player>
					{
						p
					}
				});
			}

			return Task.FromResult(stuffs);
		}
	}
}
