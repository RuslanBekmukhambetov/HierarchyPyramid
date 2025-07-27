using GemBox.Spreadsheet;
using HierarchyPyr.Models;
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
    class TechnicalImporter : ITechnicalImporter
    {
        private readonly ClassifierOfMeterPointsByEnergyEntities _classifer;
        private readonly ImportHelper _import;
        public TechnicalImporter(ClassifierOfMeterPointsByEnergyEntities classifier, ImportHelper import)
        {
            _classifer = classifier;
            _import = import;
        }
        public (string result, PowerLine powerLine) CreateTechnicalLinks(AddressDto address)
        {
            var result = new List<string>();

            var electricalNetworksSubsidiaryFolder = CreateElectricalNetworksSubsidiaryFolder(address.ElectricalNetworksSubsidiary) // ElectricalNetworksSubsidiary
                ?? throw new InvalidOperationException("Не удалось создать филиал");
            var electricalNetworksDistrictFolder = CreateElectricalNetworksDistrictFolder(address.ElectricalNetworksDistrict, electricalNetworksSubsidiaryFolder) // ElectricalNetworksDistrict
                ?? throw new InvalidOperationException("Не удалось создать РЭС");
            var substationsGroupByVoltageFolder = CreateSubstationsGroupByVoltageFolder(address.HighVoltageSubstationsGroupByVoltage, electricalNetworksDistrictFolder) // SubstationsGroupByVoltage
                ?? throw new InvalidOperationException("Не удалось создать Группу ПС по уровню напряжения");
            var highVoltageSubstationFolder = CreateHighVoltageSubstationFolder(address.HighVoltageSubstation, substationsGroupByVoltageFolder) // HighVoltageSubstation
                ?? throw new InvalidOperationException("Не удалось создать ПС");
            var highVoltageSwitchgearFolder = CreateHighVoltageSwitchgearFolder(address.HighVoltageSwitchgear, highVoltageSubstationFolder) // Switchgear
                ?? throw new InvalidOperationException("Не удалось создать РУ ПС");
            var highVoltageBusbarSectionFolder = CreateBusbarSectionFolder(address.HighVoltageBusbarSection, highVoltageSwitchgearFolder) // BusbarSection
                ?? throw new InvalidOperationException("Не удалось создать СШ ПС");
            var highVoltageCubiclePowerLineFolder = CreateCubiclePowerLineFolder(address.HighVoltageCubiclePowerLine, highVoltageBusbarSectionFolder) // CubiclePowerLine
                ?? throw new InvalidOperationException("Не удалось создать ячейку ПС");
            var highVoltagePowerLineFolder = CreatePowerLineFolder(address.HighVoltagePowerLine, highVoltageCubiclePowerLineFolder) // PowerLine
                ?? throw new InvalidOperationException("Не удалось создать фидер ПС");
            // Уровень ТП 0,4 кВ
            var lowVoltageElectricalNetworksDistrictFolder = CreateElectricalNetworksDistrictFolder(address.LowVoltageElectricalNetworksDistrict, ElectricalNetworksSubsidiaryFolder) // ElectricalNetworksDistrict
                ?? throw new InvalidOperationException("Не удалось создать РЭС 0,4 кВ");
            var lowVoltageSubstationsGroupByVoltageFolder = CreateSubstationsGroupByVoltageFolder(address.LowVoltageSubstationsGroupByVoltage, lowVoltageElectricalNetworksDistrictFolder) // SubstationsGroupByVoltage
                ?? throw new InvalidOperationException("Не удалось создать группу ТП по уровню напряжения");
            var lowVoltageSubstationFolder = CreateLowVoltageSubstationFolder(address.LowVoltageSubstation, lowVoltageSubstationsGroupByVoltageFolder) // LowVoltageSubstation
                ?? throw new InvalidOperationException("Не удалось создать ТП");
            var lowVoltageHighSideSwitchgearFolder = CreateLowVoltageSwitchgearFolder(address.LowVoltageHighSideSwitchgear, lowVoltageSubstationFolder) // Switchgear
                ?? throw new InvalidOperationException("Не удалось создать РУ ТП");
            var lowVoltageHighSideBusbarSectionFolder = CreateBusbarSectionFolder(address.LowVoltageHighSideBusbarSection, lowVoltageHighSideSwitchgearFolder) // BusbarSection
                ?? throw new InvalidOperationException("Не удалось создать СШ ТП");
            var lowVoltageHighSideCubiclePowerLineFolder = CreateCubiclePowerLineFolder(address.LowVoltageHighSideCubiclePowerLine, lowVoltageHighSideBusbarSectionFolder) //  CubiclePowerLine
                ?? throw new InvalidOperationException("Не удалось создать линию ТП");

            // Добавить ссылку на фидер ПС
            var lowVoltageCubicle = lowVoltageHighSideCubiclePowerLineFolder as CubiclePowerLine;
            var powerLine = lowVoltageHighSideCubiclePowerLineFolder as PowerLine;
            if (lowVoltageCubicle != null && powerLine != null)
                lowVoltageCubicle.AttributePowerLine = powerLine;

            var lowVoltageSwitchgearFolder = CreateLowVoltageSwitchgearFolder(address.LowVoltageLowSideSwitchgear, lowVoltageSubstationFolder) // Switchgear
                ?? throw new InvalidOperationException("Не удалось создать РУ ТП по низкой стороне");
            var lowVoltageBusbarSectionFolder = CreateBusbarSectionFolder(address.LowVoltageLowSideBusbarSection, lowVoltageSwitchgearFolder) // BusbarSection
                ?? throw new InvalidOperationException("Не удалось создать СШ ТП по низкой стороне");
            var lowVoltageCubiclePowerLineFolder = CreateCubiclePowerLineFolder(address.LowVoltageLowSideCubiclePowerLine, lowVoltageBusbarSectionFolder) // CubiclePowerLine
                ?? throw new InvalidOperationException("Не удалось создать ячейку по низкой стороне");
            var lowVoltagePowerLineFolder = CreatePowerLineFolder(address.LowVoltageLowSideCubiclePowerLine, lowVoltageCubiclePowerLineFolder) // PowerLine
                ?? throw new InvalidOperationException("Не удалось создать линию по низкой стороне");
            var lowVoltagePowerLine = lowVoltagePowerLineFolder as PowerLine;
            var returnResult = "Иерархия существует/создана";
            return (returnResult, powerLine);
        }
        private ElectricalNetworksSubsidiary CreateElectricalNetworksSubsidiaryFolder(string caption)
        {
            return _import.CreateOrGetFolder(
                parent: _classifer,
                caption: caption,
                getLowerItems: p => ((ClassifierOfMeterPointsByEnergyEntities)p).GetLowerItems());
        }
        private ElectricalNetworksDistrict CreateElectricalNetworksDistrictFolder(string caption, ElectricalNetworksSubsidiary parent)
        {
            return _import.CreateOrGetFolder(
                parent: parent,
                caption: caption,
                getLowerItems: p => ((ElectricalNetworksSubsidiary)p).GetLowerItems());
        }
        private SubstationsGroupByVoltage CreateSubstationsGroupByVoltageFolder(string caption, ElectricalNetworksDistrict parent)
        {
            return _import.CreateOrGetFolder(
                parent: parent,
                caption: caption,
                getLowerItems: p => ((ElectricalNetworksDistrict)p).GetLowerItems());
        }
        private HighVoltageSubstation CreateHighVoltageSubstationFolder(string caption, SubstationsGroupByVoltage parent)
        {
            return _import.CreateOrGetFolder(
                parent: parent,
                caption: caption,
                getLowerItems: p => ((SubstationsGroupByVoltage)p).GetLowerItems());
        }
        private LowVoltageSubstation CreateLowVoltageSubstationFolder(string caption, SubstationsGroupByVoltage parent)
        {
            return _import.CreateOrGetFolder(
                parent: parent,
                caption: caption,
                getLowerItems: p => ((SubstationsGroupByVoltage)p).GetLowerItems());
        }
        private Switchgear CreateHighVoltageSwitchgearFolder(string caption, HighVoltageSubstation parent)
        {
            return _import.CreateOrGetFolder(
                parent: parent,
                caption: caption,
                getLowerItems: p => ((HighVoltageSubstation)p).GetLowerItems());
        }
        private Switchgear CreateLowVoltageSwitchgearFolder(string caption, LowVoltageSubstation parent)
        {
            return _import.CreateOrGetFolder(
                parent: parent,
                caption: caption,
                getLowerItems: p => ((LowVoltageSubstation)p).GetLowerItems());
        }
        private BusbarSection CreateBusbarSectionFolder(string caption, Switchgear parent)
        {
            return _import.CreateOrGetFolder(
                parent: parent,
                caption: caption,
                getLowerItems: p => ((Switchgear) p).GetLowerItems());
        }
        private CubiclePowerLine CreateCubiclePowerLineFolder(string caption, BusbarSection parent)
        {
            return _import.CreateOrGetFolder(
                parent: parent,
                caption: caption,
                getLowerItems: p => ((BusbarSection)p).GetLowerItems());
        }
        private PowerLine CreatePowerLineFolder(string caption, CubiclePowerLine parent)
        {
            return _import.CreateOrGetFolder(
                parent: parent,
                caption: caption,
                getLowerItems: p => ((CubiclePowerLine)p).GetLowerItems());
        }
    }
}
