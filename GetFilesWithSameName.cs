using MetadataChange;

namespace MetadataChange
{
    /// <summary>
    /// The class that permits to get a dictionary of files with the same name.
    /// </summary>
    public class GetFilesWithSameName
    {
        /// <summary>
        /// A Dictionary that contains the file name as a key, and a list of TagInfoContainer that have that name as the value
        /// </summary>
        public Dictionary<string, List<TagInfoContainer>> allowedValues = [];
        public GetFilesWithSameName(List<TagInfoContainer> Container)
        {
            foreach (TagInfoContainer infoContainer in Container)
            {
                string outputDirectory = infoContainer.file.Substring(0, infoContainer.file.LastIndexOf("."));
                if (allowedValues.TryGetValue(outputDirectory, out List<TagInfoContainer>? value)) value.Add(infoContainer); else allowedValues[outputDirectory] = [infoContainer];

            }
        }
    }
}