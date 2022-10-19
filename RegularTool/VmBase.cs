using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace RegularTool
{
    public class VmBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public virtual void RaisePropertyChanged<T>(Expression<Func<T>> expression)
        {
            var propertyName = (expression.Body as MemberExpression).Member.Name;
            RaisePropertyChanged(propertyName);
        }
    }
}
