
using GemBox.Spreadsheet;
using ObjStudioClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

public class Script
{
    public static object Execute()
    {
        {
            var worksheet = WorkbookNonExcel.Worksheets.FirstOrDefault();
            int choice = ReportParams.choice;

            string geoClsName = "тест_География"; // Имя Географического классификатора
            string techClsName = "тест_Иерархия сети"; // Имя классификатора Иерархии сети
            string SKcodeCaption = "Код Uid СК-11 ТП";
            string SUPAcodeCaption = "Код СУПА";
            int resultCol = 21;

            ClassifierOfMeterPointsByGeoLocation clsGeo = ClassifierOfMeterPointsByGeoLocation.GetInstances().FirstOrDefault(qwe => qwe.AttributeCaption == geoClsName); // выбор географического классификатора
            ClassifierOfMeterPointsByEnergyEntities clsTech = ClassifierOfMeterPointsByEnergyEntities.GetInstances().FirstOrDefault(x => x.AttributeCaption == techClsName); // выбор классификатора сети
            var tanker = DirectoryOfCommonUserItems.OnlyInstance.AttributeCommonUserItems;

            // Создаем Dictionary из серийных номеров ПУ, имеющих место установки, и списком Приборов учета
            var metersGroups = MeterPoint.GetInstances().Where(x => x.AttributeElectricityMeter?.AttributeSerialNumber != null)
                                                        .GroupBy(x => x.AttributeElectricityMeter?.AttributeSerialNumber.TrimStart('0'))
                                                        .ToDictionary(g => g.Key, g => g.ToList());
            int i = 1;
            int rowCount = worksheet.Rows.Count;
            while (worksheet.Cells[i, 0].Value != null)
            {
                bool stop = false;
                var result = new List<string>();
                Address CurrentAddress = new Address();
                // Заполнение полей класса Адреса по очереди со столбца B
                var properties = typeof(Address).GetProperties().ToList();
                for (int col = 1; col - 1 < properties.Count - 2; col++)
                {
                    var cellValue = worksheet.Cells[i, col].Value;
                    if (cellValue == null)
                    {
                        worksheet.Cells[i, resultCol].Value = "Незаполненные поля";
                        worksheet.Cells[i, col].Style.FillPattern.SetSolid(Color.Red);
                        stop = true;
                        break;
                    }
                    properties[col - 1].SetValue(CurrentAddress, Convert.ChangeType(cellValue, properties[col - 1].PropertyType));
                }
                if (stop)
                {
                    i++;
                    continue;
                }
                CurrentAddress.techSKcodeCaption = SKcodeCaption;
                CurrentAddress.techSUPAcodeCaption = SUPAcodeCaption;
                if (choice == 1)
                {
                    string serial = worksheet.Cells[i, 0].Value.ToString().TrimStart('0');
                    List<MeterPoint> meterPoint = new List<MeterPoint>();
                    if (metersGroups.TryGetValue(serial, out var foundMeters))
                        meterPoint = foundMeters;
                    if (meterPoint.Count == 0)
                        worksheet.Cells[i, 7].Value = "Не найдена ТУ с таким ПУ";
                    if (meterPoint.Count > 1)
                        worksheet.Cells[i, 7].Value = "Найдено несколько ТУ с таким ПУ";
                    if (meterPoint.Count == 1)
                    {
                        var techCreation = clsTech.CreateTechLinks(meterPoint.First(), CurrentAddress);
                        result.Add(techCreation.result);
                        result.Add(clsGeo.CreateGeoLinks(meterPoint.First(), CurrentAddress, techCreation.powerLine));
                    }
                }
                if (choice == 2)
                {
                    // Создание иерархии без привязки ПУ
                    var techCreation = clsTech.CreateTechLinks(null, CurrentAddress);
                    result.Add(techCreation.result);
                }
               
                if (i % 1000 == 0)
                    AddLogInfo($"Выполнено {i} из {rowCount} строк");
                worksheet.Cells[i, resultCol].Value = string.Join(", ", result);
                i++;
            }
        }
        return 0;
    }
}
