using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace VectorMath
{
	public sealed partial class Plugin
	{
		public static float DistanceTo(Vector a, Vector b)
		{
			return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
		}

		public static Vector GetEyePosition(CCSPlayerController player)
		{
			Vector absorigin = player.PlayerPawn.Value!.AbsOrigin!;
			CPlayer_CameraServices camera = player.PlayerPawn.Value!.CameraServices!;

			return new Vector(absorigin.X, absorigin.Y, absorigin.Z + camera.OldPlayerViewOffsetZ);
		}

		public static Vector NormalizeVector(Vector v)
		{
			float magnitude = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
			if (magnitude > 0)
			{
				return new Vector(v.X / magnitude, v.Y / magnitude, v.Z / magnitude);
			}
			return v;
		}

		public static void BounceBack(CCSPlayerController? controller)
		{
			if (controller == null || controller is not { IsValid: true } || controller.PlayerPawn.Value == null)
				return;

			Vector vel = controller.PlayerPawn.Value.AbsVelocity;
			double speed = Math.Sqrt(vel.X * vel.X + vel.Y * vel.Y);

			vel *= -350 / (float)speed;
			vel.Z = vel.Z <= 0 ? 150 : Math.Min(vel.Z, 150);
			controller.PlayerPawn.Value.Teleport(velocity: vel);
		}
	}
}