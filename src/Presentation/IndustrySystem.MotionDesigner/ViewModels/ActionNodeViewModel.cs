using System.Windows.Media;
using IndustrySystem.Application.Contracts.Dtos.MotionProgram;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels;

public class ActionNodeViewModel : BindableBase
{
    private Guid _id;
    private string _name = string.Empty;
    private ActionType _actionType;
    private Dictionary<string, object> _parameters = new();
    private double _x;
    private double _y;
    private double _width = 180;
    private double _height = 60;
    private string _description = string.Empty;
    private bool _isEnabled = true;
    private bool _isSelected;
    private bool _isExecuting;
    private bool _hasError;
    private int _timeoutMs;
    private int _retryCount;
    private ErrorHandling _errorHandling = ErrorHandling.Stop;

    public Guid Id { get => _id; set => SetProperty(ref _id, value); }
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public ActionType ActionType
    {
        get => _actionType;
        set
        {
            if (SetProperty(ref _actionType, value))
            {
                RaisePropertyChanged(nameof(IconKind));
                RaisePropertyChanged(nameof(HeaderColor));
            }
        }
    }
    public Dictionary<string, object> Parameters { get => _parameters; set => SetProperty(ref _parameters, value); }
    public double X { get => _x; set => SetProperty(ref _x, value); }
    public double Y { get => _y; set => SetProperty(ref _y, value); }
    public double Width { get => _width; set => SetProperty(ref _width, value); }
    public double Height { get => _height; set => SetProperty(ref _height, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }
    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    public bool IsExecuting { get => _isExecuting; set => SetProperty(ref _isExecuting, value); }
    public bool HasError { get => _hasError; set => SetProperty(ref _hasError, value); }
    public int TimeoutMs { get => _timeoutMs; set => SetProperty(ref _timeoutMs, value); }
    public int RetryCount { get => _retryCount; set => SetProperty(ref _retryCount, value); }
    public ErrorHandling ErrorHandling { get => _errorHandling; set => SetProperty(ref _errorHandling, value); }

    public string IconKind => ActionType switch
    {
        ActionType.MotorMoveAbsolute => "AxisArrow",
        ActionType.MotorMoveRelative => "AxisArrowInfo",
        ActionType.MotorHome => "Home",
        ActionType.MotorStop => "Stop",
        ActionType.WaitMotorDone => "TimerSand",
        ActionType.IoOutput => "ExportVariant",
        ActionType.WaitIoInput => "ImportVariant",
        ActionType.RobotMoveTo => "Robot",
        ActionType.RobotRunProgram => "RobotIndustrial",
        ActionType.Delay => "ClockOutline",
        ActionType.LoopStart => "Repeat",
        ActionType.LoopEnd => "RepeatOff",
        ActionType.Condition => "HelpRhombus",
        ActionType.CallSubProgram => "FunctionVariant",
        ActionType.SetVariable => "Variable",
        ActionType.Log => "TextBoxOutline",
        ActionType.Alarm => "AlertCircle",
        // Logic Control Icons
        ActionType.IfStart => "CodeBraces",
        ActionType.ElseIf => "CodeBrackets",
        ActionType.Else => "CodeBrackets",
        ActionType.IfEnd => "CodeBraces",
        ActionType.WhileStart => "Sync",
        ActionType.WhileEnd => "SyncOff",
        ActionType.Break => "ExitRun",
        ActionType.Continue => "SkipNext",
        ActionType.Return => "KeyboardReturn",
        ActionType.Switch => "FormatListBulleted",
        ActionType.Case => "CheckboxMarked",
        ActionType.Default => "CheckboxBlankOutline",
        ActionType.SwitchEnd => "FormatListBulleted",
        ActionType.ParallelStart => "CallSplit",
        ActionType.ParallelEnd => "CallMerge",
        ActionType.ParallelBranch => "SourceBranch",
        _ => "CogOutline"
    };

    public Brush HeaderColor => ActionType switch
    {
        ActionType.MotorMoveAbsolute or ActionType.MotorMoveRelative or ActionType.MotorHome or ActionType.MotorStop or ActionType.WaitMotorDone 
            => new SolidColorBrush(Color.FromRgb(33, 150, 243)),  // Blue - Motor
        ActionType.IoOutput or ActionType.WaitIoInput 
            => new SolidColorBrush(Color.FromRgb(76, 175, 80)),   // Green - IO
        ActionType.RobotMoveTo or ActionType.RobotRunProgram 
            => new SolidColorBrush(Color.FromRgb(156, 39, 176)),  // Purple - Robot
        ActionType.Delay => new SolidColorBrush(Color.FromRgb(255, 152, 0)),  // Orange
        ActionType.LoopStart or ActionType.LoopEnd => new SolidColorBrush(Color.FromRgb(0, 188, 212)),  // Cyan
        ActionType.Condition => new SolidColorBrush(Color.FromRgb(255, 193, 7)),  // Amber
        ActionType.CallSubProgram => new SolidColorBrush(Color.FromRgb(63, 81, 181)),  // Indigo
        ActionType.SetVariable => new SolidColorBrush(Color.FromRgb(121, 85, 72)),  // Brown
        ActionType.Log => new SolidColorBrush(Color.FromRgb(96, 125, 139)),  // Blue Grey
        ActionType.Alarm => new SolidColorBrush(Color.FromRgb(244, 67, 54)),  // Red
        // Logic Control Colors
        ActionType.IfStart or ActionType.ElseIf or ActionType.Else or ActionType.IfEnd 
            => new SolidColorBrush(Color.FromRgb(233, 30, 99)),   // Pink - If/Else
        ActionType.WhileStart or ActionType.WhileEnd 
            => new SolidColorBrush(Color.FromRgb(0, 150, 136)),   // Teal - While
        ActionType.Break or ActionType.Continue or ActionType.Return 
            => new SolidColorBrush(Color.FromRgb(255, 87, 34)),   // Deep Orange - Control Flow
        ActionType.Switch or ActionType.Case or ActionType.Default or ActionType.SwitchEnd 
            => new SolidColorBrush(Color.FromRgb(103, 58, 183)),  // Deep Purple - Switch
        ActionType.ParallelStart or ActionType.ParallelEnd or ActionType.ParallelBranch 
            => new SolidColorBrush(Color.FromRgb(0, 188, 212)),   // Cyan - Parallel
        _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))
    };

    public static ActionNodeViewModel CreateDefault(ActionType actionType, double x = 100, double y = 100)
    {
        return new ActionNodeViewModel
        {
            Id = Guid.NewGuid(),
            ActionType = actionType,
            Name = GetDefaultName(actionType),
            Description = GetDefaultDescription(actionType),
            X = x, Y = y, Width = 180, Height = 60, IsEnabled = true
        };
    }

    private static string GetDefaultName(ActionType type) => type switch
    {
        ActionType.MotorMoveAbsolute => "Motor Absolute",
        ActionType.MotorMoveRelative => "Motor Relative",
        ActionType.MotorHome => "Motor Home",
        ActionType.MotorStop => "Motor Stop",
        ActionType.WaitMotorDone => "Wait Motor Done",
        ActionType.IoOutput => "IO Output",
        ActionType.WaitIoInput => "Wait IO Input",
        ActionType.RobotMoveTo => "Robot Move",
        ActionType.RobotRunProgram => "Run Robot Program",
        ActionType.Delay => "Delay",
        ActionType.LoopStart => "Loop Start",
        ActionType.LoopEnd => "Loop End",
        ActionType.Condition => "Condition",
        ActionType.CallSubProgram => "Call SubProgram",
        ActionType.SetVariable => "Set Variable",
        ActionType.Log => "Log",
        ActionType.Alarm => "Alarm",
        // Logic Control Names
        ActionType.IfStart => "If",
        ActionType.ElseIf => "Else If",
        ActionType.Else => "Else",
        ActionType.IfEnd => "End If",
        ActionType.WhileStart => "While",
        ActionType.WhileEnd => "End While",
        ActionType.Break => "Break",
        ActionType.Continue => "Continue",
        ActionType.Return => "Return",
        ActionType.Switch => "Switch",
        ActionType.Case => "Case",
        ActionType.Default => "Default",
        ActionType.SwitchEnd => "End Switch",
        ActionType.ParallelStart => "Parallel Start",
        ActionType.ParallelEnd => "Parallel End",
        ActionType.ParallelBranch => "Branch",
        _ => "Unknown"
    };

    private static string GetDefaultDescription(ActionType type) => type switch
    {
        ActionType.MotorMoveAbsolute => "Move motor to absolute position",
        ActionType.MotorMoveRelative => "Move motor relative to current",
        ActionType.MotorHome => "Home motor",
        ActionType.MotorStop => "Stop motor",
        ActionType.WaitMotorDone => "Wait for motor done",
        ActionType.IoOutput => "Set IO output",
        ActionType.WaitIoInput => "Wait for IO input",
        ActionType.RobotMoveTo => "Move robot to point",
        ActionType.RobotRunProgram => "Run robot program",
        ActionType.Delay => "Delay execution",
        ActionType.LoopStart => "Start loop",
        ActionType.LoopEnd => "End loop",
        ActionType.Condition => "Conditional branch",
        ActionType.CallSubProgram => "Call sub-program",
        ActionType.SetVariable => "Set variable",
        ActionType.Log => "Log message",
        ActionType.Alarm => "Trigger alarm",
        // Logic Control Descriptions
        ActionType.IfStart => "If condition is true",
        ActionType.ElseIf => "Else if condition is true",
        ActionType.Else => "Otherwise execute",
        ActionType.IfEnd => "End of if block",
        ActionType.WhileStart => "While condition is true",
        ActionType.WhileEnd => "End of while loop",
        ActionType.Break => "Break out of loop",
        ActionType.Continue => "Continue to next iteration",
        ActionType.Return => "Return from program",
        ActionType.Switch => "Switch on value",
        ActionType.Case => "Case value match",
        ActionType.Default => "Default case",
        ActionType.SwitchEnd => "End of switch block",
        ActionType.ParallelStart => "Start parallel execution",
        ActionType.ParallelEnd => "End parallel execution",
        ActionType.ParallelBranch => "Parallel branch",
        _ => ""
    };
}
