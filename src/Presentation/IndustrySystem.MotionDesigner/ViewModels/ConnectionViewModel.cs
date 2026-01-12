using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using System.Windows;

namespace IndustrySystem.MotionDesigner.ViewModels
{

    public class ConnectionViewModel : BindableBase
    {
        private Guid _id;
        private Guid _sourceNodeId;
        private Guid _targetNodeId;
        private string _pathData = string.Empty;
        private string _arrowData = string.Empty;
        private double _arrowX;
        private double _arrowY;
        private double _arrowAngle;
        private int _executionOrder;
        private bool _isHighlighted;
        private int _sourcePortDirection;  // 0=Top, 1=Right, 2=Bottom, 3=Left
        private int _targetPortDirection;
        private Point _startPoint = new Point(0,0);
        private Point _endPoint = new Point(0,0);

        public Guid Id { get => _id; set => SetProperty(ref _id, value); }
        public Guid SourceNodeId { get => _sourceNodeId; set => SetProperty(ref _sourceNodeId, value); }
        public Guid TargetNodeId { get => _targetNodeId; set => SetProperty(ref _targetNodeId, value); }
        public string PathData { get => _pathData; set => SetProperty(ref _pathData, value); }
        public string ArrowData { get => _arrowData; set => SetProperty(ref _arrowData, value); }
        public double ArrowX { get => _arrowX; set => SetProperty(ref _arrowX, value); }
        public double ArrowY { get => _arrowY; set => SetProperty(ref _arrowY, value); }
        public double ArrowAngle { get => _arrowAngle; set => SetProperty(ref _arrowAngle, value); }
        public int ExecutionOrder { get => _executionOrder; set => SetProperty(ref _executionOrder, value); }
        public bool IsHighlighted { get => _isHighlighted; set => SetProperty(ref _isHighlighted, value); }
        public int SourcePortDirection { get => _sourcePortDirection; set => SetProperty(ref _sourcePortDirection, value); }
        public int TargetPortDirection { get => _targetPortDirection; set => SetProperty(ref _targetPortDirection, value); }

        // New: StartPoint/EndPoint for binding to connection visuals
        public Point StartPoint { get => _startPoint; set => SetProperty(ref _startPoint, value); }
        public Point EndPoint { get => _endPoint; set => SetProperty(ref _endPoint, value); }
    }
}
