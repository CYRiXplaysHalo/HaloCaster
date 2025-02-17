using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.CompilerServices;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using OBSWebsocketDotNet.Types.Events;
using xemuh2stats.classes;
using xemuh2stats.enums;
using xemuh2stats.objects;
using System.IO;
using DocumentFormat.OpenXml.Vml;
using Path = System.IO.Path;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using WhatTheFuck.classes;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Numerics;
using WhatTheFuck.extensions;
using WhatTheFuck.objects;


namespace xemuh2stats
{
    public partial class Form1 : Form
    {
        public static bool is_valid = false;
        public static bool real_time_lock = false;
        public static bool dump_lock = false;

        public static bool time_lock = false;
        public static DateTime StartTime;
        

        public static Process xemu_proccess;

        public static List<real_time_player_stats> real_time_cache = new List<real_time_player_stats>();


        public Form1()
        {
            InitializeComponent();

            for (int i = 0; i < 16; i++)
            {
                players_table.Rows.Add();
                debug_table.Rows.Add();
            }

            for (int i = 0; i < weapon_stat.weapon_list.Count; i++)
            {
                weapon_stat_table.Rows.Add();
                weapon_stat_table.Rows[i].Cells[0].Value = weapon_stat.weapon_list[i];
            }

            for (int i = 0; i < 8; i++)
            {
                obs_scene_link_table.Rows.Add();
            }

            foreach (configuration configuration in Program.configurations.AsList)
            {
                configuration_combo_box.Items.Add(configuration.name);
            }

            main_tab_control.TabPages.RemoveAt(1);
            main_tab_control.TabPages.RemoveAt(1);

            weapon_player_select.Tag = 0;

            game_event_monitor.add_event_callbaack(add_event_text);
        }

        private void main_timer_Tick(object sender, EventArgs e)
        {
            if (xemu_proccess != null)
            {
                if (xemu_proccess.HasExited)
                {
                    is_valid = false;
                    configuration_combo_box.Enabled = true;
                    settings_group_box.Enabled = true;
                    xemu_launch_button.Enabled = true;
                }
            }

            if (is_valid)
            {
                var game_ending = Program.memory.ReadBool(Program.game_state_resolver["game_ending"].address);

                if (game_ending)
                {
                    if (!real_time_lock)
                    {
                        Program.variant_details_cache.end_time = DateTime.Now;

                        int player_count =
                            Program.memory.ReadInt(Program.game_state_resolver["game_state_players"].address + 0x3C);

                        real_time_cache.Clear();
                        for (int i = 0; i < player_count; i++)
                        {
                            real_time_cache.Add(objects.real_time_player_stats.get(i));
                        }


                        real_time_lock = true;
                    }
                }
                else
                {
                    real_time_lock = false;
                }

                var cycle = (life_cycle) Program.memory.ReadInt(Program.exec_resolver["life_cycle"].address);

                life_cycle_status_label.Text = $@"Life Cycle: {cycle.ToString()} |";

                if (cycle == life_cycle.in_lobby)
                {
                    Program.memory.WriteBool(Program.exec_resolver["profile_enabled"].address,
                        !profile_disabled_check_box.Checked, false);
                }

                if (cycle == life_cycle.in_game)
                {
                    var variant = variant_details.get();
                    variant_status_label.Text = $@"Variant: {variant.name} |";
                    game_type_status_label.Text = $@"Game Type: {variant.game_type.ToString()} |";
                    map_status_label.Text = $@"Map: {variant.map_name} |";
                    game_event_monitor.read_events();
                    if (!time_lock && !string.IsNullOrEmpty(variant.name))
                    {
                        Program.variant_details_cache = variant_details.get();
                        Program.variant_details_cache.start_time = DateTime.Now;
                        time_lock = true;
                    }
                }
                else
                {
                    time_lock = false;
                }

                if (cycle == life_cycle.post_game)
                {
                    if (!dump_lock)
                    {
                        game_state_object.clear_cache();
                        cache_file_tags.clear_cache();
                        net_game_equipment.clear_cache();
                        game_event_monitor.reset();

                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        List<s_post_game_player> post_game_ = new List<s_post_game_player>();

                        for (int i = 0; i < real_time_cache.Count; i++)
                        {
                            post_game_.Add(post_game_report.get(i));
                        }

                        xls_generator.dump_game_to_sheet($"{timestamp}", real_time_cache, post_game_, Program.variant_details_cache);
                        dump_lock = true;
                    }
                }
                else
                {
                    dump_lock = false;
                }

                switch (main_tab_control.SelectedTab.Text)
                {
                    case "Players":
                    {
                        render_players_tab();
                        break;
                    }
                    case "Weapon Stats":
                    {
                        render_weapon_stat_tab();
                        break;
                    }
                    case "debug":
                    {
                        render_debug_tab();
                        break;
                    }
                    case "game events":
                    {
                        render_game_events_tab();
                        break;
                    }
                }

                if (cycle == life_cycle.in_game)
                {
                    //weapon_stat.s_weapon_stat astat = weapon_stat.get_weapon_stats(0, 5);

                    //if (astat.shots_fired != 0 && astat.shots_hit != 0)
                    //{
                    //    double accuracy = (double)astat.shots_hit / astat.shots_fired;
                    //    accuracy = Math.Round(accuracy * 100, 2);
                    //    obs_communicator.update_text("accuracy", $"Accuracy: {accuracy}%");
                    //}
                    //else
                    //{
                    //    obs_communicator.update_text("accuracy", $"Accuracy: 0%");
                    //}

                    //obs_communicator.update_text("kills", $"Kills: {astat.kills}");

                    if(!string.IsNullOrEmpty(obs_communicator.current_scene))
                    {
                        for (var i = 0; i < obs_scene_link_table.Rows.Count; i++)
                        {
                            if(((DataGridViewComboBoxCell)obs_scene_link_table.Rows[i].Cells[0]).Value == null)
                                continue;

                            if (((DataGridViewComboBoxCell) obs_scene_link_table.Rows[i].Cells[0]).Value.ToString() ==
                                obs_communicator.current_scene)
                            {
                                for (var j = 0; j < 16; j++)
                                {
                                    if(((DataGridViewComboBoxCell)obs_scene_link_table.Rows[i].Cells[1]).Value == null)
                                        continue;

                                    string name = Program.memory.ReadStringUnicode(
                                        Program.exec_resolver["session_players"].address + (j * 0xA4), 16, false);

                                    if (((DataGridViewComboBoxCell) obs_scene_link_table.Rows[i].Cells[1]).Value.ToString() == name)
                                    {
                                        weapon_stat.s_weapon_stat astat = weapon_stat.get_weapon_stats(j, 5);

                                        if (astat.shots_fired != 0 && astat.shots_hit != 0)
                                        {
                                            double accuracy = (double) astat.shots_hit / astat.shots_fired;
                                            accuracy = Math.Round(accuracy * 100, 2);
                                            obs_communicator.update_text("accuracy", $"Accuracy: {accuracy}%");
                                        }
                                        else
                                        {
                                            obs_communicator.update_text("accuracy", $"Accuracy: 0%");
                                        }

                                        obs_communicator.update_text("kills", $"Kills: {astat.kills}");

                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }

        private void add_event_text(string event_record)
        {
            game_events_text_box.Text += event_record + "\n";
            game_events_text_box.ScrollToCaret();
        }

        private uint last_event = 0;
        private void render_game_events_tab()
        {
            //uint out_index = 0;
            //var events = game_event_monitor.get_new_events(last_event, out out_index);

            //if (events.Any())
            //{
            //    foreach (var game_event in events)
            //    {
            //        game_events_text_box.Text += game_event_monitor.format_event(game_event) + "\n";
            //    }

            //    last_event = out_index;
            //}
            //game_events_text_box.SelectionStart = game_events_text_box.Text.Length;
            // scroll it automatically
            //game_events_text_box.ScrollToCaret();
        }

        private void render_weapon_stat_tab()
        {
            int test_player_count =
                Program.memory.ReadInt(Program.game_state_resolver["game_state_players"].address + 0x3C);

            if ((int) weapon_player_select.Tag != test_player_count)
            {
                weapon_player_select.Items.Clear();
                weapon_player_select.Tag = test_player_count;
                var cur = weapon_player_select.SelectedText;
                var ncur = -1;
                for (int i = 0; i < test_player_count; i++)
                {
                    var name = game_state_player.name(i);
                    if (name == String.Empty && i == 0)
                    {
                        weapon_player_select.Tag = 0;
                        break;
                    }

                    weapon_player_select.Items.Add(name);
                    if (cur == name)
                        ncur = i;
                }

                weapon_player_select.SelectedIndex = ncur;

            }

            if (weapon_player_select.SelectedIndex != -1)
            {
                for (int i = 0; i < weapon_stat.weapon_list.Count; i++)
                {
                    weapon_stat.s_weapon_stat
                        stat = weapon_stat.get_weapon_stats(weapon_player_select.SelectedIndex, i);
                    weapon_stat_table.Rows[i].Cells[1].Value = stat.kills;
                    weapon_stat_table.Rows[i].Cells[2].Value = stat.head_shots;
                    weapon_stat_table.Rows[i].Cells[3].Value = stat.deaths;
                    weapon_stat_table.Rows[i].Cells[4].Value = stat.suicide;
                    weapon_stat_table.Rows[i].Cells[5].Value = stat.shots_fired;
                    weapon_stat_table.Rows[i].Cells[6].Value = stat.shots_hit;
                }
            }
        }

        private void render_players_tab()
        {
            int test_player_count =
                Program.memory.ReadInt(Program.game_state_resolver["game_state_players"].address + 0x3C);

            var variant = variant_details.get();
            for (int i = 0; i < test_player_count; i++)
            {
                var player = real_time_player_stats.get(i);
                players_table.Rows[i].Cells[0].Value = player.GetPlayerName();
                players_table.Rows[i].Cells[1].Value = player.player.team_index.GetDisplayName();

                switch (variant.game_type)
                {
                    case game_type.capture_the_flag:
                        players_table.Rows[i].Cells[2].Value = player.game_stats.ctf_scores;
                        break;
                    case game_type.slayer:
                        players_table.Rows[i].Cells[2].Value = player.game_stats.kills;
                        break;
                    case game_type.oddball:
                        if (player.game_stats.oddball_score > 0)
                        {
                            try
                            {
                                players_table.Rows[i].Cells[2].Value =
                                    TimeSpan.FromSeconds(player.game_stats.oddball_score).ToString("mm:ss");
                            }
                            catch (Exception)
                            {
                                players_table.Rows[i].Cells[2].Value = player.game_stats.oddball_score;
                            }
                        }

                        break;
                    case game_type.king_of_the_hill:
                    {
                        try
                        {
                            players_table.Rows[i].Cells[2].Value =
                                TimeSpan.FromSeconds(player.game_stats.oddball_score).ToString("mm:ss");
                        }
                        catch (Exception)
                        {
                            players_table.Rows[i].Cells[2].Value = player.game_stats.oddball_score;
                        }
                    }
                        break;
                    case game_type.juggernaut:
                        players_table.Rows[i].Cells[2].Value = player.game_stats.kills_as_juggernaut;
                        break;
                    case game_type.territories:
                    {
                        try
                        {
                            players_table.Rows[i].Cells[2].Value =
                                TimeSpan.FromSeconds(player.game_stats.oddball_score).ToString("mm:ss");
                        }
                        catch (Exception)
                        {
                            players_table.Rows[i].Cells[2].Value = player.game_stats.oddball_score;
                        }
                    }
                        break;
                    case game_type.assault:
                        players_table.Rows[i].Cells[2].Value = player.game_stats.assault_score;
                        break;
                    default:
                        players_table.Rows[i].Cells[2].Value = player.game_stats.kills;
                        break;
                }

                players_table.Rows[i].Cells[3].Value = player.game_stats.kills;
                players_table.Rows[i].Cells[4].Value = player.game_stats.deaths;
                players_table.Rows[i].Cells[5].Value = player.game_stats.assists;
                if (player.game_stats.deaths > 0)
                {
                    float kda = (float) (player.game_stats.kills + player.game_stats.assists) /
                                player.game_stats.deaths;
                    players_table.Rows[i].Cells[6].Value = Math.Round(kda, 3);
                }
                else
                {
                    players_table.Rows[i].Cells[6].Value = player.game_stats.kills + player.game_stats.assists;
                }
            }
        }

        private void render_debug_tab()
        {

            //0xE1780003 beaver creek scenario datum

            var a = cache_file_tags.get_tag_address(0xE1780003);
            var b = a;

            //int test_player_count =
            //    Program.memory.ReadInt(Program.game_state_resolver["game_state_players"].address + 0x3C);
            //for (int i = 0; i < test_player_count; i++)
            //{
            //    var player = real_time_player_stats.get(i);
            //    var g_player = game_state_player.get(i);
            //    var g_player_object = game_state_player.get_player_object(i);

            //    var weapon = game_state_object.unit_object_get_weapon_type(game_state_player.get_player_unit_index(i));
            //    var stat = weapon_stat.get_weapon_stats(i, weapon_stat.damage_reporting_type_to_results_index(weapon));
            //    //var biped_datum = Program.memory.ReadUInt((long)g_player_object);
            //    //var biped_tag = cache_file_tags.get_tag_address(biped_datum);

            //    debug_table.Rows[i].Cells[0].Value = stat.shots_fired;

            //    debug_table.Rows[i].Cells[1].Value = weapon.ToString();
            //    debug_table.Rows[i].Cells[2].Value = player.game_addr.ToString("X");
            //    debug_table.Rows[i].Cells[3].Value = player.medal_addr.ToString("X");
            //    debug_table.Rows[i].Cells[4].Value = player.weapon_addr.ToString("X");
            //}
        }

        private void xemu_browse_button_Click(object sender, EventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    xemu_path_text_box.Text = dialog.SelectedPath;
                }
            }
        }

        private string FindFileInDirectory(string folderPath, string fileNameToFind)
        {
            try
            {
                // Search in the current directory
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    if (Path.GetFileName(file).Equals(fileNameToFind, StringComparison.OrdinalIgnoreCase))
                    {
                        return file; // Return the full path of the file if found
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }

            return null; // Return null if the file is not found
        }

        private void resolve_addresses()
        {
            // offsets are host (host virtual address - host_base_executable_address)
            Program.exec_resolver.Add(new offset_resolver_item("game_stats", 0x35ADF02, ""));
            Program.exec_resolver.Add(new offset_resolver_item("weapon_stats", 0x35ADFE0, ""));
            Program.exec_resolver.Add(new offset_resolver_item("medal_stats", 0x35ADF4E, ""));
            Program.exec_resolver.Add(new offset_resolver_item("life_cycle", 0x35E4F04, ""));
            Program.exec_resolver.Add(new offset_resolver_item("post_game_report", 0x363A990, ""));
            Program.exec_resolver.Add(new offset_resolver_item("session_players", 0x35AD344, ""));
            Program.exec_resolver.Add(new offset_resolver_item("variant_info", 0x35AD0EC, ""));
            Program.exec_resolver.Add(new offset_resolver_item("profile_enabled", 0x3569128, ""));
            Program.exec_resolver.Add(new offset_resolver_item("game_engine_globals", 0x35A53B8, ""));
            Program.exec_resolver.Add(new offset_resolver_item("players", 0x35A44F4, ""));
            Program.exec_resolver.Add(new offset_resolver_item("objects", 0x35BBBD0, ""));
            Program.exec_resolver.Add(new offset_resolver_item("game_results_globals", 0x35ACFB0, ""));
            Program.exec_resolver.Add(new offset_resolver_item("game_results_globals_extra", 0x35CF014, ""));
            Program.exec_resolver.Add(new offset_resolver_item("disable_rendering", 0x3520E22, ""));
            Program.exec_resolver.Add(new offset_resolver_item("lobby_players", 0x35CC008, ""));
            Program.exec_resolver.Add(new offset_resolver_item("tags", 0x360558C, ""));

            Program.game_state_resolver.Add(new offset_resolver_item("game_state_players", 0, "players"));
            Program.game_state_resolver.Add(new offset_resolver_item("game_state_objects", 0, "object"));
            Program.game_state_resolver.Add(new offset_resolver_item("game_state_tags", 0, "sgat"));
            Program.game_state_resolver.Add(new offset_resolver_item("game_ending", 0, ""));
            Program.game_state_resolver.Add(new offset_resolver_item("game_engine", 0, ""));

            // xemu base_address + xbe base_address
            var host_base_executable_address = (long) Program.qmp.Translate(0x80000000) + 0x5C000;

            foreach (offset_resolver_item offsetResolverItem in Program.exec_resolver)
            {
                offsetResolverItem.address = host_base_executable_address + offsetResolverItem.offset;
            }

            var game_state_players_addr = Program.qmp.Translate(Program.memory.ReadUInt(Program.exec_resolver["players"].address));
            var game_state_objects_addr = Program.qmp.Translate(Program.memory.ReadUInt(Program.exec_resolver["objects"].address));
            var game_engine_addr = Program.qmp.Translate(Program.memory.ReadUInt(Program.exec_resolver["game_engine_globals"].address));
            var game_state_offset = Program.memory.ReadUInt(Program.exec_resolver["tags"].address);

            while (game_state_offset == 0)
            {
                game_state_offset = Program.memory.ReadUInt(Program.exec_resolver["tags"].address);
            }

            var game_state_tags_addr = Program.qmp.Translate(game_state_offset);

            Program.game_state_resolver["game_state_players"].address = (long) game_state_players_addr;
            Program.game_state_resolver["game_state_objects"].address = (long) game_state_objects_addr;
            Program.game_state_resolver["game_state_tags"].address = (long) game_state_tags_addr;
            Program.game_state_resolver["game_engine"].address = (long) game_engine_addr;
            Program.game_state_resolver["game_ending"].address = (long)game_state_players_addr - 0x1E8;

            main_tab_control.TabPages.Add(players_tab_page);
            main_tab_control.TabPages.Add(weapon_stats_tab);
        }

        private void xemu_launch_button_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(xemu_path_text_box.Text))
            {
                var xemu_path = FindFileInDirectory(xemu_path_text_box.Text, "xemu.exe");
                if (!string.IsNullOrEmpty(xemu_path))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = xemu_path,
                        Arguments = $"-qmp tcp:localhost:{int.Parse(xemu_port_text_box.Text)},server,nowait",
                    };
                    xemu_proccess = Process.Start(startInfo);
                    System.Threading.Thread.Sleep(5000);

                    Program.qmp = new QmpProxy(int.Parse(xemu_port_text_box.Text));

                    Program.memory = new MemoryHandler(xemu_proccess);
                    resolve_addresses();
                    is_valid = true;

                    configuration_combo_box.Enabled = false;
                    settings_group_box.Enabled = false;
                    xemu_launch_button.Enabled = false;
                    websocket_communicator.start(websocket_bind_text_box.Text, websocket_bind_port_text_box.Text);
                }
            }
        }

        private void dump_stats_to_binary_button_Click(object sender, EventArgs e)
        {
            var mem = Program.memory.ReadMemory(false, Program.exec_resolver["game_results_globals"].address, 0xDD8C);
            var mem2 = Program.memory.ReadMemory(false, Program.exec_resolver["game_results_globals_extra"].address,
                0x1980);

            IEnumerable<byte> rv = mem.Concat(mem2);
            File.WriteAllBytes("dump.bin", rv.ToArray());

        }

        private void disable_rendering_check_box_CheckedChanged(object sender, EventArgs e)
        {
            Program.memory.WriteBool(Program.exec_resolver["disable_rendering"].address,
                !disable_rendering_check_box.Checked, false);
        }

        private void configuration_save_button_Click(object sender, EventArgs e)
        {
            if (configuration_combo_box.SelectedIndex == -1)
            {
                configuration config = Program.configurations.add(instance_name_text_box.Text);
                config.set("instance_name", instance_name_text_box.Text);
                config.set("xemu_path", xemu_path_text_box.Text);
                config.set("dedi_mode", profile_disabled_check_box.Checked.ToString());
                config.set("xemu_port", xemu_port_text_box.Text);
                config.set("websocket_bind", websocket_bind_text_box.Text);
                config.set("websocket_port", websocket_bind_port_text_box.Text);
                config.save();


                configuration_combo_box.Items.Clear();
                foreach (configuration configuration in Program.configurations.AsList)
                {
                    configuration_combo_box.Items.Add(configuration.name);
                }

                for (var i = 0; i < configuration_combo_box.Items.Count; i++)
                {
                    if (configuration_combo_box.Items[i].ToString() == instance_name_text_box.Text)
                    {
                        configuration_combo_box.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                configuration config = Program.configurations[(string) configuration_combo_box.SelectedItem];
                config.set("instance_name", instance_name_text_box.Text);
                config.set("xemu_path", xemu_path_text_box.Text);
                config.set("dedi_mode", profile_disabled_check_box.Checked.ToString());
                config.set("xemu_port", xemu_port_text_box.Text);
                config.set("websocket_bind", websocket_bind_text_box.Text);
                config.set("websocket_port", websocket_bind_port_text_box.Text);
                config.save();
            }
        }

        private void configuration_combo_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (configuration_combo_box.SelectedIndex != -1)
            {
                configuration config = Program.configurations[(string) configuration_combo_box.SelectedItem];

                instance_name_text_box.Text = config.get("instance_name", "");
                xemu_path_text_box.Text = config.get("xemu_path", "");
                profile_disabled_check_box.Checked = config.get("dedi_mode", "False") == "True";
                xemu_port_text_box.Text = config.get("xemu_port", "4444");
                websocket_bind_text_box.Text = config.get("websocket_bind", "127.0.0.1");
                websocket_bind_port_text_box.Text = config.get("websocket_port", "3333");
            }
        }

        private void new_configuration_button_Click(object sender, EventArgs e)
        {
            configuration_combo_box.SelectedIndex = -1;
            instance_name_text_box.Text = "";
            xemu_path_text_box.Text = "";
            profile_disabled_check_box.Checked = false;
            xemu_port_text_box.Text = "4444";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(xemu_proccess?.HasExited == false)
                xemu_proccess.Kill();
        }

        private void obs_connect_button_Click(object sender, EventArgs e)
        {

            if (!obs_communicator.connected)
            {
                obs_communicator.connect($"ws://{obs_host_text_box.Text}:{obs_port_text_box.Text}",
                    obs_password_text_box.Text);

                System.Threading.Thread.Sleep(200);

                obs_status_label.Text = obs_communicator.connected ? "Connected!" : "Failed to Connect!";

                obs_connect_button.Text = "Disconnect";

                for (var i = 0; i < obs_scene_link_table.Rows.Count; i++)
                {
                    ((DataGridViewComboBoxCell) obs_scene_link_table.Rows[i].Cells[0]).Items.Clear();
                    foreach (var sceneBasicInfo in obs_communicator.Scenes)
                    {
                        ((DataGridViewComboBoxCell) obs_scene_link_table.Rows[i].Cells[0]).Items.Add(
                            sceneBasicInfo.Name);
                    }
                }
            }
            else
            {
                obs_communicator.disconnect();

                obs_status_label.Text = "Disconnected";

                obs_connect_button.Text = "Connect";
            }
        }

        private void obs_scene_link_refresh_players_button_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < obs_scene_link_table.Rows.Count; i++)
            {
                ((DataGridViewComboBoxCell) obs_scene_link_table.Rows[i].Cells[1]).Items.Clear();
                for (var j = 0; j < 16; j++)
                {
                    string player_name =
                        Program.memory.ReadStringUnicode(Program.exec_resolver["lobby_players"].address + (j * 0x10c),
                            16, false);

                    if (!string.IsNullOrEmpty(player_name))
                        ((DataGridViewComboBoxCell) obs_scene_link_table.Rows[i].Cells[1]).Items.Add(player_name);

                }
            }
        }

        private void websocket_bind_link_label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://networkengineering.stackexchange.com/a/59838");
        }
    }

    public class offset_resolver_item
    {
        public string name;
        public long offset;
        public string value;
        public long address;

        public offset_resolver_item(string name, long offset, string value)
        {
            this.name = name;
            this.offset = offset;
            this.value = value;
        }
    }
}
