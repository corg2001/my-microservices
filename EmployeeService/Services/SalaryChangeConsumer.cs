using Azure.Messaging.EventHubs.Consumer;
using EmployeeService.Models;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace EmployeeService.Services
{
    /// <summary>
    /// Background service that consumes salary change CDC events from Azure Event Hubs.
    /// Debezium Server streams changes from dbo.Employees to the employee-salary-changes hub.
    /// This service reads those events and reacts — logging the change and updating a local cache.
    /// In production this could trigger notifications, audit records, or downstream events.
    /// </summary>
    public class SalaryChangeConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SalaryChangeConsumer> _logger;

        public SalaryChangeConsumer(
            IConfiguration configuration,
            ILogger<SalaryChangeConsumer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionString = _configuration["EventHubs:ConnectionString"];
            var hubName = _configuration["EventHubs:HubName"] ?? "employee-salary-changes";
            var consumerGroup = _configuration["EventHubs:ConsumerGroup"] ?? EventHubConsumerClient.DefaultConsumerGroupName;

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("Event Hubs connection string not configured. SalaryChangeConsumer will not start.");
                return;
            }

            _logger.LogInformation("SalaryChangeConsumer starting. Hub: {Hub}, ConsumerGroup: {Group}",
                hubName, consumerGroup);

            await using var consumer = new EventHubConsumerClient(consumerGroup, connectionString, hubName);

            try
            {
                // Get all partition IDs
                var partitionIds = await consumer.GetPartitionIdsAsync(stoppingToken);

                // Start reading from all partitions
                var readTasks = partitionIds.Select(async partitionId =>
                {
                    await foreach (var partitionEvent in consumer.ReadEventsFromPartitionAsync(
                        partitionId,
                        EventPosition.Latest,
                        stoppingToken))
                    {
                        if (partitionEvent.Data == null) continue;

                        try
                        {
                            var json = partitionEvent.Data.EventBody.ToString();
                            var changeEvent = JsonSerializer.Deserialize<SalaryChangeEvent>(json,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (changeEvent == null) continue;

                            await ProcessSalaryChangeAsync(changeEvent);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning(ex, "Failed to deserialize CDC event. Raw: {Raw}",
                                partitionEvent.Data.EventBody.ToString());
                        }
                    }
                });

                await Task.WhenAll(readTasks);
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
                _logger.LogInformation("SalaryChangeConsumer stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SalaryChangeConsumer encountered a fatal error.");
                throw;
            }
        }

        private Task ProcessSalaryChangeAsync(SalaryChangeEvent changeEvent)
        {
            switch (changeEvent.Op)
            {
                case "u" when changeEvent.Before != null && changeEvent.After != null:
                    // Salary update — the most common CDC event we care about
                    if (changeEvent.Before.Salary != changeEvent.After.Salary)
                    {
                        _logger.LogInformation(
                            "Salary change detected — Employee: {FirstName} {LastName} (ID: {Id}) | " +
                            "{OldSalary:C} → {NewSalary:C} | Department: {Department}",
                            changeEvent.After.FirstName,
                            changeEvent.After.LastName,
                            changeEvent.After.Id,
                            changeEvent.Before.Salary,
                            changeEvent.After.Salary,
                            changeEvent.After.Department);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Employee record updated (non-salary) — ID: {Id}", changeEvent.After.Id);
                    }
                    break;

                case "c":
                    // New employee created
                    _logger.LogInformation(
                        "New employee created — {FirstName} {LastName} (ID: {Id}), Salary: {Salary:C}",
                        changeEvent.After?.FirstName,
                        changeEvent.After?.LastName,
                        changeEvent.After?.Id,
                        changeEvent.After?.Salary);
                    break;

                case "d":
                    // Employee deleted
                    _logger.LogInformation(
                        "Employee deleted — ID: {Id}", changeEvent.Before?.Id);
                    break;

                case "r":
                    // Snapshot read — emitted during initial snapshot, not a real change
                    _logger.LogDebug(
                        "Snapshot event — Employee ID: {Id}", changeEvent.After?.Id);
                    break;

                default:
                    _logger.LogWarning("Unknown CDC operation: {Op}", changeEvent.Op);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}