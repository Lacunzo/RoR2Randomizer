namespace RoR2Randomizer.Utility
{
    public interface ICatalogIdentifier<T, TIdentifier>
    {
        int Index { get; set; }

        bool IsValid { get; }

        bool Matches(T value);

        bool Equals(TIdentifier other, bool compareIndex);

        T CreateInstance();
    }
}
