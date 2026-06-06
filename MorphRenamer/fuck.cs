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
using System.Text.RegularExpressions;
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
        private void MergeDuplicateMorphs(IPXPmx pmx)
        {
            var groups = pmx.Morph
                .GroupBy(m => m.Name)
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var group in groups)
            {
                IPXMorph keeper = group.First();

                foreach (IPXMorph duplicate in group.Skip(1))
                {
                    foreach (var offset in duplicate.Offsets)
                        keeper.Offsets.Add(offset);

                    pmx.Morph.Remove(duplicate);
                }
            }
            var invalidItems = pmx.ExpressionNode.Items
                .Where(item => item.IsMorph &&
                       (item.MorphItem == null || !pmx.Morph.Contains(item.MorphItem.Morph)))
                .ToList();

            foreach (var item in invalidItems)
            {
                pmx.ExpressionNode.Items.Remove(item);
            }
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
        public float scale = 1.0f;
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
                var match = morphMap.Keys.FirstOrDefault(k =>
                    Regex.IsMatch(morph.Name, $@"(?<![a-zA-Z0-9]){Regex.Escape(k)}$", RegexOptions.IgnoreCase));
                if (match != null)
                {
                    morph.Name = morphMap[match].NameJ;
                    morph.NameE = morphMap[match].NameE;
                    morph.Panel = PanelID(morphMap[match].ID);
                }
            }
            MergeDuplicateMorphs(this.pmx); // if you're giving morphs the same name i'm assuming you want to merge... why else?
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

        private void button3_Click(object sender, EventArgs e)
        {
            get_pmxdata();
            IPXPmxBuilder pmx_build = this.host.Builder.Pmx;

            pmx.Morph.Clear();
            var dlg = new OpenFileDialog();
            dlg.Filter = "JSON (*.json)|*.json|All files (*.*)|*.*";
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            string jsonPath = dlg.FileName;
            try {
            var root = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, JObject>>>(System.IO.File.ReadAllText(jsonPath));

            foreach (var morphEntry in root)
            {
                string morphName = morphEntry.Key;

                IPXMorph morph = pmx.Morph.FirstOrDefault(m => m.Name == morphName);
                if (morph == null)
                {
                    morph = pmx_build.Morph();
                    morph.Name = morphName;
                    morph.NameE = morphName;
                    morph.Kind = MorphKind.Bone;
                    morph.Panel = 4;
                    pmx.Morph.Add(morph);
                }
                else
                {
                    morph.Kind = MorphKind.Bone;
                }

                foreach (var boneEntry in morphEntry.Value)
                {
                    string boneName = boneEntry.Key;
                    JObject data = boneEntry.Value;

                    IPXBone bone = pmx.Bone.FirstOrDefault(b => b.Name == boneName);
                    if (bone == null) continue;

                    var pos = data["position"].ToObject<float[]>();
                    var rot = data["rotation"].ToObject<float[]>();

                    IPXBoneMorphOffset bm = pmx_build.BoneMorphOffset();
                    bm.Bone = bone;

                    bm.Translation = new V3(
                        pos[0] * scale,
                        pos[1] * scale,
                        pos[2] * scale
                    );

                    bm.Rotation = new Q(rot[0], rot[1], rot[2], rot[3]);

                    morph.Offsets.Add(bm);
                }
            }
            var sorted = pmx.Morph.OrderBy(m => m.Name).ToList();
            pmx.Morph.Clear();
                foreach (var m in sorted)
                    pmx.Morph.Add(m); 
            }
            catch
            {
                MessageBox.Show("womp");
                return;
            }
            update_pmxdata();
        }

        public IPEPluginHost host;
        public IPEConnector connect;
        public IPXPmx pmx;
        public IList<IPXMorph> morph;

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (float.TryParse(ScaleFactor2.Text, out float newScale))
            {
                scale = newScale;
            }
        }
    }
}
