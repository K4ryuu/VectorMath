using System;
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

namespace VectorMath.Models
{
	public class Distance
	{
		public Plugin plugin;
		public CCSPlayerController player1 { get; set; }
		public CCSPlayerController player2 { get; set; }
		public DateTime StartTime { get; set; } = DateTime.Now;
		public CounterStrikeSharp.API.Modules.Timers.Timer? Timer { get; set; }
		public Color Color { get; set; } = Color.FromArgb(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255));
		public Beam? BeamEntity { get; set; }

		public Distance(Plugin plugin, CCSPlayerController player1, CCSPlayerController player2)
		{
			this.plugin = plugin;
			this.player1 = player1;
			this.player2 = player2;
			RenderDistance();
			Timer = plugin.AddTimer(0.1f, RenderDistance, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
		}

		public void RenderDistance()
		{
			if (StartTime.AddSeconds(20) <= DateTime.Now)
			{
				this.Dispose();
				return;
			}

			if (BeamEntity != null && BeamEntity.IsValid)
				BeamEntity.Remove();

			Vector player1Pos = player1.PlayerPawn.Value!.AbsOrigin!;
			Vector player2Pos = player2.PlayerPawn.Value!.AbsOrigin!;

			BeamEntity = new Beam(player1Pos, player2Pos, Color);

			Vector direction = new Vector(
				player2Pos.X - player1Pos.X,
				player2Pos.Y - player1Pos.Y,
				player2Pos.Z - player1Pos.Z
			);

			float distance = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);
			float horizontalDistance = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

			float yaw = (float)(Math.Atan2(direction.Y, direction.X) * (180 / Math.PI));
			float pitch = (float)(Math.Atan2(direction.Z, horizontalDistance) * (180 / Math.PI));

			string message = FormatMessage(direction, distance, horizontalDistance, yaw, pitch);

			UpdatePlayerMessage(player1, message);
			UpdatePlayerMessage(player2, message);
		}

		private string FormatMessage(Vector direction, float distance, float horizontalDistance, float yaw, float pitch)
		{
			return $@"Vektor Matematika
	1. Irányvektor: ({direction.X:F1}, {direction.Y:F1}, {direction.Z:F1})
	2. Táv: {distance:F1} | Vízsz.: {horizontalDistance:F1} | Magas.: {Math.Abs(direction.Z):F1}
	3. Yaw: {yaw:F1}° ({GetCardinalDirection(yaw)})
	4. Pitch: {pitch:F1}° ({(direction.Z > 0 ? "fel" : "le")})
	5. √({direction.X:F1}² + {direction.Y:F1}² + {direction.Z:F1}²) ≈ {distance:F1}";
		}

		private static string GetCardinalDirection(float yaw)
		{
			string[] directions = { "É", "ÉK", "K", "DK", "D", "DNY", "NY", "ÉNY" };
			return directions[(int)Math.Round(((yaw % 360) + 360) % 360 / 45) % 8];
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
			if (BeamEntity != null && BeamEntity.IsValid)
				BeamEntity.Remove();

			Timer?.Kill();

			plugin.UnregisterHudMessage(player1);
			plugin.UnregisterHudMessage(player2);
		}
	}
}