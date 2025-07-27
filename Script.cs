
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
    
    public class Script
    {
        private readonly ITechnicalImporter _technicalImporter;
        private readonly IGeographicalImporter _geographycalImporter;
        private readonly IMeterPointService _meterPointService;
        private readonly IExcelHelper _excelHelper;
        
        public Script()
        {
            var tanker = DirectoryOfCommonUserItems.OnlyInstance;
            var importHelper = new ImportHelper(tanker);

            var techClassifier = ClassifierOfMeterPointsByEnergyEntities.GetInstances().FirstOrDefault(x => x.AttributeCaption == Constants.TechnicallClassifierCaption);
            var geoClassifier = ClassifierOfMeterPointsByGeoLocation.GetInstances().FirstOrDefault(x => x.AttributeCaption == Constants.GeoClassifierCaption);
            var meterPoints = MeterPoint.GetInstances.Where(x => x.AttributeElectricityMeter != null)
                .GroupBy(x => x.AttributeElectricityMeter.AttributeSerialNumber)
                .ToDictionary(g => g.Key, g => g.ToList);

            _technicalImporter = new TechnicalImporter(geoClassifier, importHelper);
            _geographycalImporter = new GeographicalImporter(geoClassifier, importHelper);
            _meterPointService = new MeterPointService(meterPoints);
            _excelHelper = new ExcelHelper();
        }
        public static object Execute()
        {
            var script = new Script();
            return script.Run();
        }

        private object Run()
        {
            var worksheet = WorkbookNonExcel.Worksheets.FirstOrDefault();
            if (worksheet == null)
                return 0;
            int row = Constants.StartRowIndex;
            var addressResultList = _excelHelper.GetAddressResultList(worksheet);
            foreach (var addressResult in addressResultList)
            {
                if (addressResult.IsValid)
                    worksheet.Cells[row, Constants.ResultColumnIndex].Value = CreateLinks(addressResult.Address);
                else
                    worksheet.Cells[row, Constants.ResultColumnIndex].Value = string.Join(",", addressResult.Errors);
                row++;
            }
            return 0;
        }
        
        private string CreateLinks(AddressDto address)
        {
            var result = new List<string>();
            var meterPoint = _meterPointService.GetMeterPointBySerial(address.SerialNumber);
            if (meterPoint == null)
                return "Точка учета не найдена";
            var technicalCreation = _technicalImporter.CreateTechnicalLinks(address);
            result.Add(technicalCreation.result);
            var geograpicalCreation = _geographycalImporter.CreateGeographicalLinks(meterPoint, address, technicalCreation.powerLine);
            result.Add(geograpicalCreation);
            return string.Join(",", result);
        }
    }
}