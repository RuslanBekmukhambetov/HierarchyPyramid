using PyramidHierarchyImporter.Interfaces;
using PyramidHierarchyImporter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.Services
{
    /// <inheritdoc/>
    /// <remarks>
    /// Сервис импорта географической иерархии.
    /// Особенности:
    /// 1. Проверяет валидность входных данных перед построением
    /// 2. Логирует ошибки при невалидных данных или неудачном построении
    /// 3. Не возвращает значения, так как результат работы сохраняется в системе
    /// </remarks>
    public class GeographicalImportService : IGeographicalImportService
    {
        private readonly IHierarchyBuilder _hierarchyBuilder;
        private readonly ILogger _logger;
        private ClassifierOfMeterPointsByGeoLocation _cls;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса
        /// </summary>
        /// <param name="cls">Классификатор географических объектов</param>
        /// <param name="hierarchyBuilder">Строитель иерархии</param>
        /// <param name="logger">Логгер</param>
        public GeographicalImportService(ClassifierOfMeterPointsByGeoLocation cls, IHierarchyBuilder hierarchyBuilder, ILogger logger)
        {
            _cls = cls;
            _hierarchyBuilder = hierarchyBuilder;
            _logger = logger;
        }

        public void CreateOrGetGeographicHierarchy(GeographicalDto address, MeterPoint meterPoint, PowerLine powerLine)
        {
            if (!address.isValid)
            {
                _logger.Log("Ошибка: недопустимые входные данные для Географии");
                return;
            }
            DwellingHouse house = _hierarchyBuilder.BuildGeographicHierarchy(address, _cls, meterPoint, powerLine);
            if (house == null)
            {
                _logger.Log("Ошибка: не удалось создать полную структуру Географии");
                return;
            }
        }
    }
}
