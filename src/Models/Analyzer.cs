using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

namespace VectorMath.Models
{
	public class PlayerInfo
	{
		public Plugin plugin;
		public CCSPlayerController Player { get; set; }
		public DateTime StartTime { get; set; } = DateTime.Now;
		public CounterStrikeSharp.API.Modules.Timers.Timer? Timer { get; set; }

		public PlayerInfo(Plugin plugin, CCSPlayerController player)
		{
			this.plugin = plugin;
			this.Player = player;

			DisplayPlayerInfo();

			Timer = plugin.AddTimer(0.01f, DisplayPlayerInfo, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
		}

		public void DisplayPlayerInfo()
		{
			if (StartTime.AddSeconds(20) <= DateTime.Now)
			{
				this.Dispose();
				return;
			}

			Vector position = Player.PlayerPawn.Value!.AbsOrigin!;
			QAngle rotation = Player.PlayerPawn.Value!.AbsRotation!;
			Vector velocity = Player.PlayerPawn.Value!.AbsVelocity!;

			string message = FormatMessage(position, rotation, velocity);

			UpdatePlayerMessage(Player, message);
		}

		private static string FormatMessage(Vector position, QAngle rotation, Vector velocity)
		{
			return $@"Játékos Információ
	1. Pozíció: ({position.X:F1}, {position.Y:F1}, {position.Z:F1})
	2. Forgás: ({rotation.X:F1}, {rotation.Y:F1}, {rotation.Z:F1})
	3. Sebesség: ({velocity.X:F1}, {velocity.Y:F1}, {velocity.Z:F1})";
		}

		private void UpdatePlayerMessage(CCSPlayerController player, string message)
		{
			if (plugin.playerMessages.TryGetValue(player, out CPointWorldText? playerValue) && playerValue.IsValid)
			{
				playerValue.MessageText = message;
				Utilities.SetStateChanged(playerValue, "CPointWorldText", "m_messageText");
			}
			else
			{
				plugin.RegisterHudMessage(player, message);
			}
		}

		public void Dispose()
		{
			Timer?.Kill();
			plugin.UnregisterHudMessage(Player);
		}
	}
}