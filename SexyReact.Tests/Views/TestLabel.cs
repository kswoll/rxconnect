using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SexyReact.Tests.Views
{
    public class TestLabel : RxObject
    {
        public Action TextSetHandler { get; set; }

        public TestLabel()
        {
            TextSetHandler = () => {};
        }

        public string Text
        {
            get
            {
                return Get<string>();
            }
            set
            {
                Set(value);
                TextSetHandler();
            }
        }
    }
}
