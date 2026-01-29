using log4net;
using DotNet.Xdt;

namespace MonitorUpdaterSample.Core
{
    public class FileUpdater
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FileUpdater));

        private const string DeleteCommandExtension = ".del";
        private const string AddCommandExtension = ".add";
        private const string UpdateCommandExtension = ".upd";
        private const string XdtMergeCommandExtension = ".xmrg";
        private const string ExecuteCommandExtension = ".exc";
        private const string ExecuteCommandExtensionInitial = ".eini";
        private const string ExecuteCommandExtensionEnd = ".eend";
        private const string ExecuteCommandParamsExtension = ".params";
        private const string CannotTransformXdtMessage = "No se puede realizar la transformacion del archivo: {0}";

        private static void BackupFile(string backupDir, string targetFolder, string originalFileName, string command)
        {
            var backupFileName = originalFileName;
            var targetRelativeDirectory = string.Empty;

            if (File.Exists(originalFileName))
            {
                if (command == DeleteCommandExtension)
                {
                    command = UpdateCommandExtension;
                    targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(originalFileName, targetFolder));
                }

                if (command == ExecuteCommandExtensionInitial || command == ExecuteCommandExtension || command == ExecuteCommandExtensionEnd || command == ExecuteCommandParamsExtension)
                {
                    backupFileName = Path.Combine(Path.GetDirectoryName(originalFileName), Path.GetFileNameWithoutExtension(originalFileName));
                    targetRelativeDirectory = targetFolder;
                }
            }
            else if (command == DeleteCommandExtension)
            {
                if (!Directory.Exists(Path.GetDirectoryName(originalFileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(originalFileName));
                }

                File.WriteAllText(originalFileName, string.Empty);
                targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(originalFileName, targetFolder));
            }
            else if (command == UpdateCommandExtension)
            {
                command = DeleteCommandExtension;
                if (!Directory.Exists(Path.GetDirectoryName(originalFileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(originalFileName));
                }

                File.WriteAllText(originalFileName, string.Empty);
                targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(originalFileName, targetFolder));
            }
            else
            {
                return;
            }

            var folderToBackupFile = Path.Combine(backupDir, targetRelativeDirectory ?? string.Empty);

            if (!Directory.Exists(folderToBackupFile))
            {
                Directory.CreateDirectory(folderToBackupFile);
            }

            File.Copy(originalFileName, Path.Combine(folderToBackupFile, Path.GetFileName(backupFileName)) + command, true);
        }

        private static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new(filespec);

            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            Uri folderUri = new(folder);

            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public string UpdateFiles(string sourceFolder, string targetFolder, string backupDir = null)
        {
            var createBackup = !string.IsNullOrEmpty(backupDir);

            if (createBackup)
            {
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }
            }

            try
            {
                Directory.EnumerateFiles(sourceFolder, "*" + ExecuteCommandExtensionInitial, SearchOption.AllDirectories).ToList().ForEach(file =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(file, sourceFolder));
                    ExecuteBats(file, fileName, targetRelativeDirectory, ExecuteCommandExtensionInitial);
                });

                foreach (var file in Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories).Where(s => Path.GetExtension(s) != ExecuteCommandExtensionInitial && Path.GetExtension(s) != ExecuteCommandExtensionEnd).ToList())
                {
                    var fileExtension = Path.GetExtension(file);
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(file, sourceFolder));
                    var targetFileName = Path.Combine(targetFolder, targetRelativeDirectory ?? string.Empty, fileName ?? string.Empty);

                    switch (fileExtension.ToLower())
                    {
                        case AddCommandExtension:
                            if (createBackup)
                            {
                                BackupFile(Path.Combine(backupDir, targetRelativeDirectory ?? string.Empty), targetFolder, targetFileName, DeleteCommandExtension);
                            }
                            CopyFile(file, targetFileName);
                            break;
                        case XdtMergeCommandExtension:
                            if (createBackup && File.Exists(targetFileName))
                            {
                                BackupFile(Path.Combine(backupDir, targetRelativeDirectory ?? string.Empty), targetFolder, targetFileName, UpdateCommandExtension);
                            }
                            MergeXDT(file, targetFileName);
                            break;
                        case UpdateCommandExtension:
                            if (createBackup)
                            {
                                BackupFile(Path.Combine(backupDir, targetRelativeDirectory ?? string.Empty), targetFolder, targetFileName, UpdateCommandExtension);
                            }
                            CopyFile(file, targetFileName);
                            break;
                        case DeleteCommandExtension:
                            if (createBackup)
                            {
                                BackupFile(Path.Combine(backupDir, targetRelativeDirectory ?? string.Empty), targetFolder, targetFileName, AddCommandExtension);
                            }
                            RemoveFile(targetFileName);
                            break;
                        case ExecuteCommandExtension:
                            ExecuteBats(file, fileName, targetRelativeDirectory, ExecuteCommandExtension);
                            break;
                    }
                }

                Directory.EnumerateFiles(sourceFolder, "*" + ExecuteCommandExtensionEnd, SearchOption.AllDirectories).ToList().ForEach(file =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var targetRelativeDirectory = Path.GetDirectoryName(GetRelativePath(file, sourceFolder));
                    ExecuteBats(file, fileName, targetRelativeDirectory, ExecuteCommandExtensionEnd);
                });
            }
            catch (Exception ex)
            {
                if (createBackup)
                {
                    var errorMessage = ex.ToString();
                    var rollbackError = UpdateFiles(backupDir, targetFolder);
                    return errorMessage + (string.IsNullOrEmpty(rollbackError) ? string.Empty : Environment.NewLine + "Rollback Error =>" + Environment.NewLine + rollbackError);
                }

                return ex.ToString();
            }

            return string.Empty;
        }

        private static void ExecuteBats(string file, string fileName, string targetRelativeDirectory, string extension)
        {
            Log.Info($"Ejecutando script: {fileName}{extension}");
            Log.Info($"  Ruta: {file}");

            if (File.Exists(file))
            {
                string content = File.ReadAllText(file);
                Log.Info($"  Contenido del script:\n{content}");
            }

            Log.Info($"  Script ejecutado correctamente (simulado)");
        }

        private static void CopyFile(string sourceFile, string targetFile)
        {
            try
            {
                Log.Info($"Copiando archivo:");
                Log.Info($"  Origen: {sourceFile}");
                Log.Info($"  Destino: {targetFile}");

                string targetDir = Path.GetDirectoryName(targetFile);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                File.Copy(sourceFile, targetFile, true);
                Log.Info($"  Archivo copiado exitosamente");
            }
            catch (Exception ex)
            {
                Log.Error($"Error al copiar archivo de {sourceFile} a {targetFile}", ex);
                throw;
            }
        }

        private static void RemoveFile(string targetFile)
        {
            try
            {
                Log.Info($"Eliminando archivo: {targetFile}");

                if (File.Exists(targetFile))
                {
                    File.Delete(targetFile);
                    Log.Info($"  Archivo eliminado exitosamente");
                }
                else
                {
                    Log.Warn($"  Archivo no existe, no se puede eliminar");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error al eliminar archivo {targetFile}", ex);
                throw;
            }
        }

        private static void MergeXDT(string sourceFile, string targetFile)
        {
            if (File.Exists(targetFile))
            {
                Log.Info($"Aplicando transformacion XDT:");
                Log.Info($"  Archivo fuente: {sourceFile}");
                Log.Info($"  Archivo destino: {targetFile}");

                using var target = new XmlTransformableDocument();
                target.PreserveWhitespace = true;
                target.Load(targetFile);

                using var xdt = new XmlTransformation(sourceFile);
                if (xdt.Apply(target))
                {
                    target.Save(targetFile);
                    Log.Info($"  Transformacion XDT aplicada exitosamente");
                }
                else
                {
                    throw new Exception(string.Format(CannotTransformXdtMessage, sourceFile));
                }
            }
            else
            {
                Log.Warn($"  Archivo destino no existe para transformacion XDT: {targetFile}");
            }
        }
    }
}
