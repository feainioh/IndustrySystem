using System.ComponentModel;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

/// <summary>
/// 参数编辑区域宿主契约，允许不同页面复用参数编辑器逻辑。
/// </summary>
public interface IParameterEditorHost : INotifyPropertyChanged
{
    ExperimentParameterItemDto? CurrentParameterDetail { get; }
}
