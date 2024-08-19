namespace MetadataChange
{
    public class StreamFileAbstraction(string name, Stream stream) : TagLib.File.IFileAbstraction
    {
        private readonly Stream Stream = stream;

        public string Name { get; } = name;

        public Stream ReadStream => Stream;

        public Stream WriteStream => Stream;

        public void CloseStream(Stream stream)
        {
            // Nothing must be done to the original stream
        }
    }
}