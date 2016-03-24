using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace SourceCodeIndexer.UI.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies property changed
        /// </summary>
        /// <param name="propertyLambdaExpression">lamda for property</param>
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> propertyLambdaExpression)
        {
            MemberInfo memberInfo = ((MemberExpression)propertyLambdaExpression.Body).Member;
            NotifyPropertyChanged(memberInfo.Name);
        }

        /// <summary>
        /// Notifies property changed
        /// </summary>
        /// <param name="name">name of property</param>
        protected void NotifyPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
