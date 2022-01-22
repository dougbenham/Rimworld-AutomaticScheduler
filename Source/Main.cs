using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace AutomaticScheduler
{
    [StaticConstructorOnStartup]
    public static class AutomaticScheduler
    {
        private static void AutoSchedule(Pawn pawn)
        {
            if (pawn?.timetable != null && !WorldComp.PawnsWithScheduleAdjusted.Contains(pawn))
            {
                if (pawn.story?.traits?.HasTrait(TraitDefOf.NightOwl) == true)
                {
                    pawn.timetable.times = new List<TimeAssignmentDef>(GenDate.HoursPerDay);
                    for (int i = 0; i < GenDate.HoursPerDay; i++)
                    {
                        pawn.timetable.times.Add(i >= 11 && i <= 18 ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything);
                    }
                }
                else
                {                    
                    pawn.timetable.times = new List<TimeAssignmentDef>(GenDate.HoursPerDay);
                    for (int i = 0; i < GenDate.HoursPerDay; i++)
                    {
                        TimeAssignmentDef setNightOwlHours = ((i >= 20 && i <= 23) || (i >= 4 && i <= 7)) ? TimeAssignmentDefOf.Sleep : TimeAssignmentDefOf.Anything;
                        pawn.timetable.times.Add(setNightOwlHours);
                    }
                }

                WorldComp.PawnsWithScheduleAdjusted.Add(pawn);
            }
        }

        [HarmonyPatch(typeof(Thing), nameof(Thing.SpawnSetup))]
        public static class Patch_Thing_SpawnSetup
        {
            // Patching for initial pawns
            public static void Postfix(Thing __instance)
            {
                if (__instance is Pawn p && p.Faction?.IsPlayer == true && p.def?.race?.Humanlike == true)
                {
                    AutoSchedule(p);
                }
            }
        }

        [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), nameof(InteractionWorker_RecruitAttempt.DoRecruit), new Type[] { typeof(Pawn), typeof(Pawn), typeof(string), typeof(string), typeof(bool), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
        public static class Patch_InteractionWorker_RecruitAttempt
        {
            // Patching for recruited prisoners
            public static void Postfix(Pawn recruiter, Pawn recruitee)
            {
                if (recruitee is Pawn p && p.Faction?.IsPlayer == true && p.def?.race?.Humanlike == true)
                {
                    AutoSchedule(p);
                }
            }
        }

        [HarmonyPatch(typeof(InteractionWorker_EnslaveAttempt), nameof(InteractionWorker_EnslaveAttempt.Interacted))]
         public static class Patch_InteractionWorker_EnslaveAttempt
         { 
             // Patching for enslaved prisoners
             public static void Postfix(Pawn initiator, Pawn recipient)
             {
                 if (recipient is Pawn p && p.GuestStatus == GuestStatus.Slave)
                 {
                     AutoSchedule(p);
                 }
             }
         }

        static AutomaticScheduler()
        {
            var harmony = new Harmony("doug.AutomaticScheduler");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [DefOf]
        public static class TraitDefOf
        {
            public static TraitDef NightOwl;
        }
    }
}