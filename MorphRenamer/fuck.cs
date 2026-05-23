using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PEPlugin;
using PEPlugin.Pmd;
using PEPlugin.Pmx;
using PEPlugin.SDX;
using PEPlugin.View;
using SlimDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PECSScriptPlugin
{
    public partial class fuck : Form
    {
        public fuck(IPERunArgs args)
        {
            InitializeComponent();
            this.host = args.Host;
            this.connect = args.Host.Connector;
        }

        public void get_pmxdata()
        {
            this.pmx = this.connect.Pmx.GetCurrentState();
            this.morph = this.pmx.Morph;
        }
        public void update_pmxdata()
        {
            this.connect.Pmx.Update(this.pmx);
            this.connect.View.PMDView.UpdateModel();
            this.connect.View.PMDView.UpdateView();
            this.connect.Form.UpdateList(UpdateObject.Morph);
        }
        struct MorphNames
        {
            public string NameJ, NameE, ID;
            //public int ID;
            public MorphNames(string j, string e, string id) { NameJ = j; NameE = e; ID = id; }
        }
        Dictionary<string, MorphNames> morphMap = new Dictionary<string, MorphNames>();

        Dictionary<string, int> PanelIDList = new Dictionary<string, int>()
        {
            { "hidden", 0 },
            { "brow", 1 },
            { "eye", 2 },
            { "mouth", 3 },
            { "other", 4 },
        };
        string PanelName(int id) => PanelIDList.FirstOrDefault(x => x.Value == id).Key;
        int PanelID(string name) => PanelIDList[name];
        private void button1_Click(object sender, EventArgs e)
        {
            get_pmxdata();
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                ofd.DefaultExt = "json";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string json = System.IO.File.ReadAllText(ofd.FileName);
                    var raw = JsonConvert.DeserializeObject<List<Dictionary<string, JArray>>>(json);

                    morphMap.Clear();
                    foreach (var entry in raw)
                    {
                        foreach (var kvp in entry)
                        {
                            string key = kvp.Key;
                            string nameJ = kvp.Value[0].ToString();
                            string nameE = kvp.Value[1].ToString();
                            string id = kvp.Value[2].ToString();
                            morphMap[key] = new MorphNames(nameJ, nameE, id);
                        }
                    }
                }
            }
            foreach (var morph in this.morph)
            {
                var match = morphMap.Keys.FirstOrDefault(k => morph.Name.EndsWith(k, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    morph.Name = morphMap[match].NameJ;
                    morph.NameE = morphMap[match].NameE;
                    morph.Panel = PanelID(morphMap[match].ID);
                }
            }
            update_pmxdata();
            MessageBox.Show("Renamed all morphs in Json.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            get_pmxdata();

            var lines = this.morph.Select(m =>
                $"  {{\"{m.Name}\": [ \"{m.Name}\", \"{m.Name}\", \"{PanelName(m.Panel)}\"]}}");
            string json = "[\n" + string.Join(",\n", lines) + "\n]";

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                sfd.DefaultExt = "json";
                sfd.FileName = "morphs";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(sfd.FileName, json);
                    MessageBox.Show("Saved to " + sfd.FileName);
                }
            }
        }
        public IPEPluginHost host;
        public IPEConnector connect;
        public IPXPmx pmx;
        public IList<IPXMorph> morph;
    }
}
