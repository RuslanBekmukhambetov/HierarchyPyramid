using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.src.Models
{
    /// <summary>
    /// DTO для описания одной из сторон Топологии сети (высокой (35 кВ+) и низкой(0,4 кВ))
    /// </summary>
    public class TechnicalSideDto
    {
        /// <summary>Филиал электросетей</summary>
        public string? Subsidiary { get; }

        /// <summary>Район электрических сетей (РЭС)</summary>
        public string? District { get; }

        /// <summary>Напряжение подстанции (например, "110 кВ")</summary>
        public string? SubstationVoltage { get; }

        /// <summary>Название подстанции</summary>
        public string? Substation { get; }

        /// <summary>Распределительное устройство (РУ)</summary>
        public string? Switchgear { get; }

        /// <summary>Секция шин (СШ)</summary>
        public string? BusbarSection { get; }

        /// <summary>Ячейка секции шин</summary>
        public string? CubiclePowerLine { get; }

        /// <summary>Линия (фидер)</summary>
        public string? PowerLine { get; }

        /// <summary>Уровень напряжения высокой стороны ТП для создания ссылки на фидер ПС (по умолчанию "РУ-10 кВ")</summary>
        public string? SubstationLinkVoltage { get; }

        /// <summary>Флаг валидности данных</summary>
        public bool isValid { get; }

        /// <summary>
        /// Инициализирует новый экземпляр TechnicalSideDto
        /// </summary>
        /// <param name="subsidiary">Филиал</param>
        /// <param name="district">РЭС</param>
        /// <param name="substationVoltage">Напряжение ПС</param>
        /// <param name="substation">Подстанция</param>
        /// <param name="switchgear">РУ</param>
        /// <param name="busBarsection">Секция шин</param>
        /// <param name="cubiclePowerLine">Ячейка</param>
        /// <param name="powerLine">Фидер</param>
        public TechnicalSideDto(string? subsidiary, string? district, string? substationVoltage, string? substation, string? switchgear, string? busBarsection, string? cubiclePowerLine, string? powerLine)
        {
            Subsidiary = subsidiary;
            District = district;
            SubstationVoltage = substationVoltage;
            Substation = substation;
            Switchgear = switchgear;
            BusbarSection = busBarsection;
            CubiclePowerLine = cubiclePowerLine;
            PowerLine = powerLine;
            SubstationLinkVoltage = "РУ-10 кВ";
            isValid = !string.IsNullOrWhiteSpace(Subsidiary)
                && !string.IsNullOrWhiteSpace(District)
                && !string.IsNullOrWhiteSpace(SubstationVoltage)
                && !string.IsNullOrWhiteSpace(Substation)
                && !string.IsNullOrWhiteSpace(Switchgear)
                && !string.IsNullOrWhiteSpace(BusbarSection)
                && !string.IsNullOrWhiteSpace(CubiclePowerLine)
                && !string.IsNullOrWhiteSpace(PowerLine);
        }
    }
}
