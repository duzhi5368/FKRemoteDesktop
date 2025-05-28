//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Structs
{
    public class SBuildOptions
    {
        public string Version { get; set; }
        public string OutputPath { get; set; }

        public string RawHosts { get; set; }
        public int Delay { get; set; }
        public string Tag { get; set; }
        public string Mutex { get; set; }

        public bool Install { get; set; }
        public bool Startup { get; set; }
        public bool HideFile { get; set; }
        public bool Keylogger { get; set; }

        public bool IsUseCopySignature { get; set; }
        public string CopySignaturePath { get; set; }
        public bool IsUseCopyAsmInfoPath { get; set; }
        public string CopyAsmInfoPath { get; set; }
        public string[] AssemblyInformation { get; set; }
        public bool IsUseCustomIconPath { get; set; }
        public string IconPath { get; set; }
        public string CopyIconInfoPath { get; set; }
    }
}
