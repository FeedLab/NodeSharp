using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeSharp.Nodes.Common.Helper;
using NodeSharp.Nodes.Function;

namespace NodeSharp.Client.ViewModel
{
    [SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
        "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
    [SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
        "MVVMTK0034:Direct field reference to [ObservableProperty] backing field")]
    [SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator",
        "MVVMTK0007:Invalid RelayCommand method signature")]
    public partial class CodeViewModel(IPopupService popupService)
        : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] private NodeFunction? selectedNodeFunction;
        [ObservableProperty] private string codeText = string.Empty;
        [ObservableProperty] private bool isSaveEnabled;
        [ObservableProperty] private bool hasCompilerError;
        [ObservableProperty] private bool hasBeenValidated;
        [ObservableProperty] private string? compilerOutput;

        private string codeTextOriginal = string.Empty;
        [ObservableProperty] private ObservableCollection<LogEntry> logMessages = [];

        public void AddLog(string message, string level = "Info")
        {
            LogMessages.Add(new LogEntry { Message = message, Level = level });
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        async Task Save()
        {
            SelectedNodeFunction?.FunctionData.SourceCode = CodeText;
            await popupService.ClosePopupAsync(Shell.Current);
        }

        [RelayCommand(CanExecute = nameof(CanCancel))]
        async Task Cancel()
        {
            await popupService.ClosePopupAsync(Shell.Current);
        }

        [RelayCommand(CanExecute = nameof(CanValidate))]
        void Validate()
        {
            if (selectedNodeFunction is not null)
            {
                // CompilerOutput = "";
                HasCompilerError = false;
                HasBeenValidated = false;

                var roslynHelper = new RoslynHelper(FunctionData.MessageTemplate, CodeText);
                var cSharpCompilation = roslynHelper.CompileScript();
                var diagnostics = roslynHelper.GetDiagnostics();

                if (roslynHelper.HasCompilerError || roslynHelper.HasCompilerWarning)
                {
                    HasCompilerError = roslynHelper.HasCompilerError;
                    // CompilerOutput = diagnostics;
                    foreach (var diagnostic in diagnostics)
                    {
                        AddLog(diagnostic, "Error");
                    }
                }
            }
        }

        bool CanSave()
        {
            IsSaveEnabled = !string.IsNullOrWhiteSpace(CodeText);

            if (CodeText == codeTextOriginal)
            {
                IsSaveEnabled = false;
            }

            if (HasCompilerError)
            {
                IsSaveEnabled = false;
            }

            if (!HasBeenValidated)
            {
                IsSaveEnabled = false;
            }

            return IsSaveEnabled;
        }

        bool CanValidate()
        {
            if (selectedNodeFunction is null)
            {
                return false;
            }

            if (CodeText == codeTextOriginal)
            {
                return false;
            }

            return true;
        }

        bool CanCancel()
        {
            return true;
        }

        partial void OnCodeTextChanged(string value)
        {
            CodeText = value;

            UpdateToolbarCommandStates();
        }

        public bool HasChangedCode => CodeText != codeTextOriginal;


        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            LogMessages.Clear();
            codeTextOriginal = string.Empty;
            HasBeenValidated = false;
            HasCompilerError = false;
            SelectedNodeFunction = (NodeFunction)query[nameof(NodeFunction)];

            if (SelectedNodeFunction is not null)
            {
                CodeText = SelectedNodeFunction.FunctionData.SourceCode;
                codeTextOriginal = SelectedNodeFunction.FunctionData.SourceCode;

                UpdateToolbarCommandStates();

                AddLog("System initialized...");
            }
        }

        private void UpdateToolbarCommandStates()
        {
            CancelCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            ValidateCommand.NotifyCanExecuteChanged();
        }
    }
    
    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Level { get; set; } = "Info"; // Info, Warning, Error
        public string Message { get; set; } = string.Empty;
    }

}