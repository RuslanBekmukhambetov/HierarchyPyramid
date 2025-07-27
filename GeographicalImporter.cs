using GemBox.Spreadsheet;
using ObjStudioClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HierarchyPyr
{
    class GeographicalImporter : IGeographicalImporter
    {
        private readonly ClassifierOfMeterPointsByEnergyEntities _classifer;
        private readonly ImportHelper _import;
        public GeographicalImporter(ClassifierOfMeterPointsByEnergyEntities classifier, ImportHelper import)
        {
            _classifer = classifier;
            _import = import;
        }
        public string CreateGeographicalLinks(MeterPoint meterPoint, AddressDto address, PowerLine powerLine)
        {
            var result = new List<string>();
            var subjectRFFolder = CreateSubjectRFFolder(address.Subject) // SubjectRF
                ?? throw new InvalidOperationException("Не удалось создать Субъект РФ");
            var districtFolder = CreateDistrictFolder(address.District, subjectRFFolder) // District
                ?? throw new InvalidOperationException("Не удалось создать район");
            var planningStructureFolder = CreatePlanningStructure(address.Settlement, districtFolder) // PlanningStructure
                ?? throw new InvalidOperationException("Не удалось создать населенный пункт");
            var streetFolder = CreateStreetFolder(address.Street, planningStructureFolder) // Street
                ?? throw new InvalidOperationException("Не удалось создать улицу");
            var dwellingHouseFolder = CreateDwellingHouse(address.House, streetFolder) // DwellingHouse
                ?? throw new InvalidOperationException("Не удалось создать дом");

            var house = dwellingHouseFolder as DwellingHouse;
            if (house != null)
            {
                var houseMeterPoints = house.AttributeConsumerMeterPoints.GetValues().ToHashSet();
                if (!houseMeterPoints.Contains(meterPoint))
                {
                    houseMeterPoints.Add(meterPoint);
                    house.AttributeConsumerMeterPoints.SetValues(houseMeterPoints);
                    result.Add($"Создана ссылка на ПУ");
                }
                else
                    result.Add($"Ссылка уже существует");
                // Добавить привязку ТП к дому
                if (powerLine != null)
                {
                    house.AttributePowerLine = powerLine;
                    result.Add($"Привязана ТП");
                }
                else
                    result.Add($"Ошибка привязки ТП");

            }
            return string.Join(", ", result);
        }
        private SubjectRF CreateSubjectRFFolder(string caption)
        {
            return _import.CreateOrGetFolder(
                parent: _classifer,
                caption: caption,
                getLowerItems: p => ((ClassifierOfMeterPointsByGeoLocation)p).GetLowerItems());
        }
        private District CreateDistrictFolder(string caption, SubjectRF parent)
        {
            return _import.CreateOrGetFolder(
                parent: _classifer,
                caption: caption,
                getLowerItems: p => ((SubjectRF)p).GetLowerItems());
        }
        private PlanningStructure CreatePlanningStructure(string caption, District parent)
        {
            return _import.CreateOrGetFolder(
                parent: _classifer,
                caption: caption,
                getLowerItems: p => ((District)p).GetLowerItems());
        }
        private Street CreateStreetFolder(string caption, PlanningStructure parent)
        {
            return _import.CreateOrGetFolder(
                parent: _classifer,
                caption: caption,
                getLowerItems: p => ((PlanningStructure)p).GetLowerItems());
        }
        private DwellingHouse CreateDwellingHouse(string caption, Street parent)
        {
            return _import.CreateOrGetFolder(
                parent: _classifer,
                caption: caption,
                getLowerItems: p => ((Street)p).GetLowerItems());
        }
    }
}
