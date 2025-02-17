using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet.Types;

namespace xemuh2stats.objects
{
    public static class obs_communicator
    {
        public static OBSWebsocket obs;
        public static bool connected = false;
        public static List<SceneBasicInfo> Scenes;
        public static string current_scene;

        public static void connect(string url, string password)
        {
            obs = new OBSWebsocket();
            obs.Connected += onConnect;
            obs.Disconnected += onDisconnect;
            obs.CurrentProgramSceneChanged += onCurrentProgramSceneChanged;
            obs.Connect(url, password);
            
        }

        public static void disconnect()
        {
            obs.Disconnect();
        }

        private static void onConnect(object sender, EventArgs e)
        {
            connected = true;
            Scenes = obs.GetSceneList().Scenes;
            current_scene = obs.GetCurrentProgramScene();
        }

        private static void onDisconnect(object sender, OBSWebsocketDotNet.Communication.ObsDisconnectionInfo e)
        {
            connected = false;
        }

        private static void onCurrentProgramSceneChanged(object sender, ProgramSceneChangedEventArgs args)
        {
            current_scene = args.SceneName;
        }

        public static List<SceneBasicInfo> get_scenes()
        {
            return obs.GetSceneList().Scenes;
        }

        public static void update_text(string source, string text)
        {
            if (!connected) return;

            try
            {
                obs.SetInputSettings(source, new JObject() {{"text", text}});
            }
            catch
            {

            }
        }
    }
}
