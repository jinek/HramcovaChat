using System.Threading.Tasks;

namespace ChatContract
{
    public interface IUiInputOutput
    {
        Task<string> InputAsync();
        void Output(string text);
    }
}