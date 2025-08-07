using PyramidHierarchyImporter.src.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.src
{
    /// <summary>
    /// Карта столбцов Excel для автоматического маппинга данных
    /// </summary>
    /// <remarks>
    /// Автоматически определяет индексы столбцов по их названиям
    /// используя атрибуты [ColumnName]
    /// </remarks>
    public class ColumnMap
    {
        /// <summary>Индексы столбцов для высоковольтной стороны</summary>
        public HighSideIndexes HighSide { get; }

        /// <summary>Индексы столбцов для низковольтной стороны</summary>
        public LowSideIndexes LowSide { get; }

        /// <summary>Индексы столбцов для географических данных</summary>
        public GeographicIndexes Geographic { get; }
        private ILogger _logger;

        /// <summary>
        /// Инициализирует карту столбцов на основе заголовков
        /// </summary>
        /// <param name="headersMap">Словарь [название → индекс]</param>
        /// <param name="headerRowIndex">Индекс строки заголовка</param>
        /// <param name="logger">Логгер для записи ошибок</param>
        public ColumnMap(Dictionary<string, int> headersMap, int headerRowIndex, ILogger logger)
        {
            _logger = logger;
            HighSide = BuildMap<HighSideIndexes>(headersMap, headerRowIndex);
            LowSide = BuildMap<LowSideIndexes>(headersMap, headerRowIndex);
            Geographic = BuildMap<GeographicIndexes>(headersMap, headerRowIndex);
        }

        /// <summary>
        /// Атрибут для указания названия столбца в Excel
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class ColumnNameAttribute : Attribute
        {
            public string ColumnName { get; }
            public ColumnNameAttribute(string columnName) => ColumnName = columnName;
        }

        /// <summary>
        /// Индексы столбцов для высоковольтной стороны (ПС)
        /// </summary>
        public class HighSideIndexes
        {
            [ColumnName("Филиал ПС")] public int Subsidiary { get; private set; }
            [ColumnName("РЭС ПС")] public int District { get; private set; }
            [ColumnName("Группа ПС")] public int SubstationVoltage { get; private set; }
            [ColumnName("ПС")] public int Substation { get; private set; }
            [ColumnName("РУ ПС")] public int Switchgear { get; private set; }
            [ColumnName("СШ ПС")] public int BusbarSection { get; private set; }
            [ColumnName("яч ПС")] public int CubiclePowerLine { get; private set; }
            [ColumnName("ф ПС")] public int PowerLine { get; private set; }
        }

        /// <summary>
        /// Индексы столбцов для низковольтной стороны (ТП)
        /// </summary>
        public class LowSideIndexes
        {
            [ColumnName("Филиал ТП")] public int Subsidiary { get; private set; }
            [ColumnName("РЭС ТП")] public int District { get; private set; }
            [ColumnName("Группа ТП")] public int SubstationVoltage { get; private set; }
            [ColumnName("ТП")] public int Substation { get; private set; }
            [ColumnName("РУ ТП")] public int Switchgear { get; private set; }
            [ColumnName("СШ ТП")] public int BusbarSection { get; private set; }
            [ColumnName("яч ТП")] public int CubiclePowerLine { get; private set; }
            [ColumnName("ф ТП")] public int PowerLine { get; private set; }
        }

        /// <summary>
        /// Индексы столбцов для географических данных
        /// </summary>
        public class GeographicIndexes
        {
            [ColumnName("Субъект")] public int Subject { get; private set; }
            [ColumnName("Район")] public int District { get; private set; }
            [ColumnName("Населенный пункт")] public int Settlement { get; private set; }
            [ColumnName("Улица")] public int Street { get; private set; }
            [ColumnName("Дом")] public int House { get; private set; }
        }

        /// <summary>
        /// Строит карту столбцов для указанного типа
        /// </summary>
        /// <typeparam name="T">Тип карты (HighSideIndexes/LowSideIndexes/GeographicIndexes)</typeparam>
        /// <param name="headersMap">Словарь заголовков</param>
        /// <param name="headerRowIndex">Индекс строки заголовка</param>
        /// <returns>Заполненный объект с индексами</returns>
        private T BuildMap<T>(Dictionary<string, int> headersMap, int headerRowIndex) where T : new()
        {
            var instance = new T();
            var type = typeof(T);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = prop.GetCustomAttribute<ColumnNameAttribute>();
                if (attr == null)
                {
                    continue;
                }
                if (!headersMap.TryGetValue(attr.ColumnName, out int columnIndex))
                {
                    _logger.Log($"Ошибка: не найден заголовок {attr.ColumnName}");
                }
                prop.SetValue(instance, columnIndex);
            }
            return instance;
        }
    }
}
