namespace qmux.codec;

using gostdlib.errors;
using gostdlib.io;

internal static class ErrorMessages
{
    public static string MarshalObjectFailed = "marshal object failed";
    public static string UnmarshalObjectFailed = "unmarshal object failed";
    public static string ObjectAsMarshalerFailed = "object v failed to cast to type Marshaler";
    public static string UnmarshalUnsupportedField = "unmarshal unsupported filed";
}

public static class Codec
{
    public static IWriter? DebugMessages;
    public static IWriter? DebugBytes;
    public static (byte[], Error?) Marshal(object v)
    {
        if (!(v.GetType() == typeof(Marshaler)))
        {
            return (new byte[] { }, new Error($"{ErrorMessages.MarshalObjectFailed} was={v.GetType()} expected={typeof(Marshaler)}"));
        }

        var m = v as Marshaler;
        if (m == null)
        {
            return (new byte[] { }, new Error(ErrorMessages.ObjectAsMarshalerFailed));
        }
        return m.MarshalMux();
    }
    public static Error? Unmarshal(byte[] b, object v)
    {
        if (!(v.GetType() == typeof(Unmarshaler)))
        {
            return new Error($"{ErrorMessages.UnmarshalObjectFailed} was={v.GetType()} expected={typeof(Unmarshaler)}");
        }
        var u = v as Unmarshaler;
        if (u == null)
        {
            return new Error($"{ErrorMessages.UnmarshalUnsupportedField} for value={v.GetType()}");
        }

        return u.UnmarshalMux(b);
    }
}

public interface Marshaler
{
    public (byte[], Error?) MarshalMux();
}

public interface Unmarshaler
{
    public Error? UnmarshalMux(byte[] b);
}

