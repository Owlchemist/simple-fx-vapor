using HarmonyLib;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
 
namespace SimpleFxVapor
{
    [HarmonyPatch(typeof(SteadyEnvironmentEffects), nameof(SteadyEnvironmentEffects.DoCellSteadyEffects))]
    static class Patch_DoCellSteadyEffects
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var mapInfo = AccessTools.Field(typeof(SteadyEnvironmentEffects), nameof(SteadyEnvironmentEffects.map));
            var rangeInfo = AccessTools.Property(typeof(Room), nameof(Room.Temperature)).GetGetMethod();
            bool ran = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
	            if (codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == rangeInfo)
	            {
		            codes.InsertRange(i + 2, new List<CodeInstruction>(){

                        new CodeInstruction(OpCodes.Ldarg_1),
						new CodeInstruction(OpCodes.Ldloc_S, 8), //temperature
						new CodeInstruction(OpCodes.Ldarg_0),
						new CodeInstruction(OpCodes.Ldfld, mapInfo),
						new CodeInstruction(OpCodes.Call, typeof(Patch_DoCellSteadyEffects).GetMethod(nameof(Patch_DoCellSteadyEffects.ColdGlow)))
                    });
                    ran = true;
                    break;
                }
            }
            if (!ran) Log.Warning("[Simple FX: Freezers] Transpiler could not find target. There may be a mod conflict, or RimWorld updated?");
            return codes.AsEnumerable();
        }

        static World worldCache = null;
        static TileTemperaturesComp compCache = null;
		static FastRandom fastRandom = new FastRandom();

        static public void ColdGlow(IntVec3 c, float temperature, Map map)
        {
            
            if (ModSettings_SimpleFxVapor.considerOutdoors)
            {
                if (worldCache == null) worldCache = Current.Game.World;
                //Check if the world has changed (loaded a new save)
                if (Current.Game.World != worldCache || compCache == null)
                {
                    worldCache = Current.Game.World;
                    compCache = worldCache?.GetComponent<TileTemperaturesComp>();
                }
                else if (compCache.cache[map.info.parent.tileInt].cachedOutdoorTemp < 0f) return;
            }
            if (temperature < 0f) 
			{
                FleckDef fleckDef;
                if (temperature < -8f) fleckDef = ResourceBank.FleckDefOf.Owl_VeryColdGlow;
                else fleckDef = ResourceBank.FleckDefOf.Owl_ColdGlow;
                
				Vector3 vector = c.ToVector3Shifted();
				if (!vector.ShouldSpawnMotesAt(map, true)) return;

				vector += 1f * new Vector3(fastRandom.Next(1,50) / 100f, 0f, fastRandom.Next(1,50) / 100f);
				
				FleckCreationData dataStatic = FleckMaker.GetDataStatic(vector, map, fleckDef, fastRandom.Next(200,300) / 100f * 1f);
				dataStatic.rotationRate = fastRandom.Next(-300,300) / 100f;
				dataStatic.velocityAngle =  (float)fastRandom.Next(0,360);
				dataStatic.velocitySpeed = 0.12f;
				map.flecks.CreateFleck(dataStatic);
			}
        }
    }
}