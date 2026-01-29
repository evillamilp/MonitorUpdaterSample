using log4net;

namespace MonitorUpdaterSample.Services
{
    public class WindowsServiceManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WindowsServiceManager));

        public WindowsServiceManager()
        {
            Log.Info("WindowsServiceManager inicializado (dummy)");
        }

        public void StartService(string serviceName)
        {
            Log.Info($"Iniciando servicio: {serviceName} (simulado)");
        }

        public void StopService(string serviceName)
        {
            Log.Info($"Deteniendo servicio: {serviceName} (simulado)");
        }

        public bool IsServiceRunning(string serviceName)
        {
            Log.Info($"Verificando estado del servicio: {serviceName} (simulado)");
            return false;
        }
    }
}
