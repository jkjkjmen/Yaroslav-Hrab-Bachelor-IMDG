using System;

namespace IMDG.Manager
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks>Inclusive at both ends</remarks>
    public readonly struct PartitionRange : IEquatable<PartitionRange>
    {
        public int Start { get; }
        public int End { get; }


        public PartitionRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public static PartitionRange All()
        {
            return new PartitionRange(int.MinValue, int.MaxValue);
        }


        public bool Equals(PartitionRange other) => Start == other.Start && End == other.End;

        public override bool Equals(object obj) => obj is PartitionRange other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Start, End);

        public static bool operator ==(PartitionRange left, PartitionRange right) => left.Equals(right);

        public static bool operator !=(PartitionRange left, PartitionRange right) => !left.Equals(right);

        public bool Contains(int value) => value >= Start && value <= End;
    }
}