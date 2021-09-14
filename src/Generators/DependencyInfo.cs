using System;

namespace Generators
{
    public class DependencyInfo : IEquatable<DependencyInfo>
    {
        public string TypeName { get; }
        public string Name { get; }

        public DependencyInfo(string typeName, string name)
        {
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public bool Equals(DependencyInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TypeName == other.TypeName && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DependencyInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (TypeName.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }
    }
}