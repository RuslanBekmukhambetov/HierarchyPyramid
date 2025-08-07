using PyramidHierarchyImporter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.Interfaces
{
    /// <summary>
    /// Сервис для импорта географической иерархии
    /// </summary>
    public interface IGeographicalImportService
    {
        /// <summary>
        /// Создает или получает географическую иерархию объектов
        /// </summary>
        /// <param name="address">DTO с географическими данными</param>
        /// <param name="meterPoint">Точка учета</param>
        /// <param name="powerLine">Линия низковольтной ТП, от которой запитан дом</param>
        void CreateOrGetGeographicHierarchy(GeographicalDto address, MeterPoint meterPoint, PowerLine powerLine);
    }
}
