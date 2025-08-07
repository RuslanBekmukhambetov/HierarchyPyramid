using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.Interfaces
{
    /// <summary>
    /// Сервис для работы с приборами и точками учета
    /// </summary>
    public interface IMeterService
    {
        /// <summary>
        /// Возвращает точку учета по серийному номеру прибора
        /// </summary>
        /// <param name="serial">Серийный номер прибора</param>
        /// <returns>Точка учета или null, если не найдена</returns>
        MeterPoint GetMeterPointBySerial(string serial);
    }
}
