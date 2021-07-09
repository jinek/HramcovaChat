using System.Threading;
using System.Threading.Tasks;

namespace ChatHelpers
{
    public interface IUiInputOutput
    {
        Task<string> InputAsync(CancellationToken cancellationToken);
        void Output(string text);
    }
}