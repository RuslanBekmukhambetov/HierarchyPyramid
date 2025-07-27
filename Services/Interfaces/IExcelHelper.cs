using GemBox.Spreadsheet;
using HierarchyPyr.Models;
using ObjStudioClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HierarchyPyr.Services.Interfaces
{
    internal interface IExcelHelper
    {
        List<AddressDtoResult> GetAddressResultList(ExcelWorksheet worksheet);
    }
}
