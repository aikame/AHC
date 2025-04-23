using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text;
using Elastic.Clients.Elasticsearch;
using DBC.Data;
using Microsoft.EntityFrameworkCore;

namespace DBC.Services
{

    public class ElasticIndexStateService : IHostedService, IDisposable
    {
        private readonly ILogger<ElasticIndexStateService> _logger;
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ElasticsearchClient _elasticsearchClient;
        public ElasticIndexStateService(IServiceProvider serviceProvider, ILogger<ElasticIndexStateService> logger, ElasticsearchClient elasticsearchClient)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _elasticsearchClient = elasticsearchClient;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ElasticIndexStateService is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            CheckProfileIndexState();
            CheckComputerIndexState();
            CheckGroupIndexState();
        }
        protected async Task CheckProfileIndexState()
        {
            _logger.LogInformation("CheckProfileIndexState started");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var unsyncedProfiles = await db.Profiles
                        .Where(p => !p.isIndexed)
                        .ToListAsync();

                    foreach (var profile in unsyncedProfiles)
                    {
                        try
                        {
                            var elasticProfile = profile.ToElasticProfileModel();
                            var response = await _elasticsearchClient.IndexAsync(elasticProfile, i => i
                                .Index("profiles")
                                .Id(profile.Id));

                            if (response.IsValidResponse)
                            {
                                profile.isIndexed = true;
                                db.Profiles.Update(profile);
                            }
                            else
                            {
                                _logger.LogWarning($"[CheckProfileIndexState] Sync Error ID {profile.Id}: {response.DebugInformation}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"[CheckProfileIndexState] Sync profile error ID {profile.Id}: {ex.Message}");
                        }
                    }

                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError("[CheckProfileIndexState] Total error: " + e.Message);
                }
            

            _logger.LogInformation("CheckProfileIndexState stopped");
        }
        protected async Task CheckComputerIndexState()
        {
            _logger.LogInformation("CheckComputerIndexState started");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var unsyncedComputers = await db.Computers
                    .Where(p => !p.isIndexed)
                    .ToListAsync();

                foreach (var computer in unsyncedComputers)
                {
                    try
                    {
                        var response = await _elasticsearchClient.IndexAsync(computer, i => i
                            .Index("computers")
                            .Id(computer.Id));

                        if (response.IsValidResponse)
                        {
                            computer.isIndexed = true;
                            db.Computers.Update(computer);
                        }
                        else
                        {
                            _logger.LogWarning($"[CheckComputerIndexState] Sync Error ID {computer.Id}: {response.DebugInformation}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[CheckComputerIndexState] Sync profile error ID {computer.Id}: {ex.Message}");
                    }
                }

                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("[CheckComputerIndexState] Total error: " + e.Message);
            }


            _logger.LogInformation("CheckComputerIndexState stopped");
        }

        protected async Task CheckGroupIndexState()
        {
            _logger.LogInformation("CheckGroupIndexState started");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var unsyncedGroups = await db.Groups
                    .Where(p => !p.isIndexed)
                    .ToListAsync();

                foreach (var group in unsyncedGroups)
                {
                    try
                    {
                        var response = await _elasticsearchClient.IndexAsync(group, i => i
                            .Index("groups")
                            .Id(group.Id));

                        if (response.IsValidResponse)
                        {
                            group.isIndexed = true;
                            db.Groups.Update(group);
                        }
                        else
                        {
                            _logger.LogWarning($"[CheckGroupIndexState] Sync Error ID {group.Id}: {response.DebugInformation}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[CheckGroupIndexState] Sync profile error ID {group.Id}: {ex.Message}");
                    }
                }

                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("[CheckGroupIndexState] Total error: " + e.Message);
            }


            _logger.LogInformation("CheckGroupIndexState stopped");
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ElasticIndexStateService is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
