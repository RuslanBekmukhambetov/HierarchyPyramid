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

namespace HierarchyPyr.Models
{
    public class AddressDto
    {
        public string SerialNumber { get; set; }
        public string ElectricalNetworksSubsidiary { get; set; }
        public string ElectricalNetworksDistrict { get; set; }
        public string HighVoltageSubstationsGroupByVoltage { get; set; }
        public string HighVoltageSubstation { get; set; }
        public string HighVoltageSwitchgear { get; set; }
        public string HighVoltageBusbarSection { get; set; }
        public string HighVoltageCubiclePowerLine { get; set; }
        public string HighVoltagePowerLine { get; set; }
        public string LowVoltageElectricalNetworksDistrict { get; set; }
        public string LowVoltageSubstationsGroupByVoltage { get; set; }
        public string LowVoltageSubstation { get; set; }
        public string LowVoltageHighSideSwitchgear { get; set; }
        public string LowVoltageHighSideBusbarSection { get; set; }
        public string LowVoltageHighSideCubiclePowerLine { get; set; }
        public string LowVoltageLowSideSwitchgear { get; set; }
        public string LowVoltageLowSideBusbarSection { get; set; }
        public string LowVoltageLowSideCubiclePowerLine { get; set; }
        public string Subject { get; set; }
        public string District { get; set; }
        public string Settlement { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
      
    }
    public class AddressDtoResult
    {
        public AddressDto Address { get; }
        public List<string> Errors { get; }
        public bool IsValid => Errors.Count == 0;
        public AddressDtoResult(AddressDto address, List<string> errors)
        {
            Address = address;
            Errors = errors ?? new List<string>();
        }
        public static AddressDtoResult GetAddressFromExcel(ExcelWorksheet worksheet, int row)
        {
            var newAddress = new AddressDto();
            var errors = new List<string>();
            string readCell(int row, int column)
            {
                if (worksheet.Cells[row, column].Value == null || string.IsNullOrEmpty(worksheet.Cells[row, column].Value.ToString()))
                {
                    errors.Add($"Ошибка: ячейка[{row + 1}, {column + 1}] не заполнена");
                    return string.Empty;
                }
                return worksheet.Cells[row, column].Value.ToString();
            }
            newAddress.SerialNumber = readCell(row, Constants.AddressDtoColumnIndices.SerialNumber);
            newAddress.ElectricalNetworksSubsidiary = readCell(row, Constants.AddressDtoColumnIndices.ElectricalNetworksSubsidiary);
            newAddress.ElectricalNetworksDistrict = readCell(row, Constants.AddressDtoColumnIndices.ElectricalNetworksDistrict);
            newAddress.HighVoltageSubstationsGroupByVoltage = readCell(row, Constants.AddressDtoColumnIndices.HighVoltageSubstationsGroupByVoltage);
            newAddress.HighVoltageSubstation = readCell(row, Constants.AddressDtoColumnIndices.HighVoltageSubstation);
            newAddress.HighVoltageSwitchgear = readCell(row, Constants.AddressDtoColumnIndices.HighVoltageSwitchgear);
            newAddress.HighVoltageBusbarSection = readCell(row, Constants.AddressDtoColumnIndices.HighVoltageBusbarSection);
            newAddress.HighVoltageCubiclePowerLine = readCell(row, Constants.AddressDtoColumnIndices.HighVoltageCubiclePowerLine);
            newAddress.HighVoltagePowerLine = readCell(row, Constants.AddressDtoColumnIndices.HighVoltagePowerLine);
            newAddress.LowVoltageElectricalNetworksDistrict = readCell(row, Constants.AddressDtoColumnIndices.LowVoltageElectricalNetworksDistrict);
            newAddress.LowVoltageSubstationsGroupByVoltage = readCell(row, Constants.AddressDtoColumnIndices.LowVoltageSubstationsGroupByVoltage);
            newAddress.LowVoltageSubstation = readCell(row, Constants.AddressDtoColumnIndices.LowVoltageSubstation);
            newAddress.LowVoltageHighSideSwitchgear = readCell(row, Constants.AddressDtoColumnIndices.LowVoltageHighSideSwitchgear);
            newAddress.LowVoltageHighSideBusbarSection = readCell(row, Constants.AddressDtoColumnIndices.LowVoltageHighSideBusbarSection);
            newAddress.LowVoltageHighSideCubiclePowerLine = readCell(row, Constants.AddressDtoColumnIndices.LowVoltageHighSideCubiclePowerLine);
            newAddress.LowVoltageLowSideSwitchgear = readCell(row, Constants.AddressDtoColumnIndices.LowVoltageLowSideSwitchgear);
            newAddress.LowVoltageLowSideBusbarSection = readCell(row, Constants.AddressDtoColumnIndices.LowVoltageLowSideBusbarSection);
            newAddress.LowVoltageLowSideCubiclePowerLine = readCell(row, Constants.AddressDtoColumnIndices.LowVoltageLowSideCubiclePowerLine);
            newAddress.Subject = readCell(row, Constants.AddressDtoColumnIndices.Subject);
            newAddress.District = readCell(row, Constants.AddressDtoColumnIndices.District);
            newAddress.Settlement = readCell(row, Constants.AddressDtoColumnIndices.Settlement);
            newAddress.Street = readCell(row, Constants.AddressDtoColumnIndices.Street);
            newAddress.House = readCell(row, Constants.AddressDtoColumnIndices.House);
            return new AddressDtoResult(newAddress, errors);
        }
    }
}

