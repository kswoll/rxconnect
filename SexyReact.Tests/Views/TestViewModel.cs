using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SexyReact.Tests.Views
{
    public class TestViewModel : RxObject
    {
        public string StringProperty { get { return Get<string>(); } set { Set(value); } }
    }
}