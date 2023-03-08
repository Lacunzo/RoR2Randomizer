namespace RoR2Randomizer.Utility.Catalog
{
    public interface ICatalogIdentifier<T, TIdentifier>
    {
        int Index { get; set; }

        bool IsValid { get; }

        bool Matches(T value);

        bool Equals(in TIdentifier other, bool compareIndex);

        T CreateInstance();
    }
}
