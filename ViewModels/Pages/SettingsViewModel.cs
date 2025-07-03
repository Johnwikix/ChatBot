using System.Collections.ObjectModel;
using System.Diagnostics;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;
using wpfChat.Data;
using wpfChat.Models;

namespace wpfChat.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;
        [ObservableProperty]
        private uint _contextSize = AppConfig.ContextSize;
        [ObservableProperty]
        private int _maxTokens = AppConfig.MaxTokens;
        [ObservableProperty]
        private int _totalLayers = AppConfig.TotalLayers;
        [ObservableProperty]
        private string _initalPrompt = AppConfig.InitialPrompt;
        [ObservableProperty]
        private string _endPrompt = AppConfig.EndPrompt;
        [ObservableProperty]
        private ObservableCollection<Prompt> _promptPresets = AppConfig.PromptPresets;
        [ObservableProperty]
        private Prompt _selectedPromptPreset = new Prompt();
        partial void OnSelectedPromptPresetChanged(Prompt value)
        {
            if (value != null && _isInitialized)
            {
                InitalPrompt = value.Content;
                EndPrompt = value.End;
                AppConfig.SelectPromptName = value.Name;
                _ = DataService.SaveAllAppConfigAsync();
            }
        }
        private double _temperature = AppConfig.Temperature;
        public double Temperature
        {
            get => _temperature;
            set
            {
                if (SetProperty(ref _temperature, value)) {
                    if (_isInitialized)
                    {
                        AppConfig.Temperature = (float)value;
                        _=DataService.SaveAllAppConfigAsync();
                    }
                }
            }
        }


        [ObservableProperty]
        private double _typicalP = AppConfig.TypicalP;
        partial void OnTypicalPChanged(double value)
        {
            if (_isInitialized)
            {
                AppConfig.TypicalP = (float)value;
                _ = DataService.SaveAllAppConfigAsync();
            }
        }
        [ObservableProperty]
        private double _topP = AppConfig.TopP;
        partial void OnTopPChanged(double value)
        {
            if (_isInitialized)
            {
                AppConfig.TopP = (float)value;
                _ = DataService.SaveAllAppConfigAsync();
            }
        }
        [ObservableProperty]
        private double _topK = AppConfig.TopK;
        partial void OnTopKChanged(double value)
        {
            if (_isInitialized)
            {
                AppConfig.TopK = (int)value;
                _ = DataService.SaveAllAppConfigAsync();
            }
        }
        [ObservableProperty]
        private double _repeatPenalty = AppConfig.RepeatPenalty;
        partial void OnRepeatPenaltyChanged(double value)
        {
            if (_isInitialized)
            {
                AppConfig.RepeatPenalty = (float)value;
                _ = DataService.SaveAllAppConfigAsync();
            }
        }
        [ObservableProperty]
        private double _minKeep = AppConfig.MinKeep;
        partial void OnMinKeepChanged(double value)
        {
            if (_isInitialized)
            {
                AppConfig.MinKeep = (int)value;
                _ = DataService.SaveAllAppConfigAsync();
            }
        }        

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
                InitializeViewModel();

            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"UiDesktopApp1 - {GetAssemblyVersion()}";
            SelectedPromptPreset = AppConfig.PromptPresets.FirstOrDefault(p => p.Name == AppConfig.SelectPromptName)
                ?? AppConfig.PromptPresets.FirstOrDefault();
            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;

                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;

                    break;
            }
        }

        public void ContextSizeChanged(uint value)
        {
            AppConfig.ContextSize = value;
        }
        public void MaxTokensChanged(int value)
        {
            AppConfig.MaxTokens = value;
        }
        public void TotalLayersChanged(int value)
        {
            AppConfig.TotalLayers = value;
        }
        public void InitalPromptChanged(string value)
        {
            AppConfig.InitialPrompt = value;
        }
        public void EndPromptChanged(string value)
        {
            AppConfig.EndPrompt = value;
        }
    }
}
