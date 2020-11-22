using System.Threading.Tasks;

namespace WebTest.Services
{
    public interface ITestService
    {
        Task<string> GetAsync();
    }
}