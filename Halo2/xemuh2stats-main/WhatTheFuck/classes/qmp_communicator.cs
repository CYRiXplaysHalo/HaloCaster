using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using JsonSerializer = System.Text.Json.JsonSerializer;

public class QMPError : Exception
{
    public QMPError(string message) : base(message) { }
}

public class QMPConnectError : QMPError
{
    public QMPConnectError() : base("QMP Connection Error") { }
}

public class QMPCapabilitiesError : QMPError
{
    public QMPCapabilitiesError() : base("QMP Capabilities Error") { }
}

public class QEMUMonitorProtocol
{
    private List<Dictionary<string, object>> _events;
    private string _address;
    private Socket _sock;
    private StreamReader _sockFile;
    private bool _isServerMode;

    public QEMUMonitorProtocol(string address, bool server = false)
    {
        _events = new List<Dictionary<string, object>>();
        _address = address;
        _sock = GetSocket();
        _isServerMode = server;

        if (_isServerMode)
        {
            _sock.Bind(new IPEndPoint(IPAddress.Parse(_address), 0));
            _sock.Listen(1);
        }
    }
    private Socket GetSocket()
    {
        if (_isServerMode)
        {
            // If server mode, assume TCP on IPv4 for simplicity.
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Use IPv4 for Windows
                return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // On Unix-like systems, check if it's a UNIX socket or TCP
                return _address.Contains('/') ?
                    new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified) :
                    new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            else
            {
                throw new NotSupportedException("Unsupported operating system.");
            }
        }
    }

    private Dictionary<string, object> NegotiateCapabilities()
    {
        _sockFile = new StreamReader(new NetworkStream(_sock));
        var greeting = JsonRead();

        if (greeting == null || !greeting.ContainsKey("QMP"))
        {
            throw new QMPConnectError();
        }

        var resp = Cmd("qmp_capabilities");
        if (resp != null && resp.ContainsKey("return"))
        {
            return greeting;
        }
        throw new QMPCapabilitiesError();
    }

    private Dictionary<string, object> JsonRead(bool onlyEvent = false)
    {
        while (true)
        {
            var data = _sockFile.ReadLine();
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            var resp = JsonSerializer.Deserialize<Dictionary<string, object>>(data);
            if (resp != null && resp.ContainsKey("event"))
            {
                _events.Add(resp);
                if (!onlyEvent)
                {
                    continue;
                }
            }
            return resp;
        }
    }

    public Dictionary<string, object> Connect(int port)
    {
        try
        {
            _sock.Connect(_address, port);
            return NegotiateCapabilities();
        }
        catch (SocketException e)
        {
            throw new QMPError($"Socket Error: {e.Message}");
        }
    }

    public Dictionary<string, object> Accept()
    {
        try
        {
            _sock = _sock.Accept();
            return NegotiateCapabilities();
        }
        catch (SocketException e)
        {
            throw new QMPError($"Socket Error: {e.Message}");
        }
    }

    public Dictionary<string, object> CmdObj(Dictionary<string, object> qmpCmd)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(qmpCmd);
            _sock.Send(System.Text.Encoding.UTF8.GetBytes(jsonString));
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode == SocketError.Shutdown)
            {
                return null;
            }
            throw new QMPError($"Socket Error: {e.Message}");
        }

        return JsonRead();
    }

    public Dictionary<string, object> Cmd(string name, Dictionary<string, object> args = null, object id = null)
    {
        var qmpCmd = new Dictionary<string, object> { { "execute", name } };
        if (args != null)
        {
            qmpCmd["arguments"] = args;
        }
        if (id != null)
        {
            qmpCmd["id"] = id;
        }
        return CmdObj(qmpCmd);
    }

    public List<Dictionary<string, object>> GetEvents(bool wait = false)
    {
        _sock.Blocking = false;
        try
        {
            JsonRead();
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode == SocketError.WouldBlock)
            {
                // No data available
            }
        }
        _sock.Blocking = true;

        if (_events.Count == 0 && wait)
        {
            JsonRead(true);
        }
        return _events;
    }

    public void ClearEvents()
    {
        _events.Clear();
    }

    public void Close()
    {
        _sock.Close();
        _sockFile?.Close();
    }
}

public class QmpProxy
{
    private static DateTime lastRequestTime = DateTime.Now;
    private static bool rateLimitEnabled = false;
    private static double requestRateSeconds = 0.005; // Minimum seconds between requests
    private int cmdCounter = 0;
    private DateTime cmdCounterReset = DateTime.Now;
    private static int pymemCounter = 0;

    private static Dictionary<ulong, Dictionary<string, object>> knownAddresses =
        new Dictionary<ulong, Dictionary<string, object>>();

    private static Dictionary<string, Func<ulong, object, object>> memoryFunctions =
        new Dictionary<string, Func<ulong, object, object>>();

    private static Dictionary<(ulong, ulong, ulong), byte[]> memoryCache =
        new Dictionary<(ulong, ulong, ulong), byte[]>();

    private QEMUMonitorProtocol _qmp;

    public QmpProxy(int port)
    {
        Connect(port);
    }

    private void Connect(int port)
    {
        int i = 0;
        while (true)
        {
            if (i > 0)
            {
                System.Threading.Thread.Sleep(1000);
            }

            try
            {
                _qmp = new QEMUMonitorProtocol("localhost", false);
                _qmp.Connect(port);
            }
            catch (Exception e)
            {
                if (i > 4)
                {
                    throw;
                }
                else
                {
                    i++;
                    continue;
                }
            }

            break;
        }
    }

    private Dictionary<string, object> RunCmd(object cmd)
    {
        DateTime now = DateTime.Now;
        double delta = (now - lastRequestTime).TotalSeconds;
        if (rateLimitEnabled && delta < requestRateSeconds)
        {
            Thread.Sleep((int) ((requestRateSeconds - delta) * 1000));
        }

        lastRequestTime = now;

        if (cmd is string)
        {
            cmd = new Dictionary<string, object>
            {
                {"execute", cmd},
                {"arguments", new Dictionary<string, object>()}
            };
        }

        cmdCounter++;
        if ((DateTime.Now - cmdCounterReset).TotalSeconds > 1.0)
        {
            cmdCounter = 0;
            cmdCounterReset = DateTime.Now;

            Console.WriteLine(JsonSerializer.Serialize(cmd));
            Debug.WriteLine(Environment.StackTrace);
        }

        var resp = _qmp.CmdObj((Dictionary<string, object>) cmd);
        if (resp == null)
        {
            throw new Exception("Disconnected!");
        }

        return resp;
    }

    public Dictionary<string, object> Pause()
    {
        return RunCmd("stop");
    }

    public Dictionary<string, object> Cont()
    {
        return RunCmd("cont");
    }

    public Dictionary<string, object> Restart()
    {
        return RunCmd("system_reset");
    }

    public Dictionary<string, object> Screenshot()
    {
        var cmd = new Dictionary<string, object>
        {
            {"execute", "screendump"},
            {"arguments", new Dictionary<string, object> {{"filename", "screenshot.ppm"}}}
        };
        return RunCmd(cmd);
    }

    public bool IsPaused()
    {
        var resp = RunCmd("query-status");
        return resp != null && resp.ContainsKey("return") &&
               ((JsonElement) resp["return"]).GetProperty("status").GetString() == "paused";
    }

    public byte[] Read(ulong addr, int size)
    {
        var cmd = new Dictionary<string, object>
        {
            {"execute", "human-monitor-command"},
            {"arguments", new Dictionary<string, object> {{"command-line", $"x /{size}xb {addr}"}}}
        };

        var response = RunCmd(cmd);
        var lines = response["return"].ToString().Replace("\r", "").Split('\n');
        var dataString = string.Join(" ",
            Array.ConvertAll(lines,
                l => l.Contains(": ") ? l.Split(new[] {": "}, StringSplitOptions.None)[1] : string.Empty)).Trim();
        byte[] data = Array.ConvertAll(dataString.Split(' '), b => Convert.ToByte(b, 16));
        return data;
    }

    public ulong Gpa2Hva(ulong addr)
    {
        var cmd = new Dictionary<string, object>
        {
            {"execute", "human-monitor-command"},
            {"arguments", new Dictionary<string, object> {{"command-line", $"gpa2hva {addr}"}}}
        };

        var response = RunCmd(cmd);
        var lines = response["return"].ToString().Replace("\r", "").Split('\n');
        var dataString = string.Join(" ",
            Array.ConvertAll(lines,
                l => l.Contains(" is ") ? l.Split(new[] {" is "}, StringSplitOptions.None)[1] : string.Empty)).Trim();
        return Convert.ToUInt64(dataString, 16);
    }

    public ulong Gpa2Hpa(ulong addr)
    {
        var cmd = new Dictionary<string, object>
        {
            {"execute", "human-monitor-command"},
            {"arguments", new Dictionary<string, object> {{"command-line", $"gpa2hpa {addr}"}}}
        };

        var response = RunCmd(cmd);
        // Parsing logic similar to Gpa2Hva can be added here.
        return 0;
    }

    public ulong Gva2Gpa(ulong addr)
    {
        var cmd = new Dictionary<string, object>
        {
            {"execute", "human-monitor-command"},
            {"arguments", new Dictionary<string, object> {{"command-line", $"gva2gpa {addr}"}}}
        };

        var response = RunCmd(cmd);
        var lines = response["return"].ToString().Replace("\r", "").Split('\n');
        var dataString = string.Join(" ",
            Array.ConvertAll(lines,
                l => l.Contains("gpa: ") ? l.Split(new[] {"gpa: "}, StringSplitOptions.None)[1].Replace("0x","") : string.Empty)).Trim();

        if (ulong.TryParse(dataString, System.Globalization.NumberStyles.HexNumber, null, out ulong result))
        {
            return result;
        }
        else
        {
            Console.WriteLine($"Error converting GVA {addr} to GPA (got {response})");
            throw new Exception($"Error converting GVA {addr} to GPA.");
        }
    }

    public ulong Gva2Hva(ulong addr)
    {
        return Gpa2Hva(Gva2Gpa(addr));
    }

    public ulong Translate(ulong addr)
    {
        return Gva2Hva(addr);
    }

    public ulong GetHostAddressFromCache(ulong address)
    {
        foreach (var entry in memoryCache)
        {
            var (start, end, hostAddress) = entry.Key;
            if (start <= address && address <= end)
            {
                ulong offset = address - start;
                return GetHostAddress(start) + offset;
            }
        }

        return ulong.MaxValue; // Represents -1 in Python
    }

    public ulong GetHostAddress(ulong address)
    {
        if (knownAddresses.ContainsKey(address))
        {
            return (ulong) knownAddresses[address]["host_address"];
        }
        else
        {
            ulong hostAddress = GetHostAddressFromCache(address);
            if (hostAddress != ulong.MaxValue) // Check if found in cache
            {
                if (!knownAddresses.ContainsKey(address))
                    knownAddresses[address] = new Dictionary<string, object>();

                knownAddresses[address]["host_address"] = hostAddress;
                return hostAddress;
            }
            else
            {
                hostAddress = Translate(address);
                if (!knownAddresses.ContainsKey(address))
                    knownAddresses[address] = new Dictionary<string, object>();

                knownAddresses[address]["host_address"] = hostAddress;
                return hostAddress;
            }
        }
    }

    public object ReadMemory(ulong address, string fn, bool retryOnValueChange = false, bool isHostAddress = false,
        bool keepValue = true, bool watch = false, bool returnHostAddress = false, bool assumeContiguousRam = true,
        Dictionary<string, object> kwargs = null)
    {
        object value = null;

        // Read from host memory directly if it's a host address
        if (isHostAddress)
        {
            value = memoryFunctions[fn](address, kwargs);
            pymemCounter++;
        }
        // Read from cached memory if the address is in the cached segment
        else if (TryReadFromCache(address, fn, out var cachedValue, kwargs))
        {
            value = cachedValue["value"];
            if (!knownAddresses.ContainsKey(address))
                knownAddresses[address] = new Dictionary<string, object>();

            knownAddresses[address]["value"] = keepValue ? value : 0;
            knownAddresses[address]["host_address"] = cachedValue["host_address"];
            knownAddresses[address]["type"] = fn;
        }
        // Read using known host address if already translated
        else if (knownAddresses.ContainsKey(address))
        {
            var hostAddress = (ulong) knownAddresses[address]["host_address"];
            value = memoryFunctions[fn](hostAddress, kwargs);
            pymemCounter++;

            // Retry on value change if required
            if (retryOnValueChange && !value.Equals(knownAddresses[address]["value"]))
            {
                knownAddresses[address]["host_address"] = Gva2Hva(address); // Translate address again

                // Re-read the value using the new host address
                value = memoryFunctions[fn]((ulong) knownAddresses[address]["host_address"], kwargs);
                pymemCounter++;
            }

            knownAddresses[address]["value"] = keepValue ? value : 0;
            knownAddresses[address]["type"] = fn;
        }
        // Assume contiguous RAM layout for specific addresses (e.g., Xbox memory model)
        else if (assumeContiguousRam && address > 0x80000000)
        {
            ulong baseAddress = GetHostAddress(0x80000000);
            ulong offset = address - 0x80000000;
            ulong hostAddress = baseAddress + offset;

            if (!knownAddresses.ContainsKey(address))
                knownAddresses[address] = new Dictionary<string, object>();

            knownAddresses[address]["host_address"] = hostAddress;
            value = memoryFunctions[fn](hostAddress, kwargs);
            pymemCounter++;
            knownAddresses[address]["value"] = keepValue ? value : 0;
            knownAddresses[address]["type"] = fn;
        }
        // Translate guest address to host address if not seen before
        else
        {
            if (!knownAddresses.ContainsKey(address))
                knownAddresses[address] = new Dictionary<string, object>();

            knownAddresses[address]["host_address"] = Gva2Hva(address);
            value = memoryFunctions[fn]((ulong) knownAddresses[address]["host_address"], kwargs);
            pymemCounter++;
            knownAddresses[address]["value"] = keepValue ? value : 0;
            knownAddresses[address]["type"] = fn;
            knownAddresses[address]["qmp"] = true;
            knownAddresses[address]["qmp_traceback"] =
                new StackTrace().GetFrame(1).GetMethod().ToString(); // Debugging purposes
        }

        return value;
    }

    private bool TryReadFromCache(ulong address, string fn, out Dictionary<string, object> cachedValue,
        Dictionary<string, object> kwargs)
    {
        // Placeholder function to simulate reading from cache
        cachedValue = new Dictionary<string, object>(); // Should return the actual cached value
        return false;

    }
}
