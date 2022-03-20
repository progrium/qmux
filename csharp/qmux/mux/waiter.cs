namespace qmux.mux;

using gostdlib.errors;

public interface IWaiter
{
    public errors.Error? Wait();
}
public static class Waiter
{
    // Wait blocks until the session transport has shut down, and returns the
    // error causing the shutdown.
    public static errors.Error? Wait(ISession sess)
    {
        var w = sess as IWaiter;
        if (w == null)
        {
            return new errors.Error(ErrorMessages.SessionDoesNotSupportWaiting);
        }

        return w.Wait();
    }
}

