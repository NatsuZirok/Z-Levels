﻿using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace ZLevels
{
    public class MapParent_ZLevel : MapParent
    {
        public bool shouldBeDeleted = false;
        public IntVec3 PlayerStartSpot = IntVec3.Invalid;

        public bool hasCaves = false;

        public int Z_LevelIndex = 0;

        public bool finishedGeneration = true;

        public List<InfestationData> TotalInfestations;
        public MapParent_ZLevel()
        {

        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<InfestationData>(ref this.TotalInfestations, "TotalInfestations", LookMode.Deep, null);
            Scribe_Values.Look<bool>(ref this.shouldBeDeleted, "shouldBeDeleted", false, false);
            Scribe_Values.Look<bool>(ref this.hasCaves, "hasCaves", false, false);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo current in base.GetGizmos())
            {
                yield return current;
            }
            yield break;
        }

        protected override bool UseGenericEnterMapFloatMenuOption
        {
            get
            {
                return false;
            }
        }

        public void Abandon()
        {
            this.shouldBeDeleted = true;
            this.CheckRemoveMapNow();
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = false;
            bool result = false;
            if (this.shouldBeDeleted)
            {
                result = true;
                alsoRemoveWorldObject = true;
            }
            return result;
        }
    }
}

