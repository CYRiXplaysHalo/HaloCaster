using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Linq;
using xemuh2stats;

namespace WhatTheFuck.classes
{
    public class configuration_collection : System.Collections.CollectionBase, IEnumerable<configuration>
    {
        private static string root_path = $"{AppDomain.CurrentDomain.BaseDirectory}/config/";

        public configuration_collection()
        {
            if(!Directory.Exists(root_path))
                Directory.CreateDirectory(root_path);

            load();
        }

        public void load()
        {
            List.Clear();
            foreach (var file in Directory.GetFiles(root_path))
            {
                if (Path.GetExtension(file) == ".json")
                {
                    configuration config = new configuration(file, Path.GetFileName(file).Replace(".json", ""));
                    List.Add(config);
                }
            }
        }

        public configuration this[string name] => 
            List.Cast<configuration>().FirstOrDefault(config => config.name == name);

        public configuration add(string name)
        {
            List.Add(new configuration($"{root_path}{name}.json", name));
            return (configuration) List[List.Count - 1];
        }

        public List<configuration> AsList =>
            List.Cast<configuration>().ToList();

        public IEnumerator<configuration> GetEnumerator()
        {
            foreach (configuration config in List)
            {
                yield return config;
            }
        }
    }

    public class configuration : System.Collections.CollectionBase, IEnumerable<KeyValuePair<string, string>>
    {
        public bool is_valid = false;
        public string name;

        private FileStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private string _file_path;
        

        public configuration(string file_path, string name)
        {
            
            _file_path = file_path;
            this.name = name;

            if (File.Exists(file_path))
            {
                load();
            }

            is_valid = true;
        }

        public string get(string key, string default_return)
        {
            foreach (KeyValuePair<string, string> keyValuePair in List)
                if (keyValuePair.Key == key)
                {
                    return keyValuePair.Value;
                }

            return default_return;
        }

        public void set(string key, string value)
        {
            if (AsList.Count(x => x.Key == key) == 0)
                List.Add(new KeyValuePair<string, string>(key, value));
            else
            {
                for (var index = 0; index < List.Count; index++)
                {
                    var keyValuePair = (KeyValuePair<string, string>)List[index];
                    if (keyValuePair.Key == key)
                    {
                        this.List.RemoveAt(index);
                        List.Add(new KeyValuePair<string, string>(key, value));
                    }
                }
            }
        }

        public void load()
        {
            _stream = new FileStream(_file_path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            _reader = new StreamReader(_stream);

            var settings = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(_reader.ReadToEnd());
            if (settings != null)
            {
                try
                {
                    settings.ForEach(pair => List.Add(pair));
                }
                catch (Exception)
                {
                }
            }

            _reader.Close();
            _reader.Dispose();
        }

        public void save()
        {
            _stream = new FileStream(_file_path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            _writer = new StreamWriter(_stream);

            _stream.SetLength(0);

            var settings = List.Cast<KeyValuePair<string, string>>().ToList();

            _writer.Write(JsonConvert.SerializeObject(settings));

            _writer.Flush();
            _writer.Close();
            _writer.Dispose();
        }

        private List<KeyValuePair<string, string>> AsList =>
            List.Cast<KeyValuePair<string, string>>().ToList();
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (KeyValuePair<string, string> keyValuePair in List)
            {
                yield return keyValuePair;
            }
        }
    }
}
