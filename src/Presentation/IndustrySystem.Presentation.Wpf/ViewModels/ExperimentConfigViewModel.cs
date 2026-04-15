using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentConfigViewModel : NagetiveCurdVeiwModel<object>
{
    protected override Task<IReadOnlyList<object>> LoadItemsAsync()
        => Task.FromResult<IReadOnlyList<object>>([]);
}
