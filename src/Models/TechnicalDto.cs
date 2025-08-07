using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.src.Models
{
    /// <summary>
    /// Data Transfer Object (DTO) для передачи информации о Топологии (Иерархии) сети
    /// </summary>
    public class TechnicalDto
    {
        /// <summary>
        /// Высоковольтная сторона (подстанции (ПС) с напряжением 35 кВ и выше)
        /// </summary>
        public TechnicalSideDto? HighSide { get; }

        /// <summary>
        /// Низковольтная сторона (трансформаторная подстанция (ТП) 10/0,4 кВ)
        /// </summary>
        public TechnicalSideDto? LowSide { get; }

        /// <summary>
        /// Инициализирует новый экземпляр TechnicalDto
        /// </summary>
        /// <param name="highSide">Данные высоковольтной стороны</param>
        /// <param name="lowSide">Данные низковольтной стороны</param>
        public TechnicalDto(TechnicalSideDto? highSide, TechnicalSideDto? lowSide)
        {
            HighSide = highSide;
            LowSide = lowSide;
        }
    }
}
