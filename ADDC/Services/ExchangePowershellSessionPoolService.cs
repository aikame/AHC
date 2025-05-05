using System;
using System.Collections.Concurrent;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using ADDC.Interfaces;
using Microsoft.Extensions.Logging; 

namespace ADDC.Services 
{
    public class ExchangePowershellSessionPoolService : IExchangePowershellSessionPoolService, IDisposable
    {
        private readonly int _maxSessions;
        private readonly ConcurrentBag<PowerShell> _availableSessions = new();
        private readonly ConcurrentBag<PowerShell> _temporarySessions = new();
        private readonly ILogger<ExchangePowershellSessionPoolService> _logger;
        private readonly Timer _cleanupTimer;
        private readonly object _lock = new();
        private readonly string _exchangeScriptPath; 
        private readonly string _exchangeSnapIn = "Microsoft.Exchange.Management.PowerShell.SnapIn"; 


        private readonly ConcurrentBag<Runspace> _allRunspaces = new();

        public ExchangePowershellSessionPoolService(
            ILogger<ExchangePowershellSessionPoolService> logger,
            string exchangeScriptPath = "./PowershellFunctions/ScriptsPS5.ps1", 
            int maxSessions = 3, 
            int cleanupIntervalMs = 60000)
        {
            _maxSessions = maxSessions;
            _logger = logger;
            _exchangeScriptPath = exchangeScriptPath;

            _logger.LogInformation("Initializing Exchange PowerShell Session Pool with MaxSessions={MaxSessions}", maxSessions);

            for (int i = 0; i < maxSessions; i++)
            {
                try
                {
                    var session = CreateExchangeSession();
                    if (session != null)
                    {
                        _availableSessions.Add(session);
                        _logger.LogTrace("Successfully created and added initial Exchange PowerShell session {Index}/{Max}", i + 1, maxSessions);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create initial Exchange PowerShell session {Index}/{Max}", i + 1, maxSessions);
                        
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create initial Exchange PowerShell session {Index}/{Max}", i + 1, maxSessions);
                }
            }
            _logger.LogInformation("Exchange PowerShell Session Pool Initialization complete. Available sessions: {Count}", _availableSessions.Count);

            _cleanupTimer = new Timer(CleanupTemporarySessions, null, cleanupIntervalMs, cleanupIntervalMs);
        }

        private PowerShell? CreateExchangeSession()
        {
            _logger.LogTrace("Attempting to create a new Exchange PowerShell session...");
            Runspace? runspace = null;
            PowerShell? ps = null;
            try
            {

                var connectionInfo = new WSManConnectionInfo(); 

                runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                runspace.Open();
                _allRunspaces.Add(runspace); 
                _logger.LogTrace("Runspace opened successfully. State: {State}", runspace.RunspaceStateInfo.State);

                ps = PowerShell.Create();
                ps.Runspace = runspace;
                _logger.LogTrace("Loading Exchange SnapIn: {SnapInName}", _exchangeSnapIn);
                ps.AddCommand("Add-PSSnapin").AddParameter("Name", _exchangeSnapIn).AddParameter("ErrorAction", "Stop");
                ps.Invoke();

                if (ps.Streams.Error.Any())
                {
                    var errors = ps.Streams.Error.Select(e => e.ToString()).ToList();
                    _logger.LogError("Errors loading Exchange SnapIn: {Errors}", string.Join(Environment.NewLine, errors));
                    ps.Streams.Error.Clear();
                    throw new InvalidOperationException($"Failed to load Exchange SnapIn '{_exchangeSnapIn}'. Errors: {string.Join("; ", errors)}");
                }
                _logger.LogTrace("Exchange SnapIn loaded successfully.");
                ps.Commands.Clear(); 

                if (!string.IsNullOrEmpty(_exchangeScriptPath) && File.Exists(_exchangeScriptPath))
                {
                    _logger.LogTrace("Pre-loading Exchange script: {ScriptPath}", _exchangeScriptPath);
                    string scriptText = File.ReadAllText(_exchangeScriptPath);
                    ps.AddScript(scriptText).AddParameter("ErrorAction", "Stop");
                    ps.Invoke();

                    if (ps.Streams.Error.Any())
                    {
                        var errors = ps.Streams.Error.Select(e => e.ToString()).ToList();
                        _logger.LogError("Errors pre-loading Exchange script '{ScriptPath}': {Errors}", _exchangeScriptPath, string.Join(Environment.NewLine, errors));
                        ps.Streams.Error.Clear();
                      
                    }
                    _logger.LogTrace("Exchange script pre-loaded successfully.");
                    ps.Commands.Clear(); 
                }
                else if (!string.IsNullOrEmpty(_exchangeScriptPath))
                {
                    _logger.LogWarning("Exchange script path specified but file not found: {ScriptPath}", _exchangeScriptPath);
                }

                _logger.LogTrace("Exchange PowerShell session created successfully.");
                return ps;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Exchange PowerShell session.");
                ps?.Dispose();
                if (runspace != null)
                {
                    if (_allRunspaces.TryTake(out var removedRunspace) && removedRunspace == runspace)
                    {
                        
                    }
                    runspace.Dispose();
                }
                return null; 
            }
        }

        public PowerShell GetSession()
        {
            _logger.LogTrace("Attempting to get an Exchange PowerShell session...");
            if (_availableSessions.TryTake(out var session))
            {
                _logger.LogTrace("Session taken from available pool. Remaining: {Count}", _availableSessions.Count);
                return session;
            }
            _logger.LogTrace("No available sessions in the pool. Checking temporary capacity.");

            lock (_lock)
            {

                if (_temporarySessions.Count < _maxSessions)
                {
                    _logger.LogTrace("Creating a temporary Exchange PowerShell session.");
                    var tempSession = CreateExchangeSession();
                    if (tempSession != null)
                    {
                        _temporarySessions.Add(tempSession);
                        _logger.LogTrace("Temporary session created and added. Temporary count: {Count}", _temporarySessions.Count);
                        return tempSession;
                    }
                    else
                    {
                        _logger.LogError("Failed to create a temporary Exchange session.");
                      
                    }

                }
                else
                {
                    _logger.LogWarning("Cannot create temporary session. Limit reached ({Limit}).", _maxSessions);
                }
            }

            _logger.LogError("Failed to get an Exchange PowerShell session. Pool exhausted and cannot create temporary session.");
            throw new Exception("Нет доступных сессий Exchange PowerShell и нельзя создать новую.");
        }

        public void ReleaseSession(PowerShell session)
        {
            _logger.LogTrace("Releasing Exchange PowerShell session...");
            session.Commands.Clear();
            if (session.Streams.Error.Any())
            {
                _logger.LogWarning("Clearing {Count} errors from session before release.", session.Streams.Error.Count);
                session.Streams.Error.Clear();
            }
            
            session.Streams.Information.Clear();
            session.Streams.Verbose.Clear();
            session.Streams.Warning.Clear();
            session.Streams.Debug.Clear();


            if (_temporarySessions.TryTake(out var tempSession) && tempSession == session)
            {
                _logger.LogTrace("Disposing temporary Exchange PowerShell session.");

                if (session.Runspace != null && _allRunspaces.TryTake(out var runspaceToDispose) && runspaceToDispose == session.Runspace)
                {
                    runspaceToDispose.Dispose();
                }
                else if (session.Runspace != null)
                {
                    
                    session.Runspace.Dispose();
                }
                session.Dispose(); 
            }
            else
            {

                _logger.LogTrace("Returning session to available pool. Available count will be: {Count}", _availableSessions.Count + 1);
                _availableSessions.Add(session);
            }
        }

        private void CleanupTemporarySessions(object? state)
        {
            _logger.LogTrace("Running cleanup task for temporary Exchange sessions...");
            int cleanedCount = 0;
            lock (_lock) 
            {
                while (_temporarySessions.TryTake(out var session))
                {
                    _logger.LogTrace("Cleaning up temporary session.");
                    try
                    {
                        if (session.Runspace != null && _allRunspaces.TryTake(out var runspaceToDispose) && runspaceToDispose == session.Runspace)
                        {
                            runspaceToDispose.Dispose();
                        }
                        else if (session.Runspace != null)
                        {
                            session.Runspace.Dispose();
                        }
                        session.Dispose();
                        cleanedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error disposing temporary Exchange session during cleanup.");
                    }
                }
            }
            if (cleanedCount > 0)
            {
                _logger.LogInformation("Cleaned up {Count} temporary Exchange sessions.", cleanedCount);
            }
            else
            {
                _logger.LogTrace("No temporary Exchange sessions to clean up.");
            }
        }

       
        public async Task<string> ExecuteFunction(string functionName, params (string Name, object Value)[] parameters)
        {
            PowerShell? ps = null;
            try
            {
                _logger.LogDebug("Executing Exchange function '{FunctionName}' with {ParameterCount} parameters.", functionName, parameters.Length);
                ps = GetSession(); 

                ps.Commands.Clear(); 
                ps.AddCommand(functionName);

                _logger.LogTrace("Adding parameters for '{FunctionName}':", functionName);
                foreach (var (name, value) in parameters)
                {
                    _logger.LogTrace("- {ParamName}: {ParamValue}", name, value);
                    ps.AddParameter(name, value);
                }


                var results = await ps.InvokeAsync();
                _logger.LogTrace("Invocation completed. Result count: {ResultCount}", results?.Count ?? 0);


                if (ps.Streams.Error.Any())
                {
                    var errorMessages = ps.Streams.Error.Select(errorRecord => $"{errorRecord.Exception?.GetType().Name}: {errorRecord.Exception?.Message ?? errorRecord.ToString()}").ToList();
                    _logger.LogError("Errors occurred during execution of '{FunctionName}': {Errors}", functionName, string.Join(Environment.NewLine, errorMessages));
                    
                    return $"ERROR: {string.Join("; ", errorMessages)}"; 
                }



                string final = results?.FirstOrDefault()?.BaseObject?.ToString() ?? string.Empty;
                _logger.LogDebug("Function '{FunctionName}' executed successfully. Result: {Result}", functionName, final);

                return final;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during execution of Exchange function '{FunctionName}'.", functionName);
                return $"EXCEPTION: {ex.Message}";
            }
            finally
            {
                if (ps != null)
                {
                    ReleaseSession(ps); 
                    _logger.LogTrace("Session released after executing '{FunctionName}'.", functionName);
                }
            }
        }

        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _logger.LogInformation("Disposing ExchangePowershellSessionPoolService...");
                _cleanupTimer?.Dispose();

                _logger.LogTrace("Disposing available sessions...");
                while (_availableSessions.TryTake(out var session))
                {
                    session.Runspace?.Dispose(); 
                    session.Dispose();
                }

                _logger.LogTrace("Disposing temporary sessions...");
                
                lock (_lock)
                {
                    while (_temporarySessions.TryTake(out var session))
                    {
                        session.Runspace?.Dispose();
                        session.Dispose();
                    }
                }


                _logger.LogTrace("Disposing any remaining tracked runspaces...");
                
                while (_allRunspaces.TryTake(out var runspace))
                {
                    try
                    {
                        runspace.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error disposing runspace during final cleanup.");
                    }

                }
                _logger.LogInformation("ExchangePowershellSessionPoolService disposed.");
            }

            _disposed = true;
        }

        ~ExchangePowershellSessionPoolService()
        {
            Dispose(false); 
        }
    }
}