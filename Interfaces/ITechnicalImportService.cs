using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PyramidHierarchyImporter.Models;

namespace PyramidHierarchyImporter.Interfaces
{
    /// <summary>
    /// Сервис для импорта топологии (иерархии) сети
    /// </summary>
    public interface ITechnicalImportService
    {
        /// <summary>
        /// Создает или получает топологию (иерархию) сети
        /// </summary>
        /// <param name="address">DTO с техническими данными</param>
        /// <param name="meterPoint">Точка учета</param>
        /// <param name="isGeographicValid">Флаг валидности географических данных</param>
        /// <returns>Фидер/линия или null при ошибке</returns>
        public PowerLine CreateOrGetTechnicalHierarchy(TechnicalDto address, MeterPoint meterPoint, bool isGeographicValid);

    }
}
