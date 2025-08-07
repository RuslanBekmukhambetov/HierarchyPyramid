using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GemBox.Spreadsheet;
using PyramidHierarchyImporter.src.Interfaces;

namespace PyramidHierarchyImporter.src
{
    /// <summary>
    /// Строитель настроек для скрипта импорта
    /// </summary>
    /// <remarks>
    /// Реализует паттерн Builder для пошаговой конфигурации.
    /// Обязательные параметры:
    /// - Путь к Excel-файлу
    /// - Коллекция приборов учета
    /// - Названия классификаторов
    /// 
    /// Необязательные параметры (значения по умолчанию):
    /// - Индекс строки заголовка (0)
    /// - Индекс начального столбца (0)
    /// - Индекс столбца результатов (22)
    /// </remarks>
    public class SettingsBuilder
    {
        private ILogger _logger;
        private ExcelWorksheet? _worksheet;
        private IEnumerable<ElectricityMeter> _meters;
        private string _technicalClsName;
        private string _geographicClsName;
        private DirectoryOfCommonUserItems _directoryOfCommonUserItems;
        private int _headerRowIndex;
        private int _startColumnIndex;
        private int _resultColumnIndex;

        public SettingsBuilder(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Устанавливает путь к Excel-файлу и рабочий лист
        /// </summary>
        /// <param name="path">Полный путь к файлу</param>
        public SettingsBuilder SetWorkbookAndWorksheet(string path)
        {
            _worksheet = ExcelFile.Load(path)?.Worksheets.FirstOrDefault();
            if (_worksheet == null )
            {
                _logger.Log($"Ошибка: не найден файл {path}");
            }
            return this;
        }

        /// <summary>
        /// Устанавливает коллекцию приборов учета
        /// </summary>
        /// <param name="technicalClsName">Коллекция приборов учета</param>
        public SettingsBuilder SetMeters(IEnumerable<ElectricityMeter> meters)
        {
            _meters = meters;
            return this;
        }

        /// <summary>
        /// Устанавливает названия классификаторов
        /// </summary>
        /// <param name="technicalClsName">Название технического классификатора</param>
        /// <param name="geographicClsName">Название географического классификатора</param>
        public SettingsBuilder SetClassifierNames(string technicalClsName, string geographicClsName)
        {
            _technicalClsName = technicalClsName;
            _geographicClsName = geographicClsName;
            return this;
        }

        /// <summary>
        /// Устанавливает справочник пользовательских объектов
        /// </summary>
        /// <param name="technicalClsName">Справочник пользовательских объектов</param>
        public SettingsBuilder SetDirectoryOfCommonUserItems(DirectoryOfCommonUserItems directoryOfCommonUserItems)
        {
            _directoryOfCommonUserItems = directoryOfCommonUserItems;
            return this;
        }

        /// <summary>
        /// Устанавливает значения индексов таблицы Excel
        /// </summary>
        /// <param name="headerRowIndex">Индекс строки заголовков</param>
        /// /// <param name="startColumnIndex">Индекс первого столбца таблицы</param>
        /// /// <param name="resultColumnIndex">Индекс последнего столбца таблицы (индекс столбца лога)</param>
        public SettingsBuilder SetColumnIndexes(int headerRowIndex, int startColumnIndex, int resultColumnIndex)
        {
            _headerRowIndex = headerRowIndex;
            _startColumnIndex = startColumnIndex;
            _resultColumnIndex = resultColumnIndex;
            return this;
        }

        /// <summary>
        /// Строит объект настроек
        /// </summary>
        /// <returns>Конфигурация Settings</returns>
        public Settings Build()
        {
            return new Settings(
                worksheet: _worksheet,
                meters: _meters,
                technicalClsName: _technicalClsName,
                geographicClsName: _geographicClsName,
                directoryOfCommonUserItems: _directoryOfCommonUserItems,
                headerRowIndex: _headerRowIndex,
                startColumnIndex: _startColumnIndex,
                resultColumnIndex: _resultColumnIndex
                );
        }
    }
}
