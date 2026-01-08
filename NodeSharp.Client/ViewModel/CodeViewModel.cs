using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodeSharp.NodeEngine.Helper;
using NodeSharp.NodeEngine.Node;

namespace NodeSharp.Client.ViewModel
{
    [SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
        "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
    [SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
        "MVVMTK0034:Direct field reference to [ObservableProperty] backing field")]
    public partial class CodeViewModel(IPopupService popupService) : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] private NodeFunction? selectedNodeFunction;
        [ObservableProperty] private string codeText = string.Empty;
        [ObservableProperty] private bool isSaveEnabled;
        [ObservableProperty] private bool hasCompilerError;
        [ObservableProperty] private string? compilerOutput;

        private string codeTextOriginal = string.Empty;


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
                CompilerOutput = "";
                HasCompilerError = false;

                var roslynHelper = new RoslynHelper(FunctionData.MessageTemplate, CodeText);
                var cSharpCompilation = roslynHelper.CompileScript();
                var diagnostics = roslynHelper.GetDiagnostics();

                if (roslynHelper.HasCompilerError || roslynHelper.HasCompilerWarning)
                {
                    HasCompilerError = true;
                    CompilerOutput = diagnostics;
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
            SelectedNodeFunction = (NodeFunction)query[nameof(NodeFunction)];

            if (SelectedNodeFunction is not null)
            {
                CodeText = SelectedNodeFunction.FunctionData.SourceCode;
                codeTextOriginal = SelectedNodeFunction.FunctionData.SourceCode;

                UpdateToolbarCommandStates();
            }
        }

        private void UpdateToolbarCommandStates()
        {
            CancelCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            ValidateCommand.NotifyCanExecuteChanged();
        }
    }
}