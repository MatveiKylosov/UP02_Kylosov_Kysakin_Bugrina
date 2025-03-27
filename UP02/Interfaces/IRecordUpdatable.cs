using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP02.Interfaces
{
    public interface IRecordUpdatable
    {
        event EventHandler RecordUpdate;
    }
}
