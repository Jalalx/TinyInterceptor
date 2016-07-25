using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyInterceptor
{
    public interface IInterceptSupport
    {
        void BeforeExecute(object instance);

        void AfterExecute(object instance);

        void OnError(object instance, Exception ex);
    }
}
