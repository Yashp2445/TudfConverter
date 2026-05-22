using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TudfConverter.Application.Configuration;
using TudfConverter.Application.Interfaces;
using TudfConverter.Application.Models;
using TudfConverter.Application.Pipeline;
using TudfConverter.Domain.Validation.Results;
using TudfConverter.Infrastructure.FileStorage;
using TudfConverter.WpfUI.Models;
using TudfConverter.WpfUI.Views;

namespace TudfConverter.WpfUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IFileProcessingService _processingService;
    private readonly HistoryRepository _historyRepository;
    private readonly TudfAppSettings _settings;
    
    private readonly ProcessFileView _processFileView;
    private readonly ValidationResultsView _validationResultsView;
    private readonly HistoryView _historyView;

    private List<ValidationError> _allValidationErrors = new();

    public MainViewModel(
        IFileProcessingService processingService,
        HistoryRepository historyRepository,
        TudfAppSettings settings,
        ProcessFileView processFileView,
        ValidationResultsView validationResultsView,
        HistoryView historyView)
    {
        _processingService = processingService;
        _historyRepository = historyRepository;
        _settings = settings;

        _processFileView = processFileView;
        _validationResultsView = validationResultsView;
        _historyView = historyView;

        MemberUserId = _settings.MemberUserId;

        _processFileView.DataContext = this;
        _validationResultsView.DataContext = this;
        _historyView.DataContext = this;

        CurrentView = _processFileView;
        PageTitle = "Process File";
        _ = LoadHistoryAsync();
    }

    [ObservableProperty]
    private object _currentView = null!;

    [ObservableProperty]
    private int _selectedNavIndex = 0;

    [ObservableProperty]
    private string _pageTitle = "Process File";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessFileCommand))]
    private string _selectedFilePath = string.Empty;

    [ObservableProperty]
    private DateOnly _reportDate = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    private string _selectedReportingCycle = "W1";

    [ObservableProperty]
    private string _memberUserId = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessFileCommand))]
    private bool _isProcessing = false;

    [ObservableProperty]
    private int _progressValue = 0;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _hasResult = false;

    [ObservableProperty]
    private int _totalRows;

    [ObservableProperty]
    private int _acceptedRows;

    [ObservableProperty]
    private int _rejectedRows;

    [ObservableProperty]
    private int _warningCount;

    [ObservableProperty]
    private string? _generatedFilePath;

    [ObservableProperty]
    private string? _reportFilePath;

    [ObservableProperty]
    private ObservableCollection<ValidationErrorDisplayModel> _filteredValidationErrors = new();

    private string _searchFilter = string.Empty;
    public string SearchFilter
    {
        get => _searchFilter;
        set
        {
            if (SetProperty(ref _searchFilter, value))
            {
                RefreshFilteredErrors();
            }
        }
    }

    private string _selectedOutcomeFilter = "All Results";
    public string SelectedOutcomeFilter
    {
        get => _selectedOutcomeFilter;
        set
        {
            if (SetProperty(ref _selectedOutcomeFilter, value))
            {
                RefreshFilteredErrors();
            }
        }
    }

    [ObservableProperty]
    private int _rejectedRowCount;

    [ObservableProperty]
    private int _fieldErrorCount;

    [ObservableProperty]
    private int _segmentErrorCount;

    [ObservableProperty]
    private ObservableCollection<ProcessingHistoryItem> _processingHistory = new();

    private async Task LoadHistoryAsync()
    {
        var history = await _historyRepository.LoadAsync();
        foreach (var item in history)
        {
            ProcessingHistory.Add(item);
        }
    }

    [RelayCommand]
    private void BrowseFile()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Files|*.xlsx",
            Title = "Select Excel File"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedFilePath = dialog.FileName;
        }
    }

    private bool CanProcessFile()
    {
        return !IsProcessing && !string.IsNullOrWhiteSpace(SelectedFilePath);
    }

    [RelayCommand(CanExecute = nameof(CanProcessFile))]
    private async Task ProcessFileAsync()
    {
        IsProcessing = true;
        HasResult = false;
        ProgressValue = 0;
        StatusMessage = "Starting process...";

        try
        {
            var options = new ProcessingOptions
            {
                MemberUserId = MemberUserId,
                MemberShortName = _settings.MemberShortName,
                DateReportedAndCertified = ReportDate,
                ReportingCycle = SelectedReportingCycle,
                OutputFolder = _settings.OutputFolder ?? "Output",
                ReportFolder = _settings.ReportFolder ?? "Reports"
            };

            var progress = new Progress<ProcessingProgress>(p =>
            {
                ProgressValue = p.Percentage;
                StatusMessage = p.Message;
            });

            var result = await _processingService.ProcessFileAsync(SelectedFilePath, options, progress, CancellationToken.None);

            HasResult = true;
            TotalRows = result.TotalRows;
            AcceptedRows = result.AcceptedRows;
            RejectedRows = result.RejectedRows;

            _allValidationErrors = result.ValidationResults.SelectMany(r => r.Errors).ToList();
            WarningCount = _allValidationErrors.Count(e => e.Outcome == FailureOutcome.RejectSegment || e.Outcome == FailureOutcome.RejectField);

            GeneratedFilePath = result.GeneratedFilePath;
            ReportFilePath = result.ReportFilePath;

            RefreshFilteredErrors();

            var historyItem = new ProcessingHistoryItem
            {
                ProcessedAt = result.ProcessedAt,
                InputFileName = Path.GetFileName(SelectedFilePath),
                TotalRows = result.TotalRows,
                AcceptedRows = result.AcceptedRows,
                RejectedRows = result.RejectedRows,
                OutputFilePath = result.GeneratedFilePath,
                ReportFilePath = result.ReportFilePath
            };

            ProcessingHistory.Insert(0, historyItem);
            await _historyRepository.SaveAsync(ProcessingHistory.ToList());

            if (_allValidationErrors.Any())
            {
                NavigateToValidationResults(); // Auto navigate to validation results if errors exist
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void NavigateToProcessFile()
    {
        SelectedNavIndex = 0;
        CurrentView = _processFileView;
        PageTitle = "Process File";
    }

    [RelayCommand]
    private void NavigateToValidationResults()
    {
        SelectedNavIndex = 1;
        CurrentView = _validationResultsView;
        PageTitle = "Validation Results";
    }

    [RelayCommand]
    private void NavigateToHistory()
    {
        SelectedNavIndex = 2;
        CurrentView = _historyView;
        PageTitle = "Processing History";
    }

    [RelayCommand]
    private void OpenOutputFile()
    {
        if (!string.IsNullOrEmpty(GeneratedFilePath) && File.Exists(GeneratedFilePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"/select,\"{GeneratedFilePath}\""
            });
        }
    }

    [RelayCommand]
    private void OpenReportFile()
    {
        if (!string.IsNullOrEmpty(ReportFilePath) && File.Exists(ReportFilePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = ReportFilePath,
                UseShellExecute = true
            });
        }
    }

    [RelayCommand]
    private void OpenHistoryOutputFile(ProcessingHistoryItem item)
    {
        if (item != null && !string.IsNullOrEmpty(item.OutputFilePath) && File.Exists(item.OutputFilePath))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"/select,\"{item.OutputFilePath}\""
            });
        }
    }

    [RelayCommand]
    private void ExportErrors()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV File|*.csv",
            Title = "Export Validation Errors",
            FileName = $"ValidationErrors_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            using var writer = new StreamWriter(dialog.FileName);
            writer.WriteLine("RowNumber,RecordStatus,SegmentTag,ErrorCode,FieldName,ErrorMessage,Outcome");
            foreach (var err in FilteredValidationErrors)
            {
                writer.WriteLine($"{err.RowNumber},{err.RecordStatus},{err.SegmentTag},{err.ErrorCode},{err.FieldName},\"{err.ErrorMessage?.Replace("\"", "\"\"")}\",{err.Outcome}");
            }
        }
    }

    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        ProcessingHistory.Clear();
        await _historyRepository.SaveAsync(new List<ProcessingHistoryItem>());
    }

    private void RefreshFilteredErrors()
    {
        var filtered = _allValidationErrors.AsEnumerable();

        if (SelectedOutcomeFilter == "Rejected Records Only")
        {
            filtered = filtered.Where(e => e.Outcome == FailureOutcome.RejectRecord);
        }
        else if (SelectedOutcomeFilter == "Field Errors Only")
        {
            filtered = filtered.Where(e => e.Outcome == FailureOutcome.RejectField);
        }
        else if (SelectedOutcomeFilter == "Segment Errors Only")
        {
            filtered = filtered.Where(e => e.Outcome == FailureOutcome.RejectSegment);
        }

        if (!string.IsNullOrWhiteSpace(SearchFilter))
        {
            var searchLower = SearchFilter.ToLowerInvariant();
            filtered = filtered.Where(e =>
                e.RowNumber.ToString().Contains(searchLower) ||
                (e.ErrorCode != null && e.ErrorCode.ToLowerInvariant().Contains(searchLower)) ||
                (e.FieldName != null && e.FieldName.ToLowerInvariant().Contains(searchLower)) ||
                (e.ErrorMessage != null && e.ErrorMessage.ToLowerInvariant().Contains(searchLower)));
        }

        var displayModels = filtered.Select(e => ValidationErrorDisplayModel.FromValidationError(e, e.Outcome == FailureOutcome.RejectRecord)).ToList();

        FilteredValidationErrors.Clear();
        foreach (var item in displayModels)
        {
            FilteredValidationErrors.Add(item);
        }

        RejectedRowCount = _allValidationErrors.Count(e => e.Outcome == FailureOutcome.RejectRecord);
        FieldErrorCount = _allValidationErrors.Count(e => e.Outcome == FailureOutcome.RejectField);
        SegmentErrorCount = _allValidationErrors.Count(e => e.Outcome == FailureOutcome.RejectSegment);
    }
}
