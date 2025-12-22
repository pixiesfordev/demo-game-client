namespace Shiko.Serialize
{
    public interface ISerializer : IMarshaler, IUnmarshaler { }

    public interface IMarshaler
    {
        byte[] Marshal(object obj);
    }

    public interface IUnmarshaler
    {
        void Unmarshal(byte[] data, object obj);
    }
}