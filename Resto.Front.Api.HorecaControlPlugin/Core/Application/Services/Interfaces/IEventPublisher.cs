using Resto.Front.Api.HorecaControlPlugin.Dto.Events;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces
{
    /// <summary>
    /// Интерфейс для публикации событий
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Опубликовать событие
        /// </summary>
        void PublishEvent(PluginToServerEvent evt);

        /// <summary>
        /// Отправить накопленные неотправленные события
        /// </summary>
        void SendUnsentEvents();
    }
}

