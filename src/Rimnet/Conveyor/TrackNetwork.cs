using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class TrackNetwork : IExposable
    {
        private HashSet<Building_Track> belts = new HashSet<Building_Track>();
        private List<Building_Track> flowOrder = new List<Building_Track>();
        public HashSet<Building_Track> Belts => belts.ToHashSet();
        protected bool _isCurrentlyTicking = false;
        public bool IsTicking => _isCurrentlyTicking;
        public float Speed = 1;

        public void NetworkTick()
        {
            _isCurrentlyTicking = true;
            ProcessHandoffsForAllBelts();
            UpdateItemPositionsForAllBelts();
            _isCurrentlyTicking = false;
        }

        private void ProcessHandoffsForAllBelts()
        {
            foreach (var belt in belts)
            {
                belt.ProcessItemHandoffs();
            }
        }

        private void UpdateItemPositionsForAllBelts()
        {
            foreach (var belt in belts)
            {
                belt.UpdateItemPositions();
            }
        }

        public bool AddBelt(Building_Track belt)
        {
            if (!CanAddToTrack(belt))
            {
                return false;
            }

            belts.Add(belt);
            flowOrder.Add(belt);
            belt.SetNetwork(this);

            UpdateNeighboringSpatialCaches(belt);
            UpdateAllConnections();
            return true;
        }

        public void RemoveBelt(Building_Track belt)
        {
            var spatialData = belt.GetSpatialData();
            belt.SetNetwork(null);
            belts.Remove(belt);
            flowOrder.Remove(belt);
            UpdateNeighboringSpatialCaches(belt);
            UpdateAllConnections();
        }

        private void UpdateNeighboringSpatialCaches(Building_Track belt)
        {
            belt.UpdateSpatialCache();
            var spatialData = belt.GetSpatialData();
            spatialData.NORTH?.UpdateSpatialCache();
            spatialData.EAST?.UpdateSpatialCache();
            spatialData.SOUTH?.UpdateSpatialCache();
            spatialData.WEST?.UpdateSpatialCache();
        }

        public virtual bool CanAddToTrack(Thing thing)
        {
            return true;
        }

        private bool IsNotDeadEnd(Building_Track candidate, Building_Track previous)
        {
            return candidate.GetAdjacentBelts().Count(b => b != previous && belts.Contains(b)) > 0;
        }

        private Building_Track SelectNextBelt(Building_Track current, int currentIndex)
        {
            var preferredBelt = current.GetPreferredNextBelt();
            if (preferredBelt != null)
            {
                return belts.Contains(preferredBelt) ? preferredBelt : null;
            }

            var adjacentInNetwork = current.GetAdjacentBelts().Where(b => belts.Contains(b)).ToList();
            if (!adjacentInNetwork.Any()) return null;

            for (int i = currentIndex + 1; i < flowOrder.Count; i++)
            {
                var candidate = flowOrder[i];
                if (adjacentInNetwork.Contains(candidate) && IsNotDeadEnd(candidate, current))
                {
                    return candidate;
                }
            }

            for (int i = currentIndex + 1; i < flowOrder.Count; i++)
            {
                var candidate = flowOrder[i];
                if (adjacentInNetwork.Contains(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        public void UpdateAllConnections()
        {
            foreach (var belt in belts)
            {
                belt.ClearConnections();
            }

            for (int i = 0; i < flowOrder.Count; i++)
            {
                var current = flowOrder[i];
                var next = SelectNextBelt(current, i);
                if (next != null)
                {
                    current.cachedNextBelt = next;
                    next.cachedPrevBelt = current;
                }
            }
        }

        public void SetBelts(HashSet<Building_Track> newBelts)
        {
            this.belts = newBelts;
            flowOrder.Clear();
            flowOrder.AddRange(newBelts);
            UpdateAllConnections();
        }

        private void TryEnqueue(Building_Track belt, HashSet<Building_Track> unclaimed, Queue<Building_Track> queue, HashSet<Building_Track> found)
        {
            if (unclaimed.Contains(belt))
            {
                queue.Enqueue(belt);
                unclaimed.Remove(belt);
                found.Add(belt);
                flowOrder.Add(belt);
            }
        }

        public HashSet<Building_Track> Discover(Building_Track start, HashSet<Building_Track> unclaimed)
        {
            var found = new HashSet<Building_Track>();
            flowOrder.Clear();
            var queue = new Queue<Building_Track>();

            TryEnqueue(start, unclaimed, queue, found);

            while (queue.Any())
            {
                var current = queue.Dequeue();
                this.belts.Add(current);
                current.SetNetwork(this);

                var preferredNext = current.GetPreferredNextBelt();
                if (preferredNext != null)
                {
                    TryEnqueue(preferredNext, unclaimed, queue, found);
                }

                foreach (var neighbor in current.GetAdjacentBelts())
                {
                    var neighborPreferred = neighbor.GetPreferredNextBelt();
                    if (neighborPreferred == current || neighborPreferred == null)
                    {
                        TryEnqueue(neighbor, unclaimed, queue, found);
                    }
                }
            }

            UpdateAllConnections();
            return found;
        }

        private void DiscoverIncomingFlow(Building_Track current, Queue<Building_Track> queue, HashSet<Building_Track> found)
        {
            foreach (var neighbor in current.GetAdjacentBelts())
            {
                if (belts.Contains(neighbor) && !found.Contains(neighbor))
                {
                    var neighborPreferred = neighbor.GetPreferredNextBelt();
                    if (neighborPreferred == current || neighborPreferred == null)
                    {
                        queue.Enqueue(neighbor);
                        found.Add(neighbor);
                    }
                }
            }
        }

        private void DiscoverOutgoingFlow(Building_Track current, Queue<Building_Track> queue, HashSet<Building_Track> found)
        {
            var currentPreferred = current.GetPreferredNextBelt();
            if (currentPreferred != null && belts.Contains(currentPreferred) && !found.Contains(currentPreferred))
            {
                queue.Enqueue(currentPreferred);
                found.Add(currentPreferred);
            }
            else if (currentPreferred == null)
            {
                foreach (var neighbor in current.GetAdjacentBelts())
                {
                    if (belts.Contains(neighbor) && !found.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        found.Add(neighbor);
                    }
                }
            }
        }

        public HashSet<Building_Track> DiscoverFrom(Building_Track start)
        {
            var found = new HashSet<Building_Track>();
            var queue = new Queue<Building_Track>();
            if (belts.Contains(start))
            {
                queue.Enqueue(start);
                found.Add(start);
            }

            while (queue.Any())
            {
                var current = queue.Dequeue();
                DiscoverIncomingFlow(current, queue, found);
                DiscoverOutgoingFlow(current, queue, found);
            }
            return found;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref belts, "belts", LookMode.Reference);
            Scribe_Collections.Look(ref flowOrder, "flowOrder", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if(belts == null) belts = new HashSet<Building_Track>();
                if(flowOrder == null) flowOrder = new List<Building_Track>();
                belts.RemoveWhere(b => b == null || b.Destroyed);
                flowOrder.RemoveAll(b => b == null || b.Destroyed);
            }
        }
    }
}