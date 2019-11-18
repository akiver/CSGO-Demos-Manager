using System.Windows.Media;
using GalaSoft.MvvmLight;

namespace Manager.Models
{
	public class PlayerColor : ObservableObject
	{
		public string Name { get; set; }

        public string Team { get; set; }

        public SolidColorBrush Color { get; set; }

		public override bool Equals(object obj)
		{
			var item = obj as PlayerColor;

			return item != null && Name == item.Name && Color.Equals(item.Color);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}