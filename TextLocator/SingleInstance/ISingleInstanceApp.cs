using System.Collections.Generic;

namespace TextLocator.SingleInstance
{
    public interface ISingleInstanceApp
    {
        bool SignalExternalCommandLineArgs(IList<string> args);
    }
}
