public class RegistryTransformException : Exception
{
    public RegistryTransformException() : base() { }

    public RegistryTransformException(string message)
        : base(message) { }

    public RegistryTransformException(string message, Exception innerException)
        : base(message, innerException) { }
}
