namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Communication.Interfaces
{
    /// <summary>
    /// Интерфейс для управления реконнектом
    /// </summary>
    public interface IReconnectionService
    {
        /// <summary>
        /// Попытка автоматического реконнекта
        /// </summary>
        void AttemptAutoReconnect();

        /// <summary>
        /// Попытка реконнекта с указанным количеством попыток
        /// </summary>
        /// <param name="maxRetries">Максимальное количество попыток</param>
        void AttemptReconnect(int maxRetries = 5);
    }
}

