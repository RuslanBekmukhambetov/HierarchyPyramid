using PyramidHierarchyImporter.src.Interfaces;
using PyramidHierarchyImporter.src.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.src.Services
{
    /// <inheritdoc/>
    /// <remarks>
    /// Сервис импорта технической иерархии.
    /// Особенности:
    /// 1. Последовательно строит высоковольтную и низковольтную стороны
    /// 2. Автоматически связывает ТП с ПС
    /// 3. Требует валидных данных для иерархии сети
    /// </remarks>
    public class TechnicalImportService : ITechnicalImportService
    {
        private readonly IHierarchyBuilder _hierarchyBuilder;
        private readonly ILogger _logger;
        private ClassifierOfMeterPointsByEnergyEntities _cls;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса
        /// </summary>
        /// <param name="cls">Классификатор энергообъектов</param>
        /// <param name="hierarchyBuilder">Строитель иерархии</param>
        /// <param name="logger">Логгер</param>
        public TechnicalImportService(ClassifierOfMeterPointsByEnergyEntities cls, IHierarchyBuilder hierarchyBuilder, ILogger logger)
        {
            _cls = cls;
            _hierarchyBuilder = hierarchyBuilder;
            _logger = logger;
        }

        /// <inheritdoc/>
        public PowerLine CreateOrGetTechnicalHierarchy(TechnicalDto address, MeterPoint meterPoint, bool isGeographicValid)
        {
            // Проверка излишняя, но оставим на всякий случай
            if (meterPoint == null || !address.HighSide.isValid || !address.LowSide.isValid)
            {
                _logger.Log("Ошибка: недопустимые входные данные для стуктуры иерархии сети");
                return null;
            }

            var highSidePowerLine = _hierarchyBuilder.BuildTechnicalHierarchy(address.HighSide, _cls, meterPoint, address.LowSide.isValid);
            if (highSidePowerLine == null)
            {
                _logger.Log("Ошибка: не удалось создать полную структуру ПС");
                return null;
            }

            var lowSidePowerLine = _hierarchyBuilder.BuildTechnicalHierarchy(address.LowSide, _cls, meterPoint, isGeographicValid, highSidePowerLine);

            if (lowSidePowerLine == null)
            {
                _logger.Log("Ошибка: не удалось создать полную структуру ТП");
                return null;
            }

            return lowSidePowerLine;
        }
    }
}
