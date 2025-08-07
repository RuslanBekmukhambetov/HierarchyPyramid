using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.src.Models
{
    /// <summary>
    /// DTO для передачи географических данных
    /// </summary>
    public class GeographicalDto
    {
        /// <summary>Субъект РФ</summary>
        public string? Subject { get; }

        /// <summary>Административный район</summary>
        public string? District { get; }

        /// <summary>Населенный пункт</summary>
        public string? Settlement { get; }

        /// <summary>Улица</summary>
        public string? Street { get; }

        /// <summary>Номер дома</summary>
        public string? House { get; }

        /// <summary>Флаг валидности данных</summary>
        public bool isValid { get; }

        /// <summary>
        /// Инициализирует новый экземпляр GeographicalDto
        /// </summary>
        /// <param name="subject">Субъект РФ</param>
        /// <param name="district">Район</param>
        /// <param name="settlement">Населенный пункт</param>
        /// <param name="street">Улица</param>
        /// <param name="house">Дом</param>
        public GeographicalDto(string? subject, string? district, string? settlement, string? street, string? house)
        {
            Subject = subject;
            District = district;
            Settlement = settlement;
            Street = street;
            House = house;
            isValid = !string.IsNullOrWhiteSpace(Subject)
                    && !string.IsNullOrWhiteSpace(District)
                    && !string.IsNullOrWhiteSpace(Settlement)
                    && !string.IsNullOrWhiteSpace(Street)
                    && !string.IsNullOrWhiteSpace(House);
        }
    }
}
