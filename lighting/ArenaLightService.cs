using Godot;
using ShopIsDone.Arenas;
using ShopIsDone.Core;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Utils.Extensions;
using System;
using System.Linq;
using Godot.Collections;
using System.Threading.Tasks;

namespace ShopIsDone.Lighting
{
	// This service is used for turning off and on lights in the state en masse
	public partial class ArenaLightService : Node, IService
	{
		[Export]
		public Color ClownColor;

		[Export]
		private Arena _Arena;

		private Dictionary<WorldLight, Color> _LightColors = new Dictionary<WorldLight, Color>();

		public bool AreLightsSet()
		{
			return _LightColors.Count > 0;
		}

		public async Task SetAllLightsToColorAsync()
		{
			// Clear
			_LightColors.Clear();

			// Get arena lights
			var lights = GetArenaLights();

			// Cannot create empty tweens, so return early if no lights
			if (lights.Count == 0)
			{
				await ToSignal(GetTree(), "process_frame");
				return;
			}

			// Create tween for light color
			var tween = GetTree().CreateTween().Parallel();
			foreach (var light in GetArenaLights())
			{
				// Persist the light colors in the arena light
				_LightColors.Add(light, light.LightColor);
				// Add the light to the tween
				tween.Parallel().TweenProperty(light, nameof(light.LightColor), ClownColor, 1f);
			}
			await ToSignal(tween, "finished");
		}

		public async Task RevertAllLightColorsAsync()
		{
			if (_LightColors.Count == 0)
			{
                await ToSignal(GetTree(), "process_frame");
                return;
            }

			// Create tween for light color
			var tween = GetTree().CreateTween().Parallel();
			foreach (var lightKv in _LightColors)
			{
				var light = lightKv.Key;
				var toColor = lightKv.Value;
				// Add the light to the tween
				tween.Parallel().TweenProperty(light, nameof(light.LightColor), toColor, 1f);
			}
			// Clear out tracking
			_LightColors.Clear();
			// Await finishing
			await ToSignal(tween, "finished");
		}

		private Array<WorldLight> GetArenaLights()
		{
            return GetTree()
				.GetNodesInGroup("arena_lights")
				.OfType<WorldLight>()
				.Where(_Arena.IsAncestorOf)
				.ToGodotArray();
        }
	}
}
