namespace qmux.mux;

public interface Channel
{
    // Read reads up to len(data) bytes from the channel.
    public int Read(byte[] data);

    // Write writes len(data) bytes to the channel.
    public int Write(byte[] data);

    // Close signals end of channel use. No data may be sent after this
    // call.
    public void Close();

    // CloseWrite signals the end of sending data.
    // The other side may still send data
    public void CloseWrite();

    // ID returns the unique identifier of this channel
    // within the session
    public UInt32 ID();
}