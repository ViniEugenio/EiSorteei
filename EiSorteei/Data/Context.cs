using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EiSorteei.Data
{
    public class Context
    {
        public static EiSorteeiEntities _db;

        public static EiSorteeiEntities GetContext()
        {
            return _db = new EiSorteeiEntities();
        }
    }
}