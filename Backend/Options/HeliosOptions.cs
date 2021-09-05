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

        public IEnumerable<string> ListenTopics
        {
            get
            {
                yield return this.HeliosListenTopic;
                yield return this.DimmerOnOffStatusTopic;
                yield return this.DimmerPercentageStatusTopic;
            }
        }


        public IEnumerable<string> CommandTopics
        {
            get
            {
                yield return this.DimmerOnOffCommandTopic;
                yield return this.DimmerPercentageCommandTopic;
            }
        }
    }
}