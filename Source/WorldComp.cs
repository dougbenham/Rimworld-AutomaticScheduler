using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace AutomaticScheduler
{
    class WorldComp : WorldComponent
    {
        // Using a HashSet for quick lookup
        public static HashSet<Pawn> PawnsWithScheduleAdjusted = new HashSet<Pawn>();
        // I've found it easier to have a null list for use when exposing data
        // This shouldn't be needed but mods will remove Pawns from the game completely (RuntimeGC for instance)
        // and HashSet will fail if more than one null value is added.
        private List<Pawn> usedForExposingData = null;

        public WorldComp(World w) : base(w)
        {
            // Make sure the static HashSet is cleared whenever a game is created or loaded.
            PawnsWithScheduleAdjusted.Clear();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                // When saving, populate the list
                usedForExposingData = new List<Pawn>(PawnsWithScheduleAdjusted);
            }

            Scribe_Collections.Look(ref usedForExposingData, "pawnsWithScheduleAdjusted", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // When loading, clear the HashSet then populate it with the loaded data
                PawnsWithScheduleAdjusted.Clear();
                foreach (var v in usedForExposingData)
                {
                    // Remove any null records
                    if (v != null)
                    {
                        PawnsWithScheduleAdjusted.Add(v);
                    }
                }
            }

            if (Scribe.mode == LoadSaveMode.Saving ||
                Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                // Add hints to the garbage collector that this memory can be collected
                usedForExposingData?.Clear();
                usedForExposingData = null;
            }
        }
    }
}