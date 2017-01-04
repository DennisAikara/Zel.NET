namespace Zel.Classes
{
    public class ConcurrentData<T> : IConcurrentData<T> {
        public string UniqueIdentifier { get; }
        public T Data { get; }

        public ConcurrentData(T data, string uniqueIdentifier)
        {
            Data = data;
            UniqueIdentifier = uniqueIdentifier;
        }
    }
}