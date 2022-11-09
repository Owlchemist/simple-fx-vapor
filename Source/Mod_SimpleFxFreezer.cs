using Verse;
using HarmonyLib;
using UnityEngine;
using static SimpleFxVapor.ModSettings_SimpleFxVapor;
 
namespace SimpleFxVapor
{
    public class Mod_SimpleFxVapor : Mod
	{
		public Mod_SimpleFxVapor(ModContentPack content) : base(content)
		{
			new Harmony(this.Content.PackageIdPlayerFacing).PatchAll();
			base.GetSettings<ModSettings_SimpleFxVapor>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard options = new Listing_Standard();
			options.Begin(inRect);
			options.CheckboxLabeled("SimpleFxVapor.Settings.ConsiderOutdoors".Translate(), ref considerOutdoors, "SimpleFxVapor.Settings.ConsiderOutdoors.Desc".Translate());
			options.End();
			base.DoSettingsWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "Simple FX: Vapor";
		}

		public override void WriteSettings()
		{
			base.WriteSettings();
		}
	}
	public class ModSettings_SimpleFxVapor : ModSettings
	{
		public override void ExposeData()
		{
			Scribe_Values.Look<bool>(ref considerOutdoors, "considerOutdoors", false, false);
			base.ExposeData();
		}

		public static bool considerOutdoors = false;
	}
}
