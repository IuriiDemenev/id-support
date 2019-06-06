using System;

namespace ID.SupportDatabase.Models
{
    [Serializable]
    public class Connection : ICloneable, IEquatable<Connection>
    {
        public string DataSource { get; set; }
        public string Catalog { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public object Clone()
        {
            return new Connection
            {
                DataSource = DataSource,
                Catalog = Catalog,
                UserId = UserId,
                Password = Password
            };
        }

        public bool Equals(Connection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(DataSource, other.DataSource) && string.Equals(Catalog, other.Catalog) && string.Equals(UserId, other.UserId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Connection) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (DataSource != null ? DataSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Catalog != null ? Catalog.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UserId != null ? UserId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
