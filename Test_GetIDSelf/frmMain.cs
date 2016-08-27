using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BackgroundManager.Func;

namespace BackgroundManager
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            this.IsMdiContainer = true;

            InitTreeList();
        }

        private void InitTreeList()
        {
            GetAssemblyClass getass = new GetAssemblyClass();
            Type[] frmTypes = getass.GetFormAssembly();
            Type[] ctlTypes = getass.GetUserCtlAssembly();
            mainTree.Nodes.Clear();

            var typeArray = new List<Type>();
            typeArray.AddRange( frmTypes );
            typeArray.AddRange( ctlTypes );
            foreach( Type type in typeArray ){
                TreeNode node;
                string name = type.Name;
                MyAttribute[] customAttributes = type.GetCustomAttributes( typeof(MyAttribute), true ) as MyAttribute[];
                if( customAttributes != null && customAttributes.Length > 0 ){
                    name = customAttributes[0].Display;
                }
                node = new TreeNode( name );
                node.Tag = type;
                mainTree.Nodes.Add( node );
            }
        }

        private void mainTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if( e.Node != null ){
                Type t = e.Node.Tag as Type;
                if( t != null ){
                    UserControl c = Activator.CreateInstance(t) as UserControl;
                    if( c != null ){
                        this.splitContainer1.Panel2.Controls.Clear();
                        this.splitContainer1.Panel2.Controls.Add( c );
                        c.Dock = DockStyle.Fill;
                    }
                }

                Form c2 = Activator.CreateInstance(t) as Form;
                if( c2 != null ){
                    c2.MdiParent = this;
                    this.splitContainer1.Panel2.Controls.Clear();
                    this.splitContainer1.Panel2.Controls.Add( c2 );
                    c2.Dock = DockStyle.Fill;
                    c2.Show();
                }
            }
        }
    }
}
