using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

namespace NodeSharp.Nodes.Common.ViewModels;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class MultiBoxViewModel : ObservableObject
{
    private int boxCount;
    public int BoxCount
    {
        get => boxCount;
        set
        {
            if (SetProperty(ref boxCount, value))
            {
                OnPropertyChanged(nameof(ShowBox0));
                OnPropertyChanged(nameof(ShowBox1));
                OnPropertyChanged(nameof(ShowBox2));
                OnPropertyChanged(nameof(ShowBox3));
                OnPropertyChanged(nameof(ShowBox4));
                OnPropertyChanged(nameof(ShowBox5));
                OnPropertyChanged(nameof(ShowBox6));
                OnPropertyChanged(nameof(ShowBox7));
            }
        }
    }

    [ObservableProperty] private Color onColor;
    [ObservableProperty] private Color offColor;

    [ObservableProperty] private Color box0Color;
    [ObservableProperty] private Color box1Color;
    [ObservableProperty] private Color box2Color;
    [ObservableProperty] private Color box3Color;
    [ObservableProperty] private Color box4Color;
    [ObservableProperty] private Color box5Color;
    [ObservableProperty] private Color box6Color;
    [ObservableProperty] private Color box7Color;

    public bool ShowBox0 => BoxCount > 0;
    public bool ShowBox1 => BoxCount > 1;
    public bool ShowBox2 => BoxCount > 2;
    public bool ShowBox3 => BoxCount > 3;
    public bool ShowBox4 => BoxCount > 4;
    public bool ShowBox5 => BoxCount > 5;
    public bool ShowBox6 => BoxCount > 6;
    public bool ShowBox7 => BoxCount > 7;

    public MultiBoxViewModel(int boxCount, Color onColor , Color offColor)
    {
        this.BoxCount = Math.Clamp(boxCount, 1, 8);
        this.OnColor = onColor;
        this.OffColor = offColor;

        Box0Color = this.BoxCount > 0 ? this.OffColor : Colors.Transparent;
        Box1Color = this.BoxCount > 1 ? this.OffColor : Colors.Transparent;
        Box2Color = this.BoxCount > 2 ? this.OffColor : Colors.Transparent;
        Box3Color = this.BoxCount > 3 ? this.OffColor : Colors.Transparent;
        Box4Color = this.BoxCount > 4 ? this.OffColor : Colors.Transparent;
        Box5Color = this.BoxCount > 5 ? this.OffColor : Colors.Transparent;
        Box6Color = this.BoxCount > 6 ? this.OffColor : Colors.Transparent;
        Box7Color = this.BoxCount > 7 ? this.OffColor : Colors.Transparent;
    }

    private void SetBoxState(int index, bool isOn)
    {
        var color = isOn ? OnColor : OffColor;
        switch (index)
        {
            case 0: Box0Color = color; break;
            case 1: Box1Color = color; break;
            case 2: Box2Color = color; break;
            case 3: Box3Color = color; break;
            case 4: Box4Color = color; break;
            case 5: Box5Color = color; break;
            case 6: Box6Color = color; break;
            case 7: Box7Color = color; break;
        }
    }

    public void TurnOn(int index)
    {
        SetBoxState(index, true);
    }

    public void TurnOff(int index)
    {
        SetBoxState(index, false);
    }

    public void TurnAllOff()
    {
        Box0Color = OffColor;
        Box1Color = OffColor;
        Box2Color = OffColor;
        Box3Color = OffColor;
        Box4Color = OffColor;
        Box5Color = OffColor;
        Box6Color = OffColor;
        Box7Color = OffColor;
    }

    public void TurnAllOn()
    {
        Box0Color = OnColor;
        Box1Color = OnColor;
        Box2Color = OnColor;
        Box3Color = OnColor;
        Box4Color = OnColor;
        Box5Color = OnColor;
        Box6Color = OnColor;
        Box7Color = OnColor;
    }
}
