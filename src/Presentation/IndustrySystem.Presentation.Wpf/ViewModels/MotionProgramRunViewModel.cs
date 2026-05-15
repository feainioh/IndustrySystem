namespace IndustrySystem.Presentation.Wpf.ViewModels;

/// <summary>
/// MotionProgramRunView 的页面级 ViewModel，聚合具体执行器 ViewModel。
/// </summary>
public class MotionProgramRunViewModel : NagetiveViewModel
{
    public MotionProgramViewerViewModel Viewer { get; }

    public MotionProgramRunViewModel(MotionProgramViewerViewModel viewer)
    {
        Viewer = viewer;
        Title = "Motion Program";
    }
}
