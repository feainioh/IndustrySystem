using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentTemplateViewModel
{
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
    }

    public async Task LoadAsync()
    {
        Templates.Clear();
        foreach (var t in await _svc.GetListAsync()) Templates.Add(t);
    }

    public async Task AddAsync(string name, string? desc)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        var dto = await _svc.CreateAsync(new ExperimentTemplateDto(Guid.Empty, name, desc));
        Templates.Add(dto);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _svc.DeleteAsync(id);
        var target = Templates.FirstOrDefault(x => x.Id == id);
        if (target != null) Templates.Remove(target);
    }
}
