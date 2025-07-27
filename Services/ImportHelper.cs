using GemBox.Spreadsheet;
using HierarchyPyr.Services.Interfaces;
using ObjStudioClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HierarchyPyr.Services
{
    public class ImportHelper : IImportHelper
    {
        private readonly DirectoryOfCommonUserItems _tanker;
        private static readonly Dictionary<Type, Func<RDMetadataClasses.DirectoryOfCommonUserItems, object>> _createActions;
        private static readonly Dictionary<Type, Action<object, object>> _addToParentActions;
        static ImportHelper()
        {
            _createActions = new()
            {
  		    // Иерархия сети
  		        { typeof(ElectricalNetworksSubsidiary), tanker => tanker.AttributeCommonUserItems.AppendNew<ElectricalNetworksSubsidiary>().Value },
                  { typeof(ElectricalNetworksDistrict), tanker => tanker.AttributeCommonUserItems.AppendNew<ElectricalNetworksDistrict>().Value },
                  { typeof(SubstationsGroupByVoltage), tanker => tanker.AttributeCommonUserItems.AppendNew<SubstationsGroupByVoltage>().Value },
                { typeof(HighVoltageSubstation), tanker => tanker.AttributeCommonUserItems.AppendNew<HighVoltageSubstation>().Value },
                { typeof(Switchgear), tanker => tanker.AttributeCommonUserItems.AppendNew<Switchgear>().Value },
                { typeof(BusbarSection), tanker => tanker.AttributeCommonUserItems.AppendNew<BusbarSection>().Value },
                { typeof(CubiclePowerLine), tanker => tanker.AttributeCommonUserItems.AppendNew<CubiclePowerLine>().Value },
                { typeof(ElectricPylon), tanker => tanker.AttributeCommonUserItems.AppendNew<ElectricPylon>().Value },
                { typeof(LowVoltageSubstation), tanker => tanker.AttributeCommonUserItems.AppendNew<LowVoltageSubstation>().Value },
                { typeof(PowerLine), tanker => tanker.AttributeCommonUserItems.AppendNew<PowerLine>().Value },
  		        // География
  		        { typeof(SubjectRF), tanker => tanker.AttributeCommonUserItems.AppendNew<SubjectRF>().Value },
                { typeof(District), tanker => tanker.AttributeCommonUserItems.AppendNew<District>().Value },
                { typeof(PlanningStructure), tanker => tanker.AttributeCommonUserItems.AppendNew<PlanningStructure>().Value },
                { typeof(CenterOfPopulation), tanker => tanker.AttributeCommonUserItems.AppendNew<CenterOfPopulation>().Value },
                { typeof(Street), tanker => tanker.AttributeCommonUserItems.AppendNew<Street>().Value },
                { typeof(DwellingHouse), tanker => tanker.AttributeCommonUserItems.AppendNew<DwellingHouse>().Value }

            };
            _addToParentActions = new()
            {
    	    // Иерархия сети
    	        { typeof(ElectricalNetworksSubsidiary), (parent, child) => ((ClassifierOfMeterPointsByEnergyEntities)parent).AttributeEnergyManagementItems.Add((ElectricalNetworksSubsidiary)child) },
                { typeof(ElectricalNetworksDistrict), (parent, child) => ((ElectricalNetworksSubsidiary)parent).AttributeDistricts.Add((ElectricalNetworksDistrict)child) },
                { typeof(SubstationsGroupByVoltage), (parent, child) => ((ElectricalNetworksDistrict)parent).AttributeVoltageGroups.Add((SubstationsGroupByVoltage)child) },
                { typeof(HighVoltageSubstation), (parent, child) => ((SubstationsGroupByVoltage)parent).AttributeSubstations.Add((HighVoltageSubstation)child) },
                { typeof(Switchgear), (parent, child) =>
                    {
                        if (parent is HighVoltageSubstation substation)
                            substation.AttributeSwitchgears.Add((Switchgear)child);
                        else if (parent is LowVoltageSubstation techSubstation)
                            techSubstation.AttributeSwitchgears.Add((Switchgear)child);
                        else
                            throw new InvalidOperationException($"Ошибка при создании СШ");
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
    				        center.AttributeStreets.Add((Street)child);
                        else if (parent is PlanningStructure planning) // добавить к остальным
    				        planning.AttributeStreets.Add((Street)child);
                        else
                            throw new InvalidOperationException($"Ошибка при создании улицы");
                    }
                },
                { typeof(DwellingHouse), (parent, child) => ((Street)parent).AttributeBuildings.Add((DwellingHouse)child) }
            };
        }
        public ImportHelper(DirectoryOfCommonUserItems tanker)
        {
            _tanker = tanker;
        }
        
        // универсальный метод для получения или создания элемента классификатора
        public T CreateOrGetFolder<T>(
            object parent,
            string caption,
            Func<object, IEnumerable<object>> getLowerItems) where T : class
        {
            Type entityType = typeof(T);
            // Проверка город или остальное
            Type actualType = entityType;
            if (entityType == typeof(CenterOfPopulation) || entityType == typeof(PlanningStructure))
            {
                actualType = caption.StartsWith("г ")
                    ? typeof(CenterOfPopulation)
                    : typeof(PlanningStructure);
            }

            // проверка наличия папки в классификаторе
            var lowerItems = getLowerItems(parent);
            var existingFolder = lowerItems.FirstOrDefault(x => x.GetType().GetProperty("AttributeCaption")?.GetValue(x)?.ToString() == caption);

            if (existingFolder != null)
                return existingFolder;

            // создать экземпляр класса entityType 
            if (!_createActions.TryGetValue(actualType, out var createAction))
                throw new InvalidOperationException($"Ошибка добавления типа {actualType.Name}");
            var newFolder = createAction(_tanker);
            newFolder.GetType().GetProperty("AttributeCaption")?.SetValue(newFolder, caption);
            // добавить созданный экземпляр в классификатор
            if (_addToParentActions.TryGetValue(actualType, out var addAction))
            {
                addAction(parent, newFolder);
                //		result.Add($"Добавлена {caption}");
                return (T)newFolder;
            }

            throw new InvalidOperationException($"Ошибка добавление {actualType.Name}");
        }
    }
}
