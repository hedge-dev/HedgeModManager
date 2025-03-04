namespace HedgeModManager.CoreLib;

public interface IProgress<T>
{
    void Report(T value);
    void ReportAdd(T value);
    void ReportMax(T value);
}
