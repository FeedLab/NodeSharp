using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeSharp.Client.Component;

public partial class CodeComponent : ContentView
{
    private CodeViewModel viewModel;

    public CodeComponent()
    {
        viewModel = AppService.GetRequiredService<CodeViewModel>();


        InitializeComponent();

        this.BindingContext = viewModel;
    }
}