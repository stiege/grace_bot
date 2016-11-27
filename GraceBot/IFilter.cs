using System.Threading.Tasks;

namespace GraceBot
{
    internal interface IFilter
    {
        Task<bool> FilterAsync(IExtendedActivity activity);
    }
}