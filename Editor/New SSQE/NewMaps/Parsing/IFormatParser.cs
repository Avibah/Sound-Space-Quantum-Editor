namespace New_SSQE.NewMaps.Parsing
{
    internal interface IFormatParser
    {
        /// <summary>
        /// Optional helper method for formats for determining whether a path or data is of the layout the format expects, especially for pure JSON formats
        /// </summary>
        /// <param name="data">The string being loaded to validate</param>
        /// <returns>Whether the data is valid for the given format</returns>
        static virtual bool IsValid(string data) => throw new NotImplementedException();

        /// <summary>
        /// Reads in data from a file given a valid file path, loading map objects into CurrentMap and recording metadata in Settings
        /// </summary>
        /// <param name="path">The file path to read map data from</param>
        /// <returns>Whether the operation succeeded</returns>
        static abstract bool Read(string path);

        /// <summary>
        /// Optionally, a format may allow reading directly from data instead of a file, reading into <see cref="Mapping.Current"/> and <see cref="Preferences.Settings"/> like <see cref="Read(string)"/>
        /// </summary>
        /// <param name="data">The data to read</param>
        /// <returns>Whether the operation succeeded</returns>
        static virtual bool ReadData(string data) => throw new NotImplementedException();

        /// <summary>
        /// Writes map objects from CurrentMap and applicable metadata from Settings to a file given a valid file path
        /// </summary>
        /// <param name="path">The file path to write map data to</param>
        /// <returns>Whether the operation succeeded</returns>
        static abstract bool Write(string path);

        /// <summary>
        /// Optionally, a format may include export functionality, which will allow the user to select a file to export to before writing to that file
        /// <para></para>
        /// For this to work well, the format may also want to implement metadata for use elsewhere in case Settings isn't favorable
        /// </summary>
        /// <returns>Whether the operation succeeded</returns>
        static virtual bool Export() => throw new NotImplementedException();

        /// <summary>
        /// Holds any optional metadata for the format for use in exporting - <see cref="Export()"/>
        /// <para></para>
        /// This format may also include other fields like this for additional metadata
        /// </summary>
        static Dictionary<string, string> Metadata = [];
    }
}
