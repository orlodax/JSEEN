using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JSEEN.Classes
{
    public class Observable : INotifyPropertyChanged
    {
        /// <summary>
        /// Raised when the value of a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the property whose name matches <see cref="propertyName"/>.
        /// </summary>
        /// <param name="propertyName">Optional. The name of the property whose value has changed.</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //USE this in properties set not to spam boilerplate
        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
