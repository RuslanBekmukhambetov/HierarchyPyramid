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
    public static class Constants
    {
        public const string GeoClassifierCaption = "Географический классификатор";
        public const string TechnicallClassifierCaption = "Иерархия сети";
        public static int StartRowIndex = 1;
        public static int ResultColumnIndex = 23;
        public static class AddressDtoColumnIndices
        {
            public const int SerialNumber = 0;
            public const int ElectricalNetworksSubsidiary = 1;
            public const int ElectricalNetworksDistrict = 2;
            public const int HighVoltageSubstationsGroupByVoltage = 3;
            public const int HighVoltageSubstation = 4;
            public const int HighVoltageSwitchgear = 5;
            public const int HighVoltageBusbarSection = 6;
            public const int HighVoltageCubiclePowerLine = 7;
            public const int HighVoltagePowerLine = 8;
            public const int LowVoltageElectricalNetworksDistrict = 9;
            public const int LowVoltageSubstationsGroupByVoltage = 10;
            public const int LowVoltageSubstation = 11;
            public const int LowVoltageHighSideSwitchgear = 12;
            public const int LowVoltageHighSideBusbarSection = 13;
            public const int LowVoltageHighSideCubiclePowerLine = 14;
            public const int LowVoltageLowSideSwitchgear = 15;
            public const int LowVoltageLowSideBusbarSection = 16;
            public const int LowVoltageLowSideCubiclePowerLine = 17;
            public const int Subject = 18;
            public const int District = 19;
            public const int Settlement = 20;
            public const int Street = 21;
            public const int House = 22;
        }
    }
}
