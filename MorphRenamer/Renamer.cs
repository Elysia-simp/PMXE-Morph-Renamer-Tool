using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PECSScriptPlugin;
using PEPlugin;
using PEPlugin.Pmx;
using PEPlugin.SDX;
using PEPlugin.View;
using PEPlugin.Vme;
using SlimDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MorphRenamer
{
    public class Renamer : PEPluginClass
    {

        public Renamer()
        {
            this.m_option = new PEPluginOption(false, true, "Morph Renamer");
        }

        public override void Run(IPERunArgs args)
        {
            base.Run(args);
            fuck form = new fuck(args);
            form.Show();
        }
    }
}
