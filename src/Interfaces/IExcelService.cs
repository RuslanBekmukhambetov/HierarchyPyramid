using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PyramidHierarchyImporter.src.Models;

namespace PyramidHierarchyImporter.src.Interfaces
{
    /// <summary>
    /// Сервис для работы с Excel-файлами
    /// </summary>
    public interface IExcelService
    {
        /// <summary>
        /// Создает DTO технических данных из строки Excel
        /// </summary>
        /// <param name="row">Номер строки в таблице Excel</param>
        /// <returns>DTO технических данных</returns>
        TechnicalDto CreateTechnicalDto(int row);

        /// <summary>
        /// Создает DTO географических данных из строки Excel
        /// </summary>
        /// <param name="row">Номер строки в таблице Excel</param>
        /// <returns>DTO географических данных</returns>
        GeographicalDto CreateGeographicDto(int row);
    }
}
