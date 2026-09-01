#if V8P5
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Resto.Front.Api.HorecaControlPlugin.Helpers
{
    internal  class UnmanagedDllPathHelper
    {
        public static string StorageDirectory
        {
            get
            {
                var programData =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        ".az.iiko.horecacontrol");
                if (!Directory.Exists(programData))
                    Directory.CreateDirectory(programData);
                return programData;
            }
        }
        public static void ExtractDllFromResources(string path, string fileName)
        {
            var fullPath = Path.Combine(path, fileName);
            if (File.Exists(fullPath))
            {
                return;
            }
            try
            {
                // Получаем текущую сборку
                var assembly = Assembly.GetExecutingAssembly();
                // Получаем ресурс (имя должно включать namespace проекта)
                var resourceName = fileName; // Проверь точное имя в свойствах ресурса

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    throw new Exception($"Не удалось найти встроенный ресурс {resourceName}");
                }

                using var fileStream = new FileStream(fullPath, FileMode.Create);
                stream.CopyTo(fileStream);
            }
            catch (Exception ex)
            {
                // Обработка ошибок (можно добавить логирование)
                throw new Exception("Ошибка при распаковке DLL: " + ex.Message);
            }
        }





        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);


        public static void SetDllDirectoryCPlusPlus(string path)
        {
            //var ptrSize = IntPtr.Size == 8 ? "x64" : "x32";
            //path = Path.Combine(path, ptrSize);
            bool ok = SetDllDirectory(path);
            if (!ok) throw new System.ComponentModel.Win32Exception();

            // PluginContext.Log.Info($"Set dll directory : {ptrSize}");
        }

    }
}
#endif