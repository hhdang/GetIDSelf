using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackgroundManager.Func
{
    public class GetAssemblyClass
    {
        static Type[] _assemblyArray;
        readonly static Object lockObject = new Object();

        public Type[] GetAssembly() {
            if( null == _assemblyArray ){            
                lock( lockObject ){
                    _assemblyArray = Assembly.GetExecutingAssembly().GetTypes();
                }
            }
            
            return _assemblyArray;
        }

        public Type[] GetFormAssembly() {
            Type[] assemblyArray = GetAssembly();
            string allType = typeof(Form).FullName;
            string mainType = typeof(frmMain).FullName;
            return Array.FindAll(assemblyArray, delegate(Type type)
            {
                return (type.BaseType.FullName == allType && type.FullName != mainType);
            });
        }

        public Type[] GetUserCtlAssembly() { 
            Type[] assemblyArray = GetAssembly();
            string allType = typeof(UserControl).FullName;
            string mainType = typeof(frmMain).FullName;
            return Array.FindAll(assemblyArray, delegate(Type type){
                return(type.BaseType.FullName == allType && type.FullName != mainType);
            });
        }

     }
}
