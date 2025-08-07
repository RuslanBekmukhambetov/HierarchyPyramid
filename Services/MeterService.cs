using PyramidHierarchyImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.Services
{
    /// <inheritdoc/>
    /// <remarks>
    /// Сервис для работы с приборами и точками учета.
    /// Особенности:
    /// 1. Группирует приборы по серийному номеру (без ведущих нулей)
    /// 2. Проверяет наличие единственного прибора для серийного номера
    /// 3. Возвращает точку учета, связанную с прибором
    /// 
    /// Важно: Серийные номера хранятся без ведущих нулей для сопоставления
    /// </remarks>
    public class MeterService : IMeterService
    {
        private readonly Dictionary<string, List<ElectricityMeter>> _meters;
        private ILogger _logger;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса
        /// </summary>
        /// <param name="meters">Коллекция приборов учета</param>
        /// <param name="logger">Логгер</param>
        public MeterService(IEnumerable<ElectricityMeter> meters, ILogger logger)
        {
            _meters = meters.Where(x => x.AttributeSerialNumber != null).GroupBy(x => x.AttributeSerialNumber.TrimStart('0')).ToDictionary(x => x.Key, x => x.ToList());
            _logger = logger;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Алгоритм поиска:
        /// 1. Удаляет ведущие нули из входного серийного номера
        /// 2. Ищет в словаре приборов
        /// 3. Проверяет наличие ровно одного прибора
        /// 4. Возвращает точку учета прибора
        /// </remarks>
        public MeterPoint GetMeterPointBySerial(string serial)
        {
            var meter = GetMeterBySerial(serial);
            if (meter != null && meter.AttributeMeterPointPlacement == null)
            {
                _logger.Log("Ошибка: не найдена Точка учета");
                return null;
            }
            return meter?.AttributeMeterPointPlacement;
        }

        private ElectricityMeter GetMeterBySerial(string serial)
        {
            if (string.IsNullOrEmpty(serial))
            {
                _logger.Log("Ошибка: пустое поле Серийный номер");
                return null;
            }

            if (!_meters.TryGetValue(serial.TrimStart('0'), out var meter))
            {
                _logger.Log("Ошибка: не найден прибор");
                return null;
            }
            if (meter.Count > 1)
            {
                _logger.Log("Ошибка: найдено несколько приборов");
                return null;
            }
            return meter[0];
        }
    }
}
