using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.src.Interfaces
{
    /// <summary>
    /// Интерфейс системы логирования
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Записывает сообщение в лог
        /// </summary>
        /// <param name="message">Сообщение для записи</param>
        void Log(string message);

        /// <summary>
        /// Возвращает все обработанные логи без очистки буфера
        /// </summary>
        List<string> GetProcessedLogs();

        /// <summary>
        /// Возвращает все сообщения лога и очищает внутренний буфер
        /// </summary>
        List<string> GetLogsAndClear();
    }
}
