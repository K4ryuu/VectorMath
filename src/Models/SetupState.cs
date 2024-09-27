using CounterStrikeSharp.API.Modules.Utils;

namespace VectorMath.Models
{
	public class SetupState
	{
		public Vector? PositionA { get; set; }
		public Vector? PositionB { get; set; }
		public SetupType Type { get; set; }

		public SetupState(SetupType type)
		{
			Type = type;
		}
	}

	public enum SetupType
	{
		Zone,
		Wall,
		Line
	}
}
