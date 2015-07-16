using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SexyReact.Tests.Views
{
    public class TestLabel : RxObject
    {
        public string Text { get { return Get<string>(); } set { Set(value); } }
    }
}
