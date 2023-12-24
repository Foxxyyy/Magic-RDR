using System;

namespace Magic_RDR.RPF
{
    public class RPF6Report : EventArgs
    {
        public string StatusOperationText { get; set; }

        public string StatusText { get; set; }

        public string TitleText { get; set; }

        public int CurrentOperation { get; set; }

        public int TotalOperations { get; set; }

        public int Percent => TotalOperations == 0 ? 100 : CurrentOperation * 100 / TotalOperations;
    }
}
