namespace qmux.mux;

using errors = gostdlib.errors;

public static partial class mux
{
    internal static class ErrorMessages
    {
        public static string SessionDoesNotSupportWaiting = "session does not support waiting";
    }
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