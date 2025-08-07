using PyramidHierarchyImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter
{
    /// <inheritdoc/>
    /// <remarks>
    /// Реализация логгера, хранящая сообщения в памяти.
    /// Особенности:
    /// 1. Сообщения накапливаются в списке
    /// 2. GetLogsAndClear очищает буфер после чтения
    /// 3. Используется для вывода ошибок в Excel
    /// </remarks>
    public class ExcelLogger : ILogger
    {
        private List<string> _errors;

        /// <summary>
        /// Инициализирует новый экземпляр логгера
        /// </summary>
        public ExcelLogger()
        {
            _errors = new List<string>();
        }

        /// <inheritdoc/>
        public void Log(string message)
        {
            _errors.Add(message);
        }

        /// <inheritdoc/>
        public List<string> GetProcessedLogs()
        {
            return _errors.ToList();
        }

        /// <inheritdoc/>
        public List<string> GetLogsAndClear()
        {
            var errors = _errors.ToList();
            _errors.Clear();
            return errors;
        }
    }
}
