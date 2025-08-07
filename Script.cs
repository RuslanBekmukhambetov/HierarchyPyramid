using PyramidHierarchyImporter.Interfaces;
using PyramidHierarchyImporter.Services;
using GemBox.Spreadsheet;


namespace PyramidHierarchyImporter
{
    /// <summary>
    /// Главный класс для запуска пользовательских скриптов
    /// </summary>
    /// <remarks>
    /// Точка входа в приложение. Выполняет:
    /// 1. Инициализацию всех сервисов
    /// 2. Построение конфигурации
    /// 3. Обработку строк Excel
    /// 4. Обработку ошибок и запись результатов
    /// 
    /// Особенности среды выполнения:
    /// - Выполняется внутри ПО "Пирамида 2.0"
    /// - Использует ручное внедрение зависимостей
    /// - Не поддерживает асинхронные операции
    /// </remarks>
    public class Script
    {
        public void Execute()
        {
            ILogger logger = new ExcelLogger();
            SettingsBuilder settingsBuilder = new SettingsBuilder(logger);
            var settings = settingsBuilder.SetWorkbookAndWorksheet(@"C:\HierarchyImport.xlsx")
                .SetMeters(ElectricityMeter.GetInstances())
                .SetClassifierNames("NetTopologicalClassifier", "GeograpichalClassifier")
                .SetDirectoryOfCommonUserItems(DirectoryOfCommonUserItems.OnlyInstance)
                .SetColumnIndexes(0, 0, 22)
                .Build();
            IMeterService meterService = new MeterService(settings.Meters, logger);
            IExcelService excelService = new ExcelService(settings.Worksheet, settings.HeaderRowIndex, settings.StartColumnIndex, settings.ResultColumnIndex, logger);
            IHierarchyBuilder hierarchyBuilder = new HierarchyBuilder(settings.DirectoryOfCommonUserItems, logger);
            ITechnicalImportService technicalImporter = new TechnicalImportService(settings.TechnicalCls, hierarchyBuilder, logger);
            IGeographicalImportService geographicImporter = new GeographicalImportService(settings.GeographicCls, hierarchyBuilder, logger);

            var initializationErrors = logger.GetLogsAndClear();
            if (initializationErrors.Count > 0)
            {
                settings.Worksheet.Cells[settings.HeaderRowIndex, settings.ResultColumnIndex].Value = string.Join("; ", initializationErrors);
                return;
            }

            for (int row = settings.HeaderRowIndex + 1; row < settings.Worksheet.Rows.Count; row++)
            {
                try
                {
                    string serial = settings.Worksheet.Cells[row, settings.StartColumnIndex].Value.ToString();
                    var meterPoint = meterService.GetMeterPointBySerial(serial);
                    var technicalAddress = excelService.CreateTechnicalDto(row);
                    var geographicAddress = excelService.CreateGeographicDto(row);

                    // ошибки парсинга таблицы Excel
                    var parsingErrors = logger.GetLogsAndClear();
                    if (parsingErrors.Count > 0)
                    {
                        settings.Worksheet.Cells[row, settings.ResultColumnIndex].Value = string.Join("; ", parsingErrors);
                        continue;
                    }

                    var powerLine = technicalImporter.CreateOrGetTechnicalHierarchy(technicalAddress, meterPoint, geographicAddress.isValid);
                    geographicImporter.CreateOrGetGeographicHierarchy(geographicAddress, meterPoint, powerLine);

                    settings.Worksheet.Cells[row, settings.ResultColumnIndex].Value = GetImportResults();
                }
                catch (Exception ex)
                {
                    var stackTrace = new System.Diagnostics.StackTrace(ex, true);
                    var frame = stackTrace.GetFrame(0);
                    int lineNumber = frame?.GetFileLineNumber() ?? 0;
                    string methodName = frame?.GetMethod()?.Name ?? "Неизвестный метод";
                    settings.Worksheet.Cells[row, settings.ResultColumnIndex].Value = $"Ошибка при обработке строки таблицы: {methodName} - {lineNumber - 80} - {ex.Message}";
                }
            }

            string GetImportResults()
            {
                var results = logger.GetLogsAndClear();

                if (results.Count > 0)
                {
                    return string.Join("; ", results);
                }

                return "Импорт выполнен успешно";
            }
        }
    }
}

