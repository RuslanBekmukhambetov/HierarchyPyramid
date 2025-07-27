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

namespace HierarchyPyr.Services.Interfaces
{
    public interface IImportHelper
    {
        T CreateOrGetFolder<T>(
            object parent,
            string caption,
            Func<object, IEnumerable<object>> getLowerItems) where T : class;
    }
}
