namespace MetadataChange
{
    public class CustomMetadataFormat
    {
        /// <summary>
        /// Gets the container TagLibSharp has identified for the current file
        /// </summary>
        /// <param name="type">The Type of the TagLib.Tag</param>
        /// <returns>A CustomMetadataType of the container, if a supported container for custom metadata is available</returns>
        public static CustomMetadataTypes? GetSuggestedContainer(string type)
        {
            string[] permittedTypes = System.Enum.GetNames(typeof(CustomMetadataTypes));
            foreach (string availableType in permittedTypes)
            {
                if (type.Contains(availableType.ToLower())) return (CustomMetadataTypes)Enum.Parse(typeof(CustomMetadataTypes),
                availableType);
            }
            return null;
        }
        /// <summary>
        /// The different types of container that support custom metadata
        /// </summary>
        public enum CustomMetadataTypes
        {
            APE,
            APPLE,
            ASF,
            ID3,
            MATROSKA,
            XIPH,
            PNG,
            RIFF,
            XMP
        }


    }
}