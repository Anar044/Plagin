using System;
using System.Text;

namespace Resto.Front.Api.HorecaControlPlugin.Core.Infrastructure.Configuration
{
    internal static class BundledSocketAuthSecret
    {
        // Replaced only inside GitHub Actions at build time.
        // Never commit the real shared secret to the repository.
        private const string SecretBase64 = "__BUNDLED_SECRET_BASE64__";

        public static string Value
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SecretBase64) || SecretBase64.StartsWith("__", StringComparison.Ordinal))
                    return null;

                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(SecretBase64));
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
