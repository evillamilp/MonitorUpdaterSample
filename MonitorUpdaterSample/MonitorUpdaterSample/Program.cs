using log4net;
using log4net.Config;
using MonitorUpdaterSample.Core;
using System.Reflection;

namespace MonitorUpdaterSample
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            Log.Info("=== INICIO DEL PROGRAMA ===");
            Log.Info("Sistema de Actualizacion del Monitor - Demo");
            Log.Info("");

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string testDataDir = Path.Combine(baseDir, "TestData");
                string sourceDir = Path.Combine(testDataDir, "SourceFiles");
                string targetDir = Path.Combine(testDataDir, "TargetFiles");

                Log.Info($"Directorio base: {baseDir}");
                Log.Info($"Directorio fuente: {sourceDir}");
                Log.Info($"Directorio destino: {targetDir}");
                Log.Info("");

                PrepareTestFiles(sourceDir, targetDir);

                Log.Info("Iniciando proceso de actualizacion del monitor...");
                MonitorUpdaterManagerSample.UpdateMonitor(
                    monitorFilesLocation: sourceDir,
                    installationFolder: targetDir,
                    version: "1.0.0.0"
                );

                Log.Info("");
                Log.Info("=== PROCESO COMPLETADO EXITOSAMENTE ===");
            }
            catch (Exception ex)
            {
                Log.Error("Error fatal en la aplicacion", ex);
                Console.WriteLine($"\nERROR: {ex.Message}");
            }

            Console.WriteLine("\nPresiona cualquier tecla para salir...");
        }

        private static void PrepareTestFiles(string sourceDir, string targetDir)
        {
            Log.Info("Preparando archivos de prueba...");

            Directory.CreateDirectory(sourceDir);
            Directory.CreateDirectory(targetDir);

            string addFile = Path.Combine(sourceDir, "nuevo_archivo.txt.add");
            File.WriteAllText(addFile, "Este es un archivo nuevo que sera agregado.");
            Log.Info($"Creado: {addFile}");

            string existingFile = Path.Combine(targetDir, "archivo_existente.txt");
            File.WriteAllText(existingFile, "Contenido original del archivo.");
            Log.Info($"Creado: {existingFile}");

            string updFile = Path.Combine(sourceDir, "archivo_existente.txt.upd");
            File.WriteAllText(updFile, "Contenido ACTUALIZADO del archivo.");
            Log.Info($"Creado: {updFile}");

            string fileToDelete = Path.Combine(targetDir, "archivo_a_eliminar.txt");
            File.WriteAllText(fileToDelete, "Este archivo sera eliminado.");
            Log.Info($"Creado: {fileToDelete}");

            string delFile = Path.Combine(sourceDir, "archivo_a_eliminar.txt.del");
            File.WriteAllText(delFile, "");
            Log.Info($"Creado: {delFile}");

            string einiFile = Path.Combine(sourceDir, "init_script.bat.eini");
            File.WriteAllText(einiFile, "@echo off\necho Script de inicializacion ejecutado\n");
            Log.Info($"Creado: {einiFile}");

            string eendFile = Path.Combine(sourceDir, "end_script.bat.eend");
            File.WriteAllText(eendFile, "@echo off\necho Script de finalizacion ejecutado\n");
            Log.Info($"Creado: {eendFile}");

            Log.Info("Archivos de prueba preparados correctamente.");
            Log.Info("");
        }
    }
}
