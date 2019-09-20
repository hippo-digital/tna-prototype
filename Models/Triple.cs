using System;

namespace KnowledgeGraphBuilder
{
    public class Triple : IEquatable<Triple>
    {
        public string Subject { get; set; }
        public string Predicate { get; set; }
        public string Object { get; set; }

        public bool Equals(Triple other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Subject == other.Subject && Predicate == other.Predicate && Object == other.Object;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Triple) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Subject != null ? Subject.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Predicate != null ? Predicate.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Object != null ? Object.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}