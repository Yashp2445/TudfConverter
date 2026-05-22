using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TudfConverter.Application.Models;

namespace TudfConverter.Infrastructure.FileStorage;

public class HistoryRepository
{
    private readonly string _filePath;

    public HistoryRepository()
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "processing_history.json");
    }

    public async Task<List<ProcessingHistoryItem>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<ProcessingHistoryItem>();
        }

        try
        {
            using var stream = File.OpenRead(_filePath);
            var history = await JsonSerializer.DeserializeAsync<List<ProcessingHistoryItem>>(stream);
            return history ?? new List<ProcessingHistoryItem>();
        }
        catch (Exception)
        {
            // If the file is corrupted, return an empty history rather than crashing
            return new List<ProcessingHistoryItem>();
        }
    }

    public async Task SaveAsync(List<ProcessingHistoryItem> history)
    {
        using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, history, new JsonSerializerOptions { WriteIndented = true });
    }
}
