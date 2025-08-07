using PyramidHierarchyImporter.Interfaces;
using PyramidHierarchyImporter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyramidHierarchyImporter.Services
{
    /// /// <inheritdoc/>
    /// <remarks>
    /// Реализация строителя иерархий с использованием предварительно сконфигурированных словарей делегатов.
    /// Основные особенности:
    /// 1. Использует три словаря делегатов для динамического построения иерархий
    /// 2. Автоматически определяет типы объектов по контексту
    /// 3. Обеспечивает гибкое добавление новых уровней иерархии
    /// </remarks>
    public class HierarchyBuilder : IHierarchyBuilder
    {
        private readonly ILogger _logger;
        private DirectoryOfCommonUserItems _directoryOfCommonUserItems;
        private Dictionary<string, Func<object, string, ClassifierItem>> _technicalHierarchy;
        private Dictionary<string, Func<object, string, ClassifierItem>> _geographicHierarchy;
        private Dictionary<Type, Action<object, object>> _actionDictionary;

        /// <summary>
        /// Инициализирует новый экземпляр построителя иерархий
        /// </summary>
        /// <remarks>
        /// 1. _technicalHierarchy: Словарь делегатов для построения технической иерархии объектов ПС -> РУ -> СШ -> Ячейка -> Фидер
        /// 2. _geographicHierarchy: Словарь делегатов для построения географической иерархии объектов Субъект РФ -> Район -> Населенный пункт -> Улица -> Дом
        /// 3. _actionDictionary: Словарь делегатов для привязки созданного объекта к родительскому (например, добавить экземпляр типа "Улица" в экземпляр типа "Населенный пункт")
        /// </remarks>
        public HierarchyBuilder(DirectoryOfCommonUserItems directoryOfCommonUserItems, ILogger logger)
        {
            _logger = logger;
            _directoryOfCommonUserItems = directoryOfCommonUserItems;
            _technicalHierarchy = new Dictionary<string, Func<object, string, ClassifierItem>>()
            {
                { "Subsidiary", (parent, caption) => CreateOrGetFolder<ElectricalNetworksSubsidiary, ClassifierOfMeterPointsByEnergyEntities>((ClassifierOfMeterPointsByEnergyEntities)parent, caption) },
                { "District", (parent, caption) => CreateOrGetFolder<ElectricalNetworksDistrict, ElectricalNetworksSubsidiary>((ElectricalNetworksSubsidiary)parent, caption) },
                { "SubstationVoltage", (parent, caption) => CreateOrGetFolder<SubstationsGroupByVoltage, ElectricalNetworksDistrict>((ElectricalNetworksDistrict)parent, caption) },
                { "Substation", (parent, caption) =>
                    {
                        if ( ((SubstationsGroupByVoltage)parent).Caption.Contains("0,4 кВ") )
                        {
                            return CreateOrGetFolder<LowVoltageSubstation, SubstationsGroupByVoltage>((SubstationsGroupByVoltage)parent, caption);
                        }
                        else
                        {
                            return CreateOrGetFolder<HighVoltageSubstation, SubstationsGroupByVoltage>((SubstationsGroupByVoltage)parent, caption);
                        }
                    }
                },
                { "Switchgear", (parent, caption) =>
                    {
                        if (parent is HighVoltageSubstation substation)
                        {
                            return CreateOrGetFolder<Switchgear, HighVoltageSubstation>((HighVoltageSubstation)parent, caption);
                        }
                        else
                        {
                            return CreateOrGetFolder<Switchgear, LowVoltageSubstation>((LowVoltageSubstation)parent, caption);
                        }
                    }
                },
                { "BusbarSection", (parent, caption) => CreateOrGetFolder<BusbarSection, Switchgear>((Switchgear)parent, caption) },
                { "CubiclePowerLine", (parent, caption) => CreateOrGetFolder<CubiclePowerLine, BusbarSection>((BusbarSection)parent, caption) },
                { "PowerLine", (parent, caption) => CreateOrGetFolder<PowerLine, CubiclePowerLine>((CubiclePowerLine)parent, caption) }
            };
            _geographicHierarchy = new Dictionary<string, Func<object, string, ClassifierItem>>()
            {
                { "Subject", (parent, caption) => CreateOrGetFolder<SubjectRF, ClassifierOfMeterPointsByGeoLocation>((ClassifierOfMeterPointsByGeoLocation)parent, caption) },
                { "District", (parent, caption) => CreateOrGetFolder<District, SubjectRF>((SubjectRF)parent, caption) },
                { "Settlement", (parent, caption) =>
                    {
                        if (caption.StartsWith("г ")) // добавить город
						{
                            return CreateOrGetFolder<CenterOfPopulation, District>((District)parent, caption);
                        }
                            else // добавить остальное
						{
                            return CreateOrGetFolder<PlanningStructure, District>((District)parent, caption);
                        }
                    }
                },
                { "Street", (parent, caption) =>
                    {
                        if (parent is CenterOfPopulation center)
                        {
                            return CreateOrGetFolder<Street, CenterOfPopulation>((CenterOfPopulation)parent, caption);
                        }
                        else
                        {
                            return CreateOrGetFolder<Street, PlanningStructure>((PlanningStructure)parent, caption);
                        }
                    }
                },
                { "House", (parent, caption) => CreateOrGetFolder<DwellingHouse, Street>((Street)parent, caption) }
            };
            _actionDictionary = new Dictionary<Type, Action<object, object>>()
            {
                { typeof(ElectricalNetworksSubsidiary), (parent, child) => ((ClassifierOfMeterPointsByEnergyEntities)parent).AttributeEnergyManagementItems.Add((ElectricalNetworksSubsidiary)child) },
                { typeof(ElectricalNetworksDistrict), (parent, child) => ((ElectricalNetworksSubsidiary)parent).AttributeDistricts.Add((ElectricalNetworksDistrict)child) },
                { typeof(SubstationsGroupByVoltage), (parent, child) => ((ElectricalNetworksDistrict)parent).AttributeVoltageGroups.Add((SubstationsGroupByVoltage)child) },
                { typeof(HighVoltageSubstation), (parent, child) => ((SubstationsGroupByVoltage)parent).AttributeSubstations.Add((HighVoltageSubstation)child) },
                { typeof(Switchgear), (parent, child) =>
                    {
                        if (parent is HighVoltageSubstation substation)
                        {
                            substation.AttributeSwitchgears.Add((Switchgear)child);
                        }
                        else if (parent is LowVoltageSubstation techSubstation)
                        {
                            techSubstation.AttributeSwitchgears.Add((Switchgear)child);
                        }
                        else
                        {
                            _logger.Log($"Ошибка: ошибка создания {typeof(Switchgear)}");
                        }
                    }
                },
                { typeof(BusbarSection), (parent, child) => ((Switchgear)parent).AttributeBusbarSections.Add((BusbarSection)child) },
                { typeof(CubiclePowerLine), (parent, child) => ((BusbarSection)parent).AttributeCubicles.Add((CubiclePowerLine)child) },
                { typeof(PowerLine), (parent, child) => ((CubiclePowerLine)parent).AttributePowerLine = (PowerLine)child },
                { typeof(ElectricPylon), (parent, child) => ((PowerLine)parent).AttributePylons.Add((ElectricPylon)child) },
                { typeof(LowVoltageSubstation), (parent, child) => ((SubstationsGroupByVoltage)parent).AttributeSubstations.Add((LowVoltageSubstation)child) },
				// География
				{ typeof(SubjectRF), (parent, child) => ((ClassifierOfMeterPointsByGeoLocation)parent).AttributeGeographicAreas.Add((SubjectRF)child) },
                { typeof(District), (parent, child) => ((SubjectRF)parent).AttributeDistricts.Add((District)child) },
                { typeof(PlanningStructure), (parent, child) => ((District)parent).AttributePlanningStructures.Add((PlanningStructure)child) },
                { typeof(CenterOfPopulation), (parent, child) => ((District)parent).AttributeCentersOfPopulation.Add((CenterOfPopulation)child) },
                { typeof(Street), (parent, child) =>  // улица может быть добавлена либо к Городу, либо к ко всем остальноым
					{
                        if (parent is CenterOfPopulation center) // добавить к городу
						{
                            center.AttributeStreets.Add((Street)child);
                        }
                        else if (parent is PlanningStructure planning) // добавить к остальным
						{
                            planning.AttributeStreets.Add((Street)child);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Ошибка при создании улицы");
                        }
                    }
                },
                { typeof(DwellingHouse), (parent, child) => ((Street)parent).AttributeBuildings.Add((DwellingHouse)child) }
            };
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Алгоритм построения топологии (иерархии) сети:
        /// 1. Последовательно создает объекты от филиала до фидера/линии
        /// 2. Автоматически определяет тип подстанции по уровню напряжения
        /// 3. При создании низковольтной ТП создает ссылку (связь) с фидером высоковольтной ПС
        /// </remarks>
        public PowerLine BuildTechnicalHierarchy(TechnicalSideDto address, ClassifierOfMeterPointsByEnergyEntities cls, MeterPoint meterPoint,
            bool isNextValid, PowerLine highVoltagePowerLine = null)
        {
            try
            {
                var captions = new[]
                {
                    address.Subsidiary, address.District, address.SubstationVoltage, address.Substation,
                    address.Switchgear, address.BusbarSection, address.CubiclePowerLine, address.PowerLine
                };

                ClassifierItem parent = _technicalHierarchy["Subsidiary"](cls, captions[0]);
                int i = 0;
                foreach (var item in _technicalHierarchy)
                {
                    if (item.Key == "Subsidiary")
                    {
                        i++;
                        continue;
                    }

                    var current = item.Value(parent, captions[i]);
                    if (item.Key == "Substation" && highVoltagePowerLine != null)
                    {
                        CreateHighVoltageLinks((LowVoltageSubstation)current, highVoltagePowerLine, address);
                    }
                    parent = current;
                    i++;
                }

                if (parent is not PowerLine powerLine)
                {
                    _logger.Log($"Ошибка: ошибка при создании Иерархии сети");
                    return null;
                }

                return powerLine;
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка: ошибка при создании Иерархии сети: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Алгоритм построения географической иерархии:
        /// 1. Различает города и планировочные структуры по префиксу "г "
        /// 2. Автоматически привязывает линию низковольтной ТП к дому
        /// 3. Добавляет точку учета к дому
        /// </remarks>
        public DwellingHouse BuildGeographicHierarchy(GeographicalDto address, ClassifierOfMeterPointsByGeoLocation cls, MeterPoint meterPoint, PowerLine powerLine)
        {
            try
            {
                var captions = new[]
                {
                    address.Subject, address.District, address.Settlement, address.Street, address.House
                };

                ClassifierItem parent = _geographicHierarchy["Subject"](cls, captions[0]);

                int i = 0;
                foreach (var item in _geographicHierarchy)
                {
                    if (item.Key == "Subject")
                    {
                        i++;
                        continue;
                    }
                    var current = item.Value(parent, captions[i]);
                    parent = current;
                    i++;
                }

                if (parent is not DwellingHouse house)
                {
                    _logger.Log($"Ошибка: ошибка при создании Географии");
                    return null;
                }

                CreateGeographicLinks(house, meterPoint, powerLine);

                return house;
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка: ошибка при создании Географии: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Связывает дом с точкой учета и линией ТП
        /// </summary>
        /// <remarks>
        /// Добавляет точку учета в дом → устанавливает линию ТП
        /// </remarks>
        private void CreateGeographicLinks(DwellingHouse house, MeterPoint meterPoint, PowerLine powerLine)
        {
            if (house == null || meterPoint == null || powerLine == null)
            {
                _logger.Log($"Ошибка: не созданны ссылки дома");
                return;
            }

            if (!house.AttributeConsumerMeterPoints.GetValues().Contains(meterPoint))
            {
                house.AttributeConsumerMeterPoints.Add(meterPoint);
            }

            if (powerLine != null)
            {
                house.AttributePowerLine = powerLine;
            }
        }

        /// <summary>
        /// Создает высокую сторону ТП и связывает с фидером ПС
        /// </summary>
        /// <remarks>
        /// Автоматически строит цепочку: РУ → СШ → ячейка → фидер ПС
        /// </remarks>
        private void CreateHighVoltageLinks(LowVoltageSubstation substation, PowerLine powerLine, AddressDto.TechnicalDto.TechnicalSideDto address)
        {
            try
            {
                Switchgear switchgear = CreateOrGetFolder<Switchgear, LowVoltageSubstation>(substation, address.SubstationLinkVoltage);
                BusbarSection busBarSection = CreateOrGetFolder<BusbarSection, Switchgear>(switchgear, address.BusbarSection);
                CubiclePowerLine cubiclePowerLine = CreateOrGetFolder<CubiclePowerLine, BusbarSection>(busBarSection, address.CubiclePowerLine);
                cubiclePowerLine.AttributePowerLine = powerLine;
            }
            catch (Exception ex)
            {
                _logger.Log($"Ошибка: ошибка при привязке ТП к ПС: {ex.Message}");
            }
        }

        /// <summary>
        /// Создает или находит объект в иерархии
        /// </summary>
        /// <remarks>
        /// Проверяет существование объекта → создает новый при необходимости → связывает с родителем
        /// </remarks>
        private TChild CreateOrGetFolder<TChild, TParent>(TParent parent, string caption)
            where TChild : ClassifierItem
            where TParent : class
        {
            IEnumerable<ClassifierItem> lowerItems = Enumerable.Empty<ClassifierItem>();
            if (parent is Classifier cls)
            {
                lowerItems = cls.GetLowerItems();
            }
            else
            {
                lowerItems = (parent as ClassifierItem).GetLowerItems();
            }
            var existingFolder = lowerItems.FirstOrDefault(x => x.AttributeCaption == caption) as TChild;
            if (existingFolder != null)
                return existingFolder;
            TChild newFolder = _directoryOfCommonUserItems.AttributeCommonUserItems.AppendNew<TChild>().Value;
            newFolder.AttributeCaption = caption; // ClassifierItem имеет свойство AttributeCaption

            if (_actionDictionary.TryGetValue(typeof(TChild), out var addAction))
            {
                addAction(parent, newFolder);
                return newFolder;
            }
            _logger.Log($"Ошибка создания {typeof(TChild)}");

            return null;
        }
    }
}
