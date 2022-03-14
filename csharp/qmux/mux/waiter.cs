namespace qmux.mux;

using gostdlib.errors;

public interface Waiter
{
    public Error? Wait();
}