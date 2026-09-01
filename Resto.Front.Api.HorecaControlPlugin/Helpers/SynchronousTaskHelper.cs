using System;
using System.Threading.Tasks;

namespace Resto.Front.Api.HorecaControlPlugin.Helpers
{
    /// <summary>
    /// Helper класс для безопасной синхронной работы с Task
    /// </summary>
    public static class SynchronousTaskHelper
    {
        /// <summary>
        /// Синхронно ожидает завершения Task с таймаутом
        /// </summary>
        public static T WaitForResult<T>(Task<T> task, TimeSpan timeout)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var timeoutTask = Task.Delay(timeout);
            var completedTask = Task.WaitAny(task, timeoutTask);

            if (completedTask == 0)
            {
                // task завершился
                task.ConfigureAwait(false).GetAwaiter().GetResult();

                // В .NET Framework 4.7.2 нет IsCompletedSuccessfully, используем проверку через Status
                if (task.Status == TaskStatus.RanToCompletion && !task.IsFaulted && !task.IsCanceled)
                {
                    return task.Result;
                }
                else if (task.IsFaulted)
                {
                    throw task.Exception?.GetBaseException() ?? new Exception("Task faulted");
                }
                else if (task.IsCanceled)
                {
                    throw new OperationCanceledException("Task was canceled");
                }
            }

            throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
        }

        /// <summary>
        /// Синхронно ожидает завершения Task без результата с таймаутом
        /// </summary>
        public static void WaitForCompletion(Task task, TimeSpan timeout)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var timeoutTask = Task.Delay(timeout);
            var completedTask = Task.WaitAny(task, timeoutTask);

            if (completedTask == 0)
            {
                task.ConfigureAwait(false).GetAwaiter().GetResult();

                if (task.IsFaulted)
                {
                    throw task.Exception?.GetBaseException() ?? new Exception("Task faulted");
                }
                else if (task.IsCanceled)
                {
                    throw new OperationCanceledException("Task was canceled");
                }
            }
            else
            {
                throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds} seconds");
            }
        }

        /// <summary>
        /// Синхронно ожидает завершения Task без таймаута
        /// </summary>
        public static T WaitForResult<T>(Task<T> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.ConfigureAwait(false).GetAwaiter().GetResult();

            // В .NET Framework 4.7.2 нет IsCompletedSuccessfully, используем проверку через Status
            if (task.Status == TaskStatus.RanToCompletion && !task.IsFaulted && !task.IsCanceled)
            {
                return task.Result;
            }
            else if (task.IsFaulted)
            {
                throw task.Exception?.GetBaseException() ?? new Exception("Task faulted");
            }
            else if (task.IsCanceled)
            {
                throw new OperationCanceledException("Task was canceled");
            }

            throw new Exception("Task did not complete successfully");
        }

        /// <summary>
        /// Синхронно ожидает завершения Task без результата и без таймаута
        /// </summary>
        public static void WaitForCompletion(Task task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.ConfigureAwait(false).GetAwaiter().GetResult();

            if (task.IsFaulted)
            {
                throw task.Exception?.GetBaseException() ?? new Exception("Task faulted");
            }
            else if (task.IsCanceled)
            {
                throw new OperationCanceledException("Task was canceled");
            }
        }
    }
}

