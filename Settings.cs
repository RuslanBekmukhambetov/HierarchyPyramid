using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter
{
    /// <summary>
    /// Контейнер настроек для скрипта импорта иерархий
    /// </summary>
    /// <remarks>
    /// Содержит все необходимые зависимости и параметры:
    /// - Рабочий лист Excel
    /// - Коллекция приборов учета
    /// - Классификаторы
    /// - Параметры столбцов
    /// </remarks>
    public class Settings
    {
        /// <summary>Рабочий лист Excel с данными</summary>
        public ExcelWorksheet Worksheet { get; }

        /// <summary>Коллекция приборов учета</summary>
        public IEnumerable<ElectricityMeter> Meters { get; }

        /// <summary>Классификатор географических объектов</summary>
        public ClassifierOfMeterPointsByGeoLocation GeographicCls { get; }

        /// <summary>Классификатор энергетических объектов</summary>
        public ClassifierOfMeterPointsByEnergyEntities TechnicalCls { get; }

        /// <summary>Справочник пользовательских объектов</summary>
        public DirectoryOfCommonUserItems DirectoryOfCommonUserItems { get; }

        /// <summary>Индекс строки заголовка в Excel</summary>
        public int HeaderRowIndex { get; }

        /// <summary>Индекс первого столбца в Excel</summary>
        public int StartColumnIndex { get; }

        /// <summary>Индекс последнего столбца (столбца результатов) в Excel</summary>
        public int ResultColumnIndex { get; }

        // <summary>
        /// Инициализирует новый экземпляр настроек
        /// </summary>
        /// <param name="worksheet">Рабочий лист Excel</param>
        /// <param name="meters">Приборы учета</param>
        /// <param name="technicalClsName">Название технического классификатора</param>
        /// <param name="geographicClsName">Название географического классификатора</param>
        /// <param name="directoryOfCommonUserItems">Реестр объектов</param>
        /// <param name="headerRowIndex">Строка заголовка</param>
        /// <param name="startColumnIndex">Столбец начала данных</param>
        /// <param name="resultColumnIndex">Столбец результатов</param>
        public Settings(ExcelWorksheet worksheet, IEnumerable<ElectricityMeter> meters, string technicalClsName, string geographicClsName, DirectoryOfCommonUserItems directoryOfCommonUserItems,
                int headerRowIndex, int startColumnIndex, int resultColumnIndex)
        {
            this.Worksheet = worksheet;
            this.Meters = meters;
            this.GeographicCls = ClassifierOfMeterPointsByGeoLocation.GetInstances().FirstOrDefault(cls => cls.AttributeCaption == geographicClsName);
            this.TechnicalCls = ClassifierOfMeterPointsByEnergyEntities.GetInstances().FirstOrDefault(cls => cls.AttributeCaption == technicalClsName);
            this.DirectoryOfCommonUserItems = directoryOfCommonUserItems;
            this.HeaderRowIndex = headerRowIndex;
            this.StartColumnIndex = startColumnIndex;
            this.ResultColumnIndex = resultColumnIndex;
        }
    }
}
