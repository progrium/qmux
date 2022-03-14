namespace qmux.mux;

using errors = gostdlib.errors;

public partial class mux
{
    public interface IWaiter
    {
        public errors.Error? Wait();
    }
}