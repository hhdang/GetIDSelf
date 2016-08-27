using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundManager
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = true )]
    public class MyAttribute:Attribute
    {
        private string _name = "";
        private string _display = "";
        private string _value = "";

        public MyAttribute( string name, string value, string display ){
            _name = name;
            _value = value;
            _display = display;
        }

        public string Value{
            get{ return _value; }
            set{ _value = value; }
        }
        public string Display{
            get{ return _display; }
            set{ _display = value; }
        }
        public string Name{
            get{ return _name; }
            set{ _name = value; }
        }
    }
}
