using Resto.Front.Api.Data.Security;
using Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы со сменами
    /// </summary>
    public interface IShiftService
    {
        /// <summary>
        /// Получить открытую смену
        /// </summary>
        Shift GetOpenShift();

        /// <summary>
        /// Открыть смену
        /// </summary>
        void OpenShift(DateTime? openTime = null, IUser user = null);

        /// <summary>
        /// Закрыть смену
        /// </summary>
        void CloseShift(IUser user);

        /// <summary>
        /// Проверить, открыта ли смена
        /// </summary>
        bool IsShiftOpen();

        /// <summary>
        /// Увеличить счетчик смен для открытых заказов
        /// </summary>
        void IncrementShiftCountForOpenOrders();
    }
}

