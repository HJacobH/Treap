using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treap
{
    public class ParkingGarage
    {
        private readonly Treap<int, VehicleData>[] _floors;

        public int FloorsCount { get; } = 4;
        public int SpotsPerFloor { get; } = 12;

        public ParkingGarage()
        {
            _floors = new Treap<int, VehicleData>[FloorsCount];
            for (int i = 0; i < FloorsCount; i++)
            {
                _floors[i] = new Treap<int, VehicleData>();
            }
        }

        private bool IsValidInput(int floorIndex, int spotNumber)
        {
            return floorIndex >= 0 && floorIndex < FloorsCount &&
                   spotNumber > 0 && spotNumber <= SpotsPerFloor;
        }

        public bool OccupySpot(int floorIndex, int spotNumber, VehicleData data)
        {
            if (!IsValidInput(floorIndex, spotNumber)) return false;
            return _floors[floorIndex].Insert(spotNumber, data);
        }

        public bool FreeSpot(int floorIndex, int spotNumber)
        {
            if (!IsValidInput(floorIndex, spotNumber)) return false;
            return _floors[floorIndex].Delete(spotNumber);
        }

        public bool IsSpotOccupied(int floorIndex, int spotNumber)
        {
            if (!IsValidInput(floorIndex, spotNumber)) return false;
            return _floors[floorIndex].Contains(spotNumber);
        }

        public List<TreapNode<int, VehicleData>> GetOccupiedSpots(int floorIndex)
        {
            if (floorIndex < 0 || floorIndex >= FloorsCount)
                return new List<TreapNode<int, VehicleData>>();

            return _floors[floorIndex].GetElementsInOrder();
        }

        public Treap<int, VehicleData> GetFloorTreap(int floorIndex)
        {
            return _floors[floorIndex];
        }
        public int FindNearestFreeSpot(int floorIndex, int requestedSpot)
        {
            if (!IsValidInput(floorIndex, requestedSpot))
                throw new ArgumentException("Neplatné zadání podlaží nebo místa.");

            if (!IsSpotOccupied(floorIndex, requestedSpot))
            {
                return requestedSpot;
            }

            var floorTreap = _floors[floorIndex];

            int leftCandidate = requestedSpot;
            while (true)
            {
                var pred = floorTreap.GetPredecessor(leftCandidate);
                if (pred != null && pred.Key == leftCandidate - 1)
                {
                    leftCandidate = pred.Key;
                }
                else
                {
                    leftCandidate--;
                    break;
                }
            }

            int rightCandidate = requestedSpot;
            while (true)
            {
                var succ = floorTreap.GetSuccessor(rightCandidate);
                if (succ != null && succ.Key == rightCandidate + 1)
                {
                    rightCandidate = succ.Key;
                }
                else
                {
                    rightCandidate++;
                    break;
                }
            }

            bool isLeftValid = leftCandidate >= 1;
            bool isRightValid = rightCandidate <= SpotsPerFloor;

            if (isLeftValid && isRightValid)
            {
                int leftDist = requestedSpot - leftCandidate;
                int rightDist = rightCandidate - requestedSpot;

                return (leftDist <= rightDist) ? leftCandidate : rightCandidate;
            }
            else if (isLeftValid)
            {
                return leftCandidate;
            }
            else if (isRightValid)
            {
                return rightCandidate;
            }

            return -1;
        }
    }
}
