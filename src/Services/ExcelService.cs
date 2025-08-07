using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PyramidHierarchyImporter.Models;
using PyramidHierarchyImporter.src.Interfaces;
using PyramidHierarchyImporter.src.Models;

namespace PyramidHierarchyImporter.src.Services
{
    /// <inheritdoc/>
    /// <remarks>
    /// Сервис для чтения данных из Excel с использованием GemBox.Spreadsheet.
    /// Особенности:
    /// 1. Автоматически определяет индексы столбцов по заголовкам
    /// 2. Использует атрибуты для маппинга названий столбцов
    /// 3. Логирует ошибки при отсутствии ожидаемых столбцов
    /// 
    /// Ограничения: Требует точного соответствия названий столбцов
    /// </remarks>
    public class ExcelService : IExcelService
    {
        private ExcelWorksheet _worksheet;
        private ILogger _logger;
        private Dictionary<string, int> _headerMap;
        private ColumnMap _columnMap;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса
        /// </summary>
        /// <param name="worksheet">Рабочий лист Excel</param>
        /// <param name="headerRowIndex">Индекс строки заголовка</param>
        /// <param name="startColumnIndex">Индекс начального столбца</param>
        /// <param name="resultColumnIndex">Индекс столбца результатов</param>
        /// <param name="logger">Логгер</param>
        public ExcelService(ExcelWorksheet worksheet, int headerRowIndex, int startColumnIndex, int resultColumnIndex, ILogger logger)
        {
            _worksheet = worksheet;
            _logger = logger;
            _headerMap = GetHeaders(headerRowIndex, startColumnIndex, resultColumnIndex);
            _columnMap = new ColumnMap(_headerMap, headerRowIndex, _logger);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Парсит строку Excel в TechnicalDto:
        /// 1. Разделяет данные на highSide и lowSide
        /// 2. Автоматически проверяет валидность данных
        /// 3. Логирует ошибки при пустых обязательных полях
        /// </remarks>
        public TechnicalDto CreateTechnicalDto(int row)
        {
            var rowCells = _worksheet.Rows[row];

            var highSide = new TechnicalSideDto(
                subsidiary: rowCells.Cells[_columnMap.HighSide.Subsidiary].Value?.ToString(),
                district: rowCells.Cells[_columnMap.HighSide.District].Value?.ToString(),
                substationVoltage: rowCells.Cells[_columnMap.HighSide.SubstationVoltage].Value?.ToString(),
                substation: rowCells.Cells[_columnMap.HighSide.Substation].Value?.ToString(),
                switchgear: rowCells.Cells[_columnMap.HighSide.Switchgear].Value?.ToString(),
                busBarsection: rowCells.Cells[_columnMap.HighSide.BusbarSection].Value?.ToString(),
                cubiclePowerLine: rowCells.Cells[_columnMap.HighSide.CubiclePowerLine].Value?.ToString(),
                powerLine: rowCells.Cells[_columnMap.HighSide.PowerLine].Value?.ToString()
                );

            var lowSide = new TechnicalSideDto(
                subsidiary: rowCells.Cells[_columnMap.LowSide.Subsidiary].Value?.ToString(),
                district: rowCells.Cells[_columnMap.LowSide.District].Value?.ToString(),
                substationVoltage: rowCells.Cells[_columnMap.LowSide.SubstationVoltage].Value?.ToString(),
                substation: rowCells.Cells[_columnMap.LowSide.Substation].Value?.ToString(),
                switchgear: rowCells.Cells[_columnMap.LowSide.Switchgear].Value?.ToString(),
                busBarsection: rowCells.Cells[_columnMap.LowSide.BusbarSection].Value?.ToString(),
                cubiclePowerLine: rowCells.Cells[_columnMap.LowSide.CubiclePowerLine].Value?.ToString(),
                powerLine: rowCells.Cells[_columnMap.LowSide.PowerLine].Value?.ToString()
                );
            if (!highSide.isValid)
            {
                _logger.Log($"Ошибка: пустые ячейки описания ПС");
            }
            if (!lowSide.isValid)
            {
                _logger.Log($"Ошибка: пустые ячейки описания ТП");
            }
            return new AddressDto.TechnicalDto(highSide, lowSide);
        }

        /// <inheritdoc/>
        public GeographicalDto CreateGeographicDto(int row)
        {
            var rowCells = _worksheet.Rows[row];
            var geographicAddress = new AddressDto.GeographicDto(
            subject: rowCells.Cells[_columnMap.Geographic.Subject].Value?.ToString(),
            district: rowCells.Cells[_columnMap.Geographic.District].Value?.ToString(),
            settlement: rowCells.Cells[_columnMap.Geographic.Settlement].Value?.ToString(),
            street: rowCells.Cells[_columnMap.Geographic.Street].Value?.ToString(),
            house: rowCells.Cells[_columnMap.Geographic.House].Value?.ToString()
            );
            if (!geographicAddress.isValid)
            {
                _logger.Log($"Ошибка: пустые ячейки описания Географии");
            }
            return geographicAddress;
        }

        private Dictionary<string, int> GetHeaders(int headerRowIndex, int startColumnIndex, int resultColumnIndex)
        {
            var headers = new Dictionary<string, int>();
            for (int column = startColumnIndex + 1; column < resultColumnIndex; column++)
            {
                headers.Add(_worksheet.Cells[headerRowIndex, column].Value?.ToString(), column);
            }
            return headers;
        }
    }
}
