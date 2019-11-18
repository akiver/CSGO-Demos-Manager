using System.Collections.Generic;
using System.Drawing;

namespace Core.Models.Source
{
	public abstract class Source
	{
		public string Name { get; set; }

		public string Label { get; set; }

		public Image Logo {
			get
			{
				switch (Name)
				{
					case Ebot.NAME:
						return Properties.Resources.ebot;
					case Esea.NAME:
						return Properties.Resources.esea;
					case Faceit.NAME:
						return Properties.Resources.faceit;
					case Cevo.NAME:
						return Properties.Resources.cevo;
					case Pov.NAME:
						return Properties.Resources.pov;
					case Esl.NAME:
						return Properties.Resources.esl;
					case PopFlash.NAME:
						return Properties.Resources.popflash;
					case Wanmei.NAME:
						return Properties.Resources.wanmei;
					case PugSetup.NAME:
						return Properties.Resources.pugsetup;
					default:
						return Properties.Resources.valve;
				}
			}
		}

		public static Source Factory(string name)
		{
			switch (name)
			{
				case Valve.NAME:
					return new Valve();
				case Esea.NAME:
					return new Esea();
				case Ebot.NAME:
					return new Ebot();
				case Pov.NAME:
					return new Pov();
				case PugSetup.NAME:
					return new PugSetup();
				case Faceit.NAME:
					return new Faceit();
				case Cevo.NAME:
					return new Cevo();
				case PopFlash.NAME:
					return new PopFlash();
				case Esl.NAME:
					return new Esl();
				case Wanmei.NAME:
					return new Wanmei();
				default:
					return null;
			}
		}

		public static List<Source> Sources = new List<Source>
		{
			new Cevo(),
			new Ebot(),
			new Esea(),
			new Esl(),
			new Faceit(),
			new PopFlash(),
			new Valve(),
			new Wanmei(),
			new Pov(),
			new PugSetup(),
		};
	}
}
