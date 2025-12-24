using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;
using Prism.Mvvm;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentTemplateViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IExperimentTemplateAppService _svc;
    public ObservableCollection<ExperimentTemplateDto> Templates { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }

    public ExperimentTemplateViewModel(IExperimentTemplateAppService svc)
    {
        _svc = svc;
        RefreshCommand = new AsyncDelegateCommand(LoadAsync);
        DeleteCommand = new AsyncDelegateCommand<Guid>(DeleteAsync);
        _ = LoadAsync();
        _logger.Info(Resources.Strings.Log_ExperimentTemplateViewModel_Initialized);
    }

    public async Task LoadAsync()
    {
        _logger.Debug(Resources.Strings.Log_ExperimentTemplate_LoadStart);
        Templates.Clear();
        foreach (var t in await _svc.GetListAsync()) Templates.Add(t);
        _logger.Info(string.Format(Resources.Strings.Log_ExperimentTemplate_LoadComplete, Templates.Count));
    }

    public async Task AddAsync(string name, string? desc)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        _logger.Info(string.Format(Resources.Strings.Log_ExperimentTemplate_Add, name));
        var dto = await _svc.CreateAsync(new ExperimentTemplateDto(Guid.Empty, name, desc));
        Templates.Add(dto);
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.Info(string.Format(Resources.Strings.Log_ExperimentTemplate_Delete, id));
        await _svc.DeleteAsync(id);
        var target = Templates.FirstOrDefault(x => x.Id == id);
        if (target != null) Templates.Remove(target);
    }
}
