using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace VectorMath
{
	public sealed partial class Plugin
	{
		readonly Vector vecEntity = new Vector(50, 50, 20);
		public Dictionary<CCSPlayerController, CPointWorldText> playerMessages = [];

		public void RegisterHudMessage(CCSPlayerController player, string text)
		{
			var entity = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext");
			if (entity == null || !entity.IsValid) return;

			QAngle vAngle = new QAngle(player.PlayerPawn.Value!.AbsRotation!.X, player.PlayerPawn.Value.AbsRotation.Y, player.PlayerPawn.Value.AbsRotation.Z);

			player.Pawn.Value!.Teleport(player.PlayerPawn.Value.AbsOrigin, new QAngle(0, 0, 0), player.PlayerPawn.Value.AbsVelocity);

			entity.Teleport(new Vector(
				player.PlayerPawn.Value.AbsOrigin!.X + vecEntity.X,
				player.PlayerPawn.Value.AbsOrigin.Y + vecEntity.Y,
				player.PlayerPawn.Value.AbsOrigin.Z + vecEntity.Z
			),
			new QAngle(0, 270, 75), player.PlayerPawn.Value.AbsVelocity);

			entity.FontSize = 48;
			//entity.FontName = "Consolas";
			entity.Enabled = true;
			entity.Fullbright = true;
			entity.WorldUnitsPerPx = 0.1f;
			entity.Color = System.Drawing.Color.Yellow;
			entity.MessageText = text;
			entity.DispatchSpawn();
			entity.AcceptInput("SetParent", player.PlayerPawn.Value, null, "!activator");

			player.Pawn.Value?.Teleport(player.PlayerPawn.Value.AbsOrigin, vAngle, player.PlayerPawn.Value.AbsVelocity);

			if (playerMessages.ContainsKey(player) && playerMessages[player].IsValid)
				playerMessages[player].Remove();

			playerMessages[player] = entity;
		}

		public void UnregisterHudMessage(CCSPlayerController player)
		{
			if (playerMessages.TryGetValue(player, out CPointWorldText? value))
			{
				if (value.IsValid)
					value.Remove();

				playerMessages.Remove(player);
			}
		}
	}
}