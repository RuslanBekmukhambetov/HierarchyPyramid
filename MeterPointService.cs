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
    public class MeterPointService
    {
        private readonly Dictionary<string, List<MeterPoint>> _meterPoints;
        public MeterPointService(Dictionary<string, List<MeterPoint>> meterPoints)
        {
            _meterPoints = meterPoints;
        }
        public MeterPoint GetMeterPointBySerial(string serialNumer)
        {
            if (_meterPoints.TryGetValue(serialNumer, out var foundMeterPoints))
            {
                if (foundMeterPoints.Count == 1)
                    return foundMeterPoints[0];
            }
            return null;
        }
    }
}
