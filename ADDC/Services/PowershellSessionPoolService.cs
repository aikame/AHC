using ADDC.Interfaces;
using System;
using System.Collections.Concurrent;
using System.DirectoryServices.ActiveDirectory;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
namespace ADDC.Services
{
    public class PowershellSessionPoolService : IPowershellSessionPoolService
    {
        private readonly int _maxSessions;
        private readonly ConcurrentBag<PowerShell> _availableSessions = new();
        private readonly ConcurrentBag<PowerShell> _temporarySessions = new();
        private readonly ILogger<PowershellSessionPoolService> _logger;
        private readonly Timer _cleanupTimer;
        private readonly object _lock = new();
        private readonly string _scriptPath = "./PowershellFunctions/Scripts.ps1";

        public PowershellSessionPoolService(ILogger<PowershellSessionPoolService> logger,int maxSessions = 5, int cleanupIntervalMs = 60000) {
            _maxSessions = maxSessions;
            _logger = logger;
            for (int i = 0; i < maxSessions; i++)
            {
                _availableSessions.Add(CreateSession());
            }

            _cleanupTimer = new Timer(CleanupTemporarySessions, null, cleanupIntervalMs, cleanupIntervalMs);
        }
        private PowerShell CreateSession()
        {
            var ps = PowerShell.Create();
            string scriptText = System.IO.File.ReadAllText(_scriptPath);
            ps.AddScript(scriptText).Invoke();
            return ps;
        }

        public PowerShell GetSession()
        {
            if (_availableSessions.TryTake(out var session))
            {
                return session;
            }

            lock (_lock)
            {
                if (_temporarySessions.Count < _maxSessions)
                {
                    var tempSession = CreateSession();
                    _temporarySessions.Add(tempSession);
                    return tempSession;
                }
            }

            throw new Exception("Нет доступных сессий и нельзя создать новую.");
        }

        public void ReleaseSession(PowerShell session)
        {
            if (_temporarySessions.Contains(session))
            {
                _temporarySessions.TryTake(out _);
                session.Dispose();
            }
            else
            {
                _availableSessions.Add(session);
            }
        }

        private void CleanupTemporarySessions(object? state)
        {
            lock (_lock)
            {
                while (_temporarySessions.TryTake(out var session))
                {
                    session.Dispose();
                }
            }
        }

        public async Task<string> ExecuteFunction(string functionName, params (string, object)[]? parameters)
        {
            var ps = GetSession();
            try
            {
                ps.Commands.Clear();
                ps.AddCommand(functionName);

                foreach (var (name, value) in parameters)
                {
                    ps.AddParameter(name, value);
                }

                var results = await ps.InvokeAsync();

                string final = "";
                foreach (var errorRecord in ps.Streams.Error)
                {
                    _logger.LogError("Error: " + errorRecord.Exception.Message);
                }
                foreach (var result in results)
                {
                    final += result.ToString();
                }

                return final;
            }
            finally
            {
                ReleaseSession(ps);
            }
        }
    }
}
