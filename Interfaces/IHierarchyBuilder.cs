using PyramidHierarchyImporter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.Interfaces
{
    /// <summary>
    /// Сервис для построения связанной иерархии энергообъекта
    /// </summary>
    public interface IHierarchyBuilder
    {
        /// <summary>
        /// Строит иерархию (топологию) сети (ПС, ТП, РУ, СШ, ячейки, фидеры/линии)
        /// </summary>
        /// <param name="address">DTO с техническими данными</param>
        /// <param name="cls">Классификатор точек учета</param>
        /// <param name="meterPoint">Точка учета</param>
        /// <param name="isNextValid">Флаг валидности следующего уровня иерархии</param>
        /// <param name="highVoltagePowerLine">Фидер ПС (опционально)</param>
        /// <returns>Фидер ПС или null при ошибке</returns>
        PowerLine BuildTechnicalHierarchy(TechnicalSideDto address, ClassifierOfMeterPointsByEnergyEntities cls, MeterPoint meterPoint,
            bool isNextValid, PowerLine highVoltagePowerLine);

        /// <summary>
        /// Строит географическую иерархию объектов (субъект, район, город, улица, дом)
        /// </summary>
        /// <param name="address">DTO с географическими данными</param>
        /// <param name="cls">Классификатор точек учета</param>
        /// <param name="meterPoint">Точка учета</param>
        /// <param name="powerLine">Фидер ПС</param>
        /// <returns>Жилой дом или null при ошибке</returns>
        DwellingHouse BuildGeographicHierarchy(GeographicalDto address, ClassifierOfMeterPointsByGeoLocation cls, MeterPoint meterPoint, PowerLine powerLine);
    }
}
