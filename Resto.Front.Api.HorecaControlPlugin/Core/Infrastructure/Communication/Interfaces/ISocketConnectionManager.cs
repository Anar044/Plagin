using Resto.Front.Api.HorecaControlPlugin.Dto;
using Resto.Front.Api.HorecaControlPlugin.Dto.Events;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Communication.Interfaces
{
    /// <summary>
    /// Интерфейс для управления WebSocket соединением (синхронный)
    /// </summary>
    public interface ISocketConnectionManager : IDisposable
    {
        /// <summary>
        /// Подключиться к серверу (синхронно)
        /// </summary>
        /// <param name="maxRetries">Максимальное количество попыток подключения</param>
        void Connect(int maxRetries = 5);

        /// <summary>
        /// Отключиться от сервера (синхронно)
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Отправить событие на сервер (синхронно)
        /// </summary>
        /// <param name="evt">Событие для отправки</param>
        /// <returns>true если успешно отправлено, false если добавлено в очередь</returns>
        bool SendEvent(PluginToServerEvent evt);

        /// <summary>
        /// Отправить сообщение на сервер (синхронно)
        /// </summary>
        /// <param name="message">Сообщение для отправки</param>
        /// <returns>true если успешно отправлено, false если добавлено в очередь</returns>
        bool SendMessage(PluginEventData message);

        /// <summary>
        /// Отправить накопленные неотправленные события
        /// </summary>
        void SendUnsentEvents();

        /// <summary>
        /// Отправить накопленные неотправленные сообщения
        /// </summary>
        void SendUnsentMessages();

        /// <summary>
        /// Проверить, подключен ли клиент
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Событие подключения
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        /// Событие отключения
        /// </summary>
        event EventHandler<string> Disconnected;

        /// <summary>
        /// Событие ошибки
        /// </summary>
        event EventHandler<string> Error;

        /// <summary>
        /// Событие успешного реконнекта
        /// </summary>
        event EventHandler<int> Reconnected;
    }
}

