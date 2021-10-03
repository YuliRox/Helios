using System.Collections.Generic;

namespace Helios
{
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
                if (this.HeliosListenTopic != null)
                    yield return this.HeliosListenTopic;
                if (this.DimmerOnOffStatusTopic != null)
                    yield return this.DimmerOnOffStatusTopic;
                if (this.DimmerPercentageStatusTopic != null)
                    yield return this.DimmerPercentageStatusTopic;
            }
        }

        public IEnumerable<string> CommandTopics
        {
            get
            {
                if (this.DimmerOnOffCommandTopic != null)
                    yield return this.DimmerOnOffCommandTopic;
                if (this.DimmerPercentageCommandTopic != null)
                    yield return this.DimmerPercentageCommandTopic;
            }
        }
    }
}