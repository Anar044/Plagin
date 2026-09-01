using Resto.Front.Api.Data.Security;
using Resto.Front.Api.HorecaControlPlugin.Core.Application.Services.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Persistence.Repositories.Interfaces;
using Resto.Front.Api.HorecaControlPlugin.Sqlite.Schema;
using System;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Application.Services
{
    /// <summary>
    /// Сервис для работы со сменами
    /// </summary>
    public class ShiftService : IShiftService
    {
        private readonly IRepository _repository;

        public ShiftService(IRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Shift GetOpenShift()
        {
            return _repository.GetOpenShift();
        }

        public void OpenShift(DateTime? openTime = null, IUser user = null)
        {
            PluginContext.Log.Info($"ShiftService :: Opening shift, user: {user?.Name ?? "null"}, time: {openTime ?? DateTime.Now}");
            _repository.OpenShift(openTime ?? DateTime.Now, user);
            PluginContext.Log.Info("ShiftService :: Shift opened.");
        }

        public void CloseShift(IUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            PluginContext.Log.Info($"ShiftService :: Closing shift, user: {user.Name}");
            _repository.CloseShift(user);
            PluginContext.Log.Info("ShiftService :: Shift closed.");
        }

        public bool IsShiftOpen()
        {
            var shift = GetOpenShift();
            return shift != null && shift.CloseTime == null;
        }

        public void IncrementShiftCountForOpenOrders()
        {
            PluginContext.Log.Debug("ShiftService :: Incrementing shift count for open orders...");
            _repository.IncrementShiftCountForOpenOrders();
        }
    }
}

