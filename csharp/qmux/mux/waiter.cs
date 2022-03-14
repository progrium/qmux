namespace qmux.mux;

using gostdlib.errors;

public partial class mux
{
    public interface IWaiter
    {
        public errors.Error? Wait();
    }
}