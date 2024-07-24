using RigidBodyPhysics.RuntimeObjects.Joints;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BridgeBuilderControl.Controls.Simulator.Model.Forcetracking
{
    //Schaut wie stark die Stäbe von der Brücke auf Zug/Druck belastet werden
    //-> Dient dazu, um zu ermitteln, welche maximale Zug-Druckkraft auf die Brücke gewirkt hat -> Zur Ausgabe im UnitTest
    //-> Ermittelt all die Distanzjoints, wo innerhalb der letzten N TimeSteps die MaxForce überschritten wurde -> Um die Brücke zu zerstören wenn die Kraft zu groß wird
    internal class BridgeForceTracker
    {
        class Entry
        {
            public float[] Forces { get; set; }
        }

        public class MinMaxValue
        {
            public float MinValue { get; set; }
            public float MaxValue { get; set; }
        }

        private List<Entry> entries = new List<Entry>();
        private IPublicDistanceJoint[] bridgeDistanceJoints;

        public BridgeForceTracker(IPublicDistanceJoint[] bridgeDistanceJoints)
        {
            this.bridgeDistanceJoints = bridgeDistanceJoints;
        }

        public IPublicDistanceJoint[] GetAllDistanceJointsWhereTheMaxForceIsReachedForMoreThenNLastSteps(int n)
        {
            if (this.entries.Count < n) return new IPublicDistanceJoint[0];

            List< IPublicDistanceJoint > maxIsReachedJoints  = new List< IPublicDistanceJoint >();

            //Prüfe für jedes Distanzjoint
            for (int i = 0; i < this.bridgeDistanceJoints.Length; i++)
            {
                float minForce = this.bridgeDistanceJoints[i].MinForceToBreak;
                float maxForce = this.bridgeDistanceJoints[i].MaxForceToBreak;

                //Prüfe die letzten n Einträge
                int maxIsReachedCounter = 0;
                for (int j = this.entries.Count - n; j < this.entries.Count; j++)
                {
                    float force = this.entries[j].Forces[i];
                    
                    bool isMaxForceReached = force < minForce ||force > maxForce;
                    if (isMaxForceReached) maxIsReachedCounter++;
                }
                if (maxIsReachedCounter == n)
                {
                    maxIsReachedJoints.Add(this.bridgeDistanceJoints[i]);
                }
            }

            return maxIsReachedJoints.ToArray();
        }

        public void AddSample()
        {
            float[] forces = this.bridgeDistanceJoints.Select(x => x.AccumulatedImpulse).ToArray();
            entries.Add(new Entry { Forces = forces });
        }

        public MinMaxValue[] GetMinMaxValueFromEachJoint()
        {
            var values = new MinMaxValue[bridgeDistanceJoints.Length];

            for (int i=0; i<values.Length; i++)
            {
                values[i] = new MinMaxValue()
                {
                    MinValue = float.MaxValue,
                    MaxValue = float.MinValue
                };
            }

            foreach (var entry in entries)
            {
                float[] forces = entry.Forces;
                for (int i = 0; i < values.Length; i++)
                {
                    values[i].MinValue = Math.Min(values[i].MinValue, forces[i]);
                    values[i].MaxValue = Math.Max(values[i].MaxValue, forces[i]);
                }
            }

            return values;
        }

        public MinMaxValue[] GetMinMaxValuesForEachTimeStep()
        {
            return this.entries.Select(x => new MinMaxValue(){ MinValue = x.Forces.Min(), MaxValue = x.Forces.Max() }).ToArray();
        }

        public int GetSampleCount()
        {
            return entries.Count;
        }

        public MinMaxValue GetMinMax()
        {
            var minMaxValues = GetMinMaxValueFromEachJoint();

            var minMax = new MinMaxValue()
            {
                MinValue = float.MaxValue,
                MaxValue = float.MinValue
            };

            for (int i=0;i<minMaxValues.Length;i++)
            {
                minMax.MinValue = Math.Min(minMax.MinValue, minMaxValues[i].MinValue);
                minMax.MaxValue = Math.Max(minMax.MaxValue, minMaxValues[i].MaxValue);
            }

            return minMax;
        }
    }
}
