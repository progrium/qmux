namespace qmux.mux;

using gostdlib.errors;

public interface IChannel
{
    // Read reads up to len(data) bytes from the channel.
    public (int, errors.Error?) Read(byte[] data);

    // Write writes len(data) bytes to the channel.
    public (int, errors.Error?) Write(byte[] data);

    // Close signals end of channel use. No data may be sent after this
    // call.
    public errors.Error? Close();

    // CloseWrite signals the end of sending data.
    // The other side may still send data
    public errors.Error? CloseWrite();

    // ID returns the unique identifier of this channel
    // within the session
    public nuint ID();
}