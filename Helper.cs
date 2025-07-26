using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IerarchyPyr
{
    internal static class Helper
    {
        // поиск точек учета по серийному номеру прибора
        public static List<MeterPoint> GetFoundMeters(Dictionary<string, List<MeterPoint>> meterPoints, string serial)
        {
            if (meterPoints.TryGetValue(serial, out var foundMeters))
                return foundMeters;
            return new List<MeterPoint>();
        }

        // Метод создания Иерархии сети, возвращает строку Результата и Линию, которая будет использоваться в методе создания Географии для привязки дома к ТП
        public static (string result, PowerLine powerLine) CreateTechLinks(this ClassifierOfMeterPointsByEnergyEntities cls, MeterPoint mp, Address Address)
        {
            List<string> result = new List<string>();
            // Получить или создать Филиал
            var filialFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(ElectricalNetworksSubsidiary),
                parent: cls,
                caption: Address.filial,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((ClassifierOfMeterPointsByEnergyEntities)p).GetLowerItems(),
                result: ref result);
            if (filialFolder == null)
            {
                result.Add("Ошибка при создании Филиала");
                return (string.Join(", ", result), null);
            }

            // Получить или создать РЭС ПС
            var resFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(ElectricalNetworksDistrict),
                parent: filialFolder,
                caption: $"_ПС 35(110) кВ",
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((ElectricalNetworksSubsidiary)p).GetLowerItems(),
                result: ref result);
            if (resFolder == null)
            {
                result.Add("Ошибка при создании РЭСа ПС");
                return (string.Join(", ", result), null);
            }

            // Получить или создать группу ПС по напряжению
            var substationVoltageFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(SubstationsGroupByVoltage),
                parent: resFolder,
                caption: Address.substationVoltage,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((ElectricalNetworksDistrict)p).GetLowerItems(),
                result: ref result);
            if (substationVoltageFolder == null)
            {
                result.Add("Ошибка при создании группы ПС по напряжению");
                return (string.Join(", ", result), null);
            }
            // Получить или создать ПС
            var substationFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(HighVoltageSubstation),
                parent: substationVoltageFolder,
                caption: Address.substation,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((SubstationsGroupByVoltage)p).GetLowerItems(),
                result: ref result);
            if (substationFolder == null)
            {
                result.Add("Ошибка при создании ПС");
                return (string.Join(", ", result), null);
            }
            // Получить или создать РУ
            var switchgearFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(Switchgear),
                parent: substationFolder,
                caption: Address.switchgear,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((HighVoltageSubstation)p).GetLowerItems(),
                result: ref result);
            if (switchgearFolder == null)
            {
                result.Add("Ошибка при создании РУ ПС");
                return (string.Join(", ", result), null);
            }
            // Выбрать уровень напряжения РУ ПС
            var switchgear = switchgearFolder as Switchgear;
            if (Address.switchgear == "РУ-6 кВ")
                switchgear.AttributeVoltage = VoltageEnumItemClassInfo.Instances.Near6kV;
            if (Address.switchgear == "РУ-10 кВ")
                switchgear.AttributeVoltage = VoltageEnumItemClassInfo.Instances.Near10kV;

            // Получить или создать СШ
            var busbarsectionFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(BusbarSection),
                parent: switchgearFolder,
                caption: Address.busbarsection,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((Switchgear)p).GetLowerItems(),
                result: ref result);
            if (busbarsectionFolder == null)
            {
                result.Add("Ошибка при создании СШ ПС");
                return (string.Join(", ", result), null);
            }
            // Получить или создать ячейку ПС
            var cubFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(CubiclePowerLine),
                parent: busbarsectionFolder,
                caption: Address.cub,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((BusbarSection)p).GetLowerItems(),
                result: ref result);
            if (cubFolder == null)
            {
                result.Add("Ошибка при создании яч ПС");
                return (string.Join(", ", result), null);
            }
            // Получить или создать фидер ПС
            var powerLineFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(PowerLine),
                parent: cubFolder,
                caption: Address.powerLine,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((CubiclePowerLine)p).GetLowerItems(),
                result: ref result);
            if (powerLineFolder == null)
            {
                result.Add("Ошибка при создании ф ПС");
                return (string.Join(", ", result), null);
            }
            // УРОВЕНЬ ТП
            // Получить или создать РЭС ТП
            var techResFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(ElectricalNetworksDistrict),
                parent: filialFolder,
                caption: Address.res,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((ElectricalNetworksSubsidiary)p).GetLowerItems(),
                result: ref result);
            if (techResFolder == null)
            {
                result.Add("Ошибка при создании РЭСа ТП");
                return (string.Join(", ", result), null);
            }
            // Получить или создать группу ТП по напряжению
            var techSubstationVoltageFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(SubstationsGroupByVoltage),
                parent: techResFolder,
                caption: "ТП 0,4 кВ",
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((ElectricalNetworksDistrict)p).GetLowerItems(),
                result: ref result);
            if (techSubstationVoltageFolder == null)
            {
                result.Add("Ошибка при создании группы ТП по напряжению");
                return (string.Join(", ", result), null);
            }
            // Получить или создать ТП
            var techSubstationFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(LowVoltageSubstation),
                parent: techSubstationVoltageFolder,
                caption: Address.techSubstation,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((SubstationsGroupByVoltage)p).GetLowerItems(),
                result: ref result);
            if (techSubstationFolder == null)
            {
                result.Add("Ошибка при создании ТП");
                return (string.Join(", ", result), null);
            }
            // Заполнить у ТП поля Адрес, Код Uid СК-11 ТП и Код СУПА
            var techSubstation = techSubstationFolder as LowVoltageSubstation;
            techSubstation.AttributeAddress = Address.techAddress;
            techSubstation.WriteValueByAttributeCaption(Address.techSKcodeCaption, Address.techSKcode);
            techSubstation.WriteValueByAttributeCaption(Address.techSUPAcodeCaption, Address.techSUPAcode);

            // Получить или создать РУ по высокой стороне ТП
            var techHighSwitchgearFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(Switchgear),
                parent: techSubstationFolder,
                caption: Address.switchgear,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((LowVoltageSubstation)p).GetLowerItems(),
                result: ref result);
            if (techHighSwitchgearFolder == null)
            {
                result.Add("Ошибка при создании РУ по высокой стороне ТП");
                return (string.Join(", ", result), null);
            }
            // Выбрать уровень напряжения РУ по высокой стороне ТП
            var techHighSwitchgear = techHighSwitchgearFolder as Switchgear;
            if (Address.switchgear == "РУ-6 кВ")
                techHighSwitchgear.AttributeVoltage = VoltageEnumItemClassInfo.Instances.Near6kV;
            if (Address.switchgear == "РУ-10 кВ")
                techHighSwitchgear.AttributeVoltage = VoltageEnumItemClassInfo.Instances.Near10kV;

            // Получить или создать СШ по высокой стороне ТП
            var techHighBusbarsectionFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(BusbarSection),
                parent: techHighSwitchgearFolder,
                caption: "СШ-1",
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((Switchgear)p).GetLowerItems(),
                result: ref result);
            if (techHighBusbarsectionFolder == null)
            {
                result.Add("Ошибка при создании СШ по высокой стороне ТП");
                return (string.Join(", ", result), null);
            }
            // Получить или создать ячейку по высокой стороне ТП
            var techHighCubFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(CubiclePowerLine),
                parent: techHighBusbarsectionFolder,
                caption: "яч-1",
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((BusbarSection)p).GetLowerItems(),
                result: ref result);
            if (techHighCubFolder == null)
            {
                result.Add("Ошибка при создании ячейки по высокой стороне ТП");
                return (string.Join(", ", result), null);
            }
            // Добавить ссылку на фидер ПС
            var lowVoltageCub = techHighCubFolder as CubiclePowerLine;
            var powerLine = powerLineFolder as PowerLine;
            if (lowVoltageCub != null && powerLine != null)
                lowVoltageCub.AttributePowerLine = powerLine;


            // Получить или создать РУ по низкой стороне ТП
            var techSwitchgearFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(Switchgear),
                parent: techSubstationFolder,
                caption: Address.techSwitchgear,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((LowVoltageSubstation)p).GetLowerItems(),
                result: ref result);
            if (techSwitchgearFolder == null)
            {
                result.Add("Ошибка при создании РУ ТП");
                return (string.Join(", ", result), null);
            }
            // Выбрать уровень напряжения РУ по низкой стороне ТП
            var techSwitchgear = techSwitchgearFolder as Switchgear;
            techSwitchgear.AttributeVoltage = VoltageEnumItemClassInfo.Instances.Near0dot4kV;

            // Получить или создать СШ по низкой стороне ТП
            var techBusbarsectionFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(BusbarSection),
                parent: techSwitchgearFolder,
                caption: Address.techBusbarsection,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((Switchgear)p).GetLowerItems(),
                result: ref result);
            if (techBusbarsectionFolder == null)
            {
                result.Add("Ошибка при создании СШ ТП");
                return (string.Join(", ", result), null);
            }
            // Получить или создать ячейку по низкой стороне ТП
            var techCubFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(CubiclePowerLine),
                parent: techBusbarsectionFolder,
                caption: "яч-1",
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((BusbarSection)p).GetLowerItems(),
                result: ref result);
            if (techCubFolder == null)
            {
                result.Add("Ошибка при создании яч ТП");
                return (string.Join(", ", result), null);
            }
            // Получить или создать линию по низкой стороне ТП
            var techPowerLineFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(PowerLine),
                parent: techCubFolder,
                caption: Address.techPowerLine,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((CubiclePowerLine)p).GetLowerItems(),
                result: ref result);
            if (powerLineFolder == null)
            {
                result.Add("Ошибка при создании линии ТП");
                return (string.Join(", ", result), null);
            }

            var techPowerLine = techPowerLineFolder as PowerLine;
            var returnResult = "Иерархия сущуствует/создана";

            return (returnResult, techPowerLine);
        }

        // Метод создания Географии и привязки дома к ТП из Иерархии сети
        public static string CreateGeoLinks(this ClassifierOfMeterPointsByGeoLocation cls, MeterPoint mp, Address Address, PowerLine powerLine)
        {
            List<string> result = new List<string>();
            // Получить или создать субъект
            var subjectFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(SubjectRF),
                parent: cls,
                caption: Address.subject,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((ClassifierOfMeterPointsByGeoLocation)p).GetLowerItems(),
                result: ref result);
            if (subjectFolder == null)
            {
                result.Add("Ошибка при создании папки Субъекта");
                return string.Join(", ", result);
            }
            // Получить или создать район
            var districtFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(District),
                parent: subjectFolder,
                caption: Address.district,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((SubjectRF)p).GetLowerItems(),
                result: ref result);
            if (districtFolder == null)
            {
                result.Add("Ошибка при создании папки Района");
                return string.Join(", ", result);
            }
            // Получить или создать населенный пункт
            var settlementFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(PlanningStructure),
                parent: districtFolder,
                caption: Address.settlement,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((District)p).GetLowerItems(),
                result: ref result);
            if (settlementFolder == null)
            {
                result.Add("Ошибка при создании папки Населенного пункта");
                return string.Join(", ", result);
            }
            // Получить или создать улицу
            var streetFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(Street),
                parent: settlementFolder,
                caption: Address.street,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((ClassifierItem)p).GetLowerItems(),
                result: ref result);
            if (streetFolder == null)
            {
                result.Add("Ошибка при создании папки Улицы");
                return string.Join(", ", result);
            }
            // Получить или создать дом
            var houseFolder = GeImportHelper.CreateOrGetFolder(
                entityType: typeof(DwellingHouse),
                parent: streetFolder,
                caption: Address.house,
                tanker: DirectoryOfCommonUserItems.OnlyInstance,
                getLowerItems: p => ((Street)p).GetLowerItems(),
                result: ref result);
            if (houseFolder == null)
            {
                result.Add("Ошибка при создании папки Дома");
                return string.Join(", ", result);
            }

            var house = houseFolder as DwellingHouse;
            if (house != null)
            {
                var houseMeterPoints = house.AttributeConsumerMeterPoints.GetValues().ToHashSet();
                var newMeterPoint = mp as MeterPoint;
                if (!houseMeterPoints.Contains(newMeterPoint))
                {
                    houseMeterPoints.Add(mp);
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
    }
}
