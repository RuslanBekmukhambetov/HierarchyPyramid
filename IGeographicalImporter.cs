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
    interface IGeographicalImporter
    {
        string CreateGeographicalLinks(MeterPoint meterPoint, AddressDto address, PowerLine powerLine);
    }
}
