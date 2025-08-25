namespace MetadataChange
{
    /// <summary>
    /// Parameters used to control the information shown in the Progress Toast.
    /// </summary>
    public class ProgressToastParams
    {
        /// <summary>
        /// The title to show in the Toast
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// The current progress
        /// </summary>
        public int? Progress { get; set; }
        /// <summary>
        /// The maximum of the progress bar
        /// </summary>
        public int? Max { get; set; }
        /// <summary>
        /// Action to run after one of these parameters has been changed.
        /// Note that this action should remain unset, since it'll be overwritten by the Toast when loaded.
        /// </summary>
        public Action? UpdateState { get; set; }
        /// <summary>
        /// Action to run when the user wants to stop the operation.
        /// If nothing is passed, the "Stop" button won't be visible.
        /// </summary>
        public Action? StopFetching { get; set; }
    }
}