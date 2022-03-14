namespace qmux.mux;

using gostdlib.errors;

internal static class ErrorMessages
{
    public static string SessionDoesNotSupportWaiting = "session does not support waiting";
}
public static class Mux
{
    // Wait blocks until the session transport has shut down, and returns the
    // error causing the shutdown.
    public static Error? Wait(Session sess)
    {
        var w = sess as Waiter;
        if (w == null)
        {
            return new Error(ErrorMessages.SessionDoesNotSupportWaiting);
        }

        return w.Wait();
    }
}