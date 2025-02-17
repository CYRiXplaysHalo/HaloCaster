using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.ExtendedProperties;
using xemuh2stats.enums;
using System.Reflection;
using xemuh2stats.objects;

namespace xemuh2stats.classes
{
    public static class xls_generator
    {
        public static void dump_game_to_sheet(string filename, List<real_time_player_stats> real_time_player_stats, List<s_post_game_player> post_game_player_stats, s_variant_details variant_details)
        {
            if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/stats/"))
                Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}/stats/");

            string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}/stats/{filename}.xlsx";
            // Create a spreadsheet document.
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                // Add a WorkbookPart to the document.
                WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // Add Sheets to the Workbook.
                Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

                // Add some data to the worksheet.
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                CreateDetailsSheet(workbookPart, sheets, variant_details);
                CreatePostStatsSheet(workbookPart, sheets, post_game_player_stats);
                CreateVersusSheet(workbookPart, sheets, post_game_player_stats);
                CreateGameStatsSheet(workbookPart, sheets, real_time_player_stats);
                CreateMedalStatsSheet(workbookPart, sheets, real_time_player_stats);
                CreateWeaponStatsSheet(workbookPart, sheets, real_time_player_stats);

                // Save the workbook.
                workbookPart.Workbook.Save();
            }
        }
        static SheetData CreateSheet(WorkbookPart workbookPart, Sheets sheets, string sheetName, string[] headers)
        {
            // Add a WorksheetPart to the WorkbookPart
            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Append a new sheet and associate it with the workbook
            Sheet sheet = new Sheet()
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = (uint)(sheets.Count() + 1),
                Name = sheetName
            };
            sheets.Append(sheet);

            // Add header row (if headers are provided)
            if (headers != null && headers.Length > 0)
            {
                AddHeaderRow(worksheetPart.Worksheet, headers);
            }

            // Return the SheetData of the created sheet
            return worksheetPart.Worksheet.GetFirstChild<SheetData>();
        }

        private static readonly List<string> details_column_headers = new List<string>()
        {
            "Game Type",
            "Variant Name",
            "Map Name",
            "Start Time",
            "End Time",
            "Duration"
        };

        private static void CreateDetailsSheet(WorkbookPart workbookPart, Sheets sheets, s_variant_details variant_details)
        {
            var data = CreateSheet(workbookPart, sheets, "Game Details", details_column_headers.ToArray());

            Row row = new Row();

            Cell cell = new Cell()
            {
                CellValue = new CellValue(variant_details.game_type.ToString()),
                DataType = new EnumValue<CellValues>(CellValues.String)
            };
            row.Append(cell);
            cell = new Cell()
            {
                CellValue = new CellValue(variant_details.name),
                DataType = new EnumValue<CellValues>(CellValues.String)
            };
            row.Append(cell);
            cell = new Cell()
            {
                CellValue = new CellValue(variant_details.map_name),
                DataType = new EnumValue<CellValues>(CellValues.String)
            };
            row.Append(cell);
            cell = new Cell()
            {
                CellValue = new CellValue($"{variant_details.start_time.ToShortDateString()} {variant_details.start_time.ToShortTimeString()}"),
                DataType = new EnumValue<CellValues>(CellValues.String)
            };
            row.Append(cell);
            cell = new Cell()
            {
                CellValue = new CellValue($"{variant_details.end_time.ToShortDateString()} {variant_details.end_time.ToShortTimeString()}"),
                DataType = new EnumValue<CellValues>(CellValues.String)
            };
            row.Append(cell);
            string t = variant_details.duration.ToString(@"mm\:ss");
            cell = new Cell()
            {
                CellValue = new CellValue(t),
                DataType = new EnumValue<CellValues>(CellValues.String)
            };
            row.Append(cell);
            data.Append(row);
        }

        private static readonly List<string> post_column_headers = new List<string>()
        {
            "name",
            "place",
            "score",
            "kills",
            "deaths",
            "assists",
            "kda",
            "suicides",
            "team",
            "shots_fired",
            "shots_hit",
            "accuracy",
            "head_shots"
        };

        private static void CreatePostStatsSheet(WorkbookPart workbookPart, Sheets sheets, List<s_post_game_player> post_stats)
        {
            var data = CreateSheet(workbookPart, sheets, "Post Game Report", post_column_headers.ToArray());

            foreach (var player in post_stats)
            {
                Row playerRow = new Row();
                Cell cell = new Cell()
                {
                    CellValue = new CellValue(player.name),
                    DataType = new EnumValue<CellValues>(CellValues.String)
                };
                playerRow.Append(cell);
                cell = new Cell()
                {
                    CellValue = new CellValue(player.place),
                    DataType = new EnumValue<CellValues>(CellValues.String)
                };
                playerRow.Append(cell);
                cell = new Cell()
                {
                    CellValue = new CellValue(player.score),
                    DataType = new EnumValue<CellValues>(CellValues.String)
                };
                playerRow.Append(cell);
                AddNumberToRow(playerRow, player.kills);
                AddNumberToRow(playerRow, player.deaths);
                AddNumberToRow(playerRow, player.assists);

                if (player.deaths > 0 && (player.kills > 0 || player.assists > 0))
                {
                    var kda = (double) player.kills + player.assists;
                    kda = kda / player.deaths;
                    kda = Math.Round(kda, 3);
                    cell = new Cell()
                    {
                        CellValue = new CellValue(kda),
                        DataType = new EnumValue<CellValues>(CellValues.Number)
                    };
                    playerRow.Append(cell);
                }
                else
                {
                    AddNumberToRow(playerRow, player.kills + player.deaths);
                }

                AddNumberToRow(playerRow, player.suicides);
                cell = new Cell()
                {
                    CellValue = new CellValue(player.team.ToString()),
                    DataType = new EnumValue<CellValues>(CellValues.String)
                };
                playerRow.Append(cell);
                AddNumberToRow(playerRow, player.shots_fired);
                AddNumberToRow(playerRow, player.shots_hit);

                if (player.shots_fired != 0 && player.shots_hit != 0)
                {
                    double accuracy = (double)player.shots_hit / player.shots_fired;
                    accuracy = Math.Round(accuracy * 100, 2);
                    cell = new Cell()
                    {
                        CellValue = new CellValue(accuracy),
                        DataType = new EnumValue<CellValues>(CellValues.Number)
                    };
                    playerRow.Append(cell);
                }
                else
                {
                    AddNumberToRow(playerRow, 0);
                }

                AddNumberToRow(playerRow, player.head_shots);
                data.Append(playerRow);
            }
        }

        public static void CreateVersusSheet(WorkbookPart workbookPart, Sheets sheets, List<s_post_game_player> post_stats)
        {
            List<string> header = new List<string>()
            {
                "X"
            };
            header.AddRange(post_stats.Select(player => player.name));

            var data = CreateSheet(workbookPart, sheets, "Versus", header.ToArray());

            for (int i = 0; i < post_stats.Count; i++)
            {
                var player = post_stats[i];
                Row playerRow = new Row();
                Cell cell = new Cell()
                {
                    CellValue = new CellValue(player.name),
                    DataType = new EnumValue<CellValues>(CellValues.String)
                };
                playerRow.Append(cell);
                for (int j = 0; j < post_stats.Count; j++)
                {
                    FieldInfo field = typeof(s_post_game_player).GetField($"killed_player_{j + 1}");
                    if (field != null)
                    {
                        int value = (int)field.GetValue(player);
                        AddNumberToRow(playerRow, value);
                    }
                }

                data.Append(playerRow);
            }
        }

        private static readonly List<string> game_column_headers = new List<string>()
        {
            "Player",
            "kills",
            "assists",
            "deaths",
            "betrayals",
            "suicides",
            "best_spree",
            "total_time_alive",
            "ctf_scores",
            "ctf_flag_steals",
            "ctf_flag_saves",
            "ctf_unkown",
            "assault_score",
            "assault_bomber_kills",
            "assault_bomb_grabbed",
            "oddball_score",
            "oddball_ball_kills",
            "oddball_carried_kills",
            "koth_kills_as_king",
            "koth_kings_killed",
            "juggernauts_killed",
            "kills_as_juggernaut",
            "juggernaut_time",
            "territories_taken",
            "territories_lost"
        };
        private static void CreateGameStatsSheet(WorkbookPart workbookPart, Sheets sheets,
            List<real_time_player_stats> real_time_player_stats)
        {
            var data = CreateSheet(workbookPart, sheets, "Game Statistics", game_column_headers.ToArray());

            foreach (var player in real_time_player_stats)
            {
                Row playerRow = new Row();
                Cell cell = new Cell()
                {
                    CellValue = new CellValue(player.GetPlayerName()),
                    DataType = new EnumValue<CellValues>(CellValues.String)
                };
                playerRow.Append(cell);
                AddNumberToRow(playerRow, player.game_stats.kills);
                AddNumberToRow(playerRow, player.game_stats.assists);
                AddNumberToRow(playerRow, player.game_stats.deaths);
                AddNumberToRow(playerRow, player.game_stats.betrayals);
                AddNumberToRow(playerRow, player.game_stats.suicides);
                AddNumberToRow(playerRow, player.game_stats.best_spree);
                AddNumberToRow(playerRow, player.game_stats.total_tile_alive);
                AddNumberToRow(playerRow, player.game_stats.ctf_scores);
                AddNumberToRow(playerRow, player.game_stats.ctf_flag_steals);
                AddNumberToRow(playerRow, player.game_stats.ctf_flag_saves);
                AddNumberToRow(playerRow, player.game_stats.ctf_unkown);
                AddNumberToRow(playerRow, player.game_stats.assault_score);
                AddNumberToRow(playerRow, player.game_stats.assault_bomber_kills);
                AddNumberToRow(playerRow, player.game_stats.assault_bomb_grabbed);
                AddNumberToRow(playerRow, (int)player.game_stats.oddball_score);
                AddNumberToRow(playerRow, player.game_stats.oddball_ball_kills);
                AddNumberToRow(playerRow, player.game_stats.oddball_carried_kills);
                AddNumberToRow(playerRow, player.game_stats.koth_kills_as_king);
                AddNumberToRow(playerRow, player.game_stats.koth_kings_killed);
                AddNumberToRow(playerRow, player.game_stats.juggernauts_killed);
                AddNumberToRow(playerRow, player.game_stats.kills_as_juggernaut);
                AddNumberToRow(playerRow, player.game_stats.juggernaut_time);
                AddNumberToRow(playerRow, player.game_stats.territories_taken);
                AddNumberToRow(playerRow, player.game_stats.territories_lost);
                data.Append(playerRow);
            }
        }

        private static readonly List<string> medal_column_headers = new List<string>()
        {
            "player",
            "double_kill",
            "triple_kill",
            "killtacular",
            "kill_frenzy",
            "killtrocity",
            "killamanjaro",
            "sniper_kill",
            "road_kill",
            "bone_cracker",
            "assassin",
            "vehicle_destroyed",
            "car_jacking",
            "stick_it",
            "killing_spree",
            "running_riot",
            "rampage",
            "beserker",
            "over_kill",
            "flag_taken",
            "flag_carrier_kill",
            "flag_returned",
            "bomb_planted",
            "bomb_carrier_kill",
            "bomb_returned",
        };
        private static void CreateMedalStatsSheet(WorkbookPart workbookPart, Sheets sheets, List<real_time_player_stats> real_time_player_stats)
        {


            var data = CreateSheet(workbookPart, sheets, "Medal Stats", medal_column_headers.ToArray());

            foreach (var player in real_time_player_stats)
            {
                Row playerRow = new Row();

                Cell cell = new Cell()
                {
                    CellValue = new CellValue(player.GetPlayerName()),
                    DataType = new EnumValue<CellValues>(CellValues.String)
                };
                playerRow.Append(cell);

                AddNumberToRow(playerRow, player.medal_stats.double_kill);
                AddNumberToRow(playerRow, player.medal_stats.triple_kill);
                AddNumberToRow(playerRow, player.medal_stats.killtacular);
                AddNumberToRow(playerRow, player.medal_stats.kill_frenzy);
                AddNumberToRow(playerRow, player.medal_stats.killtrocity);
                AddNumberToRow(playerRow, player.medal_stats.killamanjaro);
                AddNumberToRow(playerRow, player.medal_stats.sniper_kill);
                AddNumberToRow(playerRow, player.medal_stats.road_kill);
                AddNumberToRow(playerRow, player.medal_stats.bone_cracker);
                AddNumberToRow(playerRow, player.medal_stats.assassin);
                AddNumberToRow(playerRow, player.medal_stats.vehicle_destroyed);
                AddNumberToRow(playerRow, player.medal_stats.car_jacking);
                AddNumberToRow(playerRow, player.medal_stats.stick_it);
                AddNumberToRow(playerRow, player.medal_stats.killing_spree);
                AddNumberToRow(playerRow, player.medal_stats.running_riot);
                AddNumberToRow(playerRow, player.medal_stats.rampage);
                AddNumberToRow(playerRow, player.medal_stats.beserker);
                AddNumberToRow(playerRow, player.medal_stats.over_kill);
                AddNumberToRow(playerRow, player.medal_stats.flag_taken);
                AddNumberToRow(playerRow, player.medal_stats.flag_carrier_kill);
                AddNumberToRow(playerRow, player.medal_stats.flag_returned);
                AddNumberToRow(playerRow, player.medal_stats.bomb_planted);
                AddNumberToRow(playerRow, player.medal_stats.bomb_carrier_kill);
                AddNumberToRow(playerRow, player.medal_stats.bomb_returned);
                data.Append(playerRow);
            }
        }

        private static void CreateWeaponStatsSheet(WorkbookPart workbookPart, Sheets sheets, List<real_time_player_stats> real_time_player_stats)
        {
            // create headers
            List<string> column_headers = new List<string>();
            column_headers.Add("Player");
            foreach (var weapon in weapon_stat.weapon_list)
            {
                column_headers.Add($"{weapon} kills");
                column_headers.Add($"{weapon} headshot kills");
                column_headers.Add($"{weapon} deaths");
                column_headers.Add($"{weapon} suicide");
                column_headers.Add($"{weapon} shots fired");
                column_headers.Add($"{weapon} shots hit");
            }

            var data = CreateSheet(workbookPart, sheets, "Weapon Statistics", column_headers.ToArray());

            foreach (var player in real_time_player_stats)
            {
                Row playerRow = new Row();

                Cell cell = new Cell()
                {
                    CellValue = new CellValue(player.GetPlayerName()),
                    DataType = new EnumValue<CellValues>(CellValues.String)
                };
                playerRow.Append(cell);
                foreach (var weapon in weapon_stat.weapon_list)
                {
                    var stat = player.weapon_stats[weapon];
                    AddNumberToRow(playerRow, stat.kills);
                    AddNumberToRow(playerRow, stat.head_shots);
                    AddNumberToRow(playerRow, stat.deaths);
                    AddNumberToRow(playerRow, stat.suicide);
                    AddNumberToRow(playerRow, stat.shots_fired);
                    AddNumberToRow(playerRow, stat.shots_hit);
                }
                data.Append(playerRow);
            }
        }

        public static void GenerateFromTemplate(WorkbookPart workbookPart, Sheets sheets, spread_sheet_template template, List<real_time_player_stats> real_time_player_stats, List<s_post_game_player> post_game_players)
        {
            if (template.isValid())
            {
                var data = CreateSheet(workbookPart, sheets, template.name, template.headers.ToArray());
                for (int i = 0; i < real_time_player_stats.Count; i++)
                {
                    var real_time = real_time_player_stats[i];
                    var post_time = post_game_players[i];

                    Row playerRow = new Row();
                    for (var j = 0; j < template.column_count; j++)
                    {
                        var format = template.cells[j];
                        var type = template.types[j];
                        Cell cell = new Cell()
                        {
                            CellValue = new CellValue(object_formatter.Format(format, real_time.weapon_stats, real_time.game_stats, real_time.medal_stats, post_time)),
                            DataType = new EnumValue<CellValues>(type)
                        };
                        playerRow.Append(cell);
                    }

                    data.Append(playerRow);
                }
            }
        }

        private static void AddHeaderRow(Worksheet worksheet, string[] headers)
        {
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            Row headerRow = new Row();

            foreach (string header in headers)
            {
                Cell cell = new Cell()
                {
                    CellValue = new CellValue(header),
                    DataType = new EnumValue<CellValues>(CellValues.String)
                };
                headerRow.Append(cell);
            }

            sheetData.Append(headerRow);
        }

        private static void AddNumberToRow(Row row, int value)
        {
            Cell cell = new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(CellValues.Number)
            };
            row.Append(cell);
        }
    }
}
