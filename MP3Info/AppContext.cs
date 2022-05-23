namespace MP3Info
{
    public class AppContext
    {
        public string Path { get; set; }
        public bool WhatIf { get; set; }
        public bool Force { get; set; }
        public bool Verbose { get; set; }
        public string OutputFile { get; set; }
    }
}
