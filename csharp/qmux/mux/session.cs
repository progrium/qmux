namespace qmux.mux;

using gostdlib.context;

public interface Session
{
    // Close closes the underlying transport.
    // Any blocked Accept operations will be unblocked and return errors.
    public void Close();

    // Open establishes a new channel with the other end.
    public Channel Open(IContext context);

    // Accept waits for and returns the next incoming channel.  public Channel Accept();
    public Channel Accept();
}