namespace MetadataChange
{
    /// <summary>
    /// The container of all the selected files
    /// </summary>
    /// <param name="_tag">The TagLib.File of the selected resource</param>
    /// <param name="_name">The file name</param>
    /// <param name="_id">An unique identifier of the container</param>
    public class TagInfoContainer(TagLib.File _tag, string _name, string _id)
    {
        /// <summary>
        /// The TagLib.File of the selected resource
        /// </summary>
        public TagLib.File tag = _tag;
        /// <summary>
        /// The file name
        /// </summary>
        public string file = _name;
        /// <summary>
        /// An unique identifier of the container
        /// </summary>
        public string id = _id;
    }
}