
namespace AzureJobs.Common {
    public interface ICompressorJob {
        void ProcessQueue();
        void Run();
    }
}
