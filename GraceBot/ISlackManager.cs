using System.Net.Http;
using System.Threading.Tasks;

namespace GraceBot
{
    public interface ISlackManager
    {
        Task<bool> Forward(string msg);
    }
}