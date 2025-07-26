using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IerarchyPyr
{
    internal class Address
    {
        public string filial { get; set; } // B - ФИЛИАЛ
        public string res { get; set; } // C - РЭС
        public string substationVoltage { get; set; } // D - УРОВЕНЬ НАПРЯЖЕНИЯ ПС
        public string substation { get; set; } // E - ПС
        public string switchgear { get; set; } // F - РУ ПС
        public string busbarsection { get; set; } // G - СШ ПС
        public string cub { get; set; } // H - ЯЧ ПС
        public string powerLine { get; set; } // I - Ф ПС
        public string techSubstation { get; set; } // J - ТП
        public string techAddress { get; set; } // 
        public string techSKcode { get; set; }
        public string techSUPAcode { get; set; }
        public string techSwitchgear { get; set; } // K - РУ ТП
        public string techBusbarsection { get; set; } // L - СШ ТП
        public string techPowerLine { get; set; } // M - ЯЧ ТП
        public string subject { get; set; } // N - СУБЪЕКТ РФ
        public string district { get; set; } // O - РАЙОН
        public string settlement { get; set; } // P - НАСЕЛЕННЫЙ ПУНКТ
        public string street { get; set; } // Q - УЛИЦА
        public string house { get; set; } // R - ДОМ
        public string techSKcodeCaption { get; set; }
        public string techSUPAcodeCaption { get; set; }

    }
}
