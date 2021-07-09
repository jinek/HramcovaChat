using System.Threading;
using System.Threading.Tasks;

namespace ChatHelpers
{
    /// <summary>
    /// Абстракция чтения/записи текста пользователя
    /// Нужна так как в консольном приложении читаем с консоли, а в вебе читаем из текстового поля в html
    /// </summary>
    public interface IUiInputOutput
    {
        Task<string> InputAsync(CancellationToken cancellationToken);
        void Output(string text);
    }
}