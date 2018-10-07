using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Emux.GameBoy.Cpu;
using Emux.OpenTK.Emux.Expressions;

namespace Emux.OpenTK.Emux
{
    public class BreakpointInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _conditionString;

        public BreakpointInfo(Breakpoint breakpoint)
        {
            Breakpoint = breakpoint ?? throw new ArgumentNullException(nameof(breakpoint));
        }

        public Breakpoint Breakpoint
        {
            get;
        }

        public ushort Address
        {
            get { return Breakpoint.Offset; }
        }

        public string ConditionString
        {
            get { return _conditionString; }
            set
            {
                if (_conditionString != value)
                {
                    Breakpoint.Condition = string.IsNullOrEmpty(value)
                        ? Breakpoint.BreakAlways
                        : ExpressionParser.CompileExpression(value);

                    _conditionString = value;
                    OnPropertyChanged(nameof(ConditionString));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
