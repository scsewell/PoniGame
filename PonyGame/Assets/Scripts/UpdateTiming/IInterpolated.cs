public interface IInterpolated<T>
{
    T ReadOriginal();
    void AffectOriginal(T value);
    T GetInterpolatedValue(T older, T newer, float interpolationFactor);
    bool AreDifferent(T first, T second);
}