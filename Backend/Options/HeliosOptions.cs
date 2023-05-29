using System.Collections.Generic;

namespace Helios;

public class HeliosOptions
{
    public string HeliosListenTopic { get; set; }
    public string DimmerOnOffStatusTopic { get; set; }
    public string DimmerOnOffCommandTopic { get; set; }
    public string DimmerPercentageStatusTopic { get; set; }
    public string DimmerPercentageCommandTopic { get; set; }
    public int DimmerMinimumPercentage { get; set; }
    public int DimmerTime { get; set; }

    public IEnumerable<string> ListenTopics
    {
        get
        {
            if (HeliosListenTopic != null)
                yield return HeliosListenTopic;
            if (DimmerOnOffStatusTopic != null)
                yield return DimmerOnOffStatusTopic;
            if (DimmerPercentageStatusTopic != null)
                yield return DimmerPercentageStatusTopic;
        }
    }

    public IEnumerable<string> CommandTopics
    {
        get
        {
            if (DimmerOnOffCommandTopic != null)
                yield return DimmerOnOffCommandTopic;
            if (DimmerPercentageCommandTopic != null)
                yield return DimmerPercentageCommandTopic;
        }
    }
}