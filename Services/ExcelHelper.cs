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
    internal class ExcelHelper : IExcelHelper
    {
        public List<AddressDtoResult> GetAddressResultList (ExcelWorksheet worksheet)
        {
            var addressResultList = new List<AddressDtoResult>();
            for (int i = 0; i < worksheet.Rows.Count; i++)
            {
                var addressResult = AddressDtoResult.GetAddressFromExcel(worksheet, i);
                addressResultList.Add(addressResult);
            }
            return addressResultList;
        }
    }
}
