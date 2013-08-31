using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NewAnimeChecker.ViewModels
{
    #region SubscriptionModel
    public class SubscriptionModel : INotifyPropertyChanged
    {
        private string _ID;
        public string ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (value != _ID)
                {
                    _ID = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _Epi;
        public string Epi
        {
            get
            {
                return _Epi;
            }
            set
            {
                if (value != _Epi)
                {
                    _Epi = value;
                    NotifyPropertyChanged("Epi");
                }
            }
        }

        private System.Windows.Visibility _Updated;
        public System.Windows.Visibility Updated
        {
            get
            {
                return _Updated;
            }
            set
            {
                if (value != _Updated)
                {
                    _Updated = value;
                    NotifyPropertyChanged("Updated");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    #endregion

    #region ScheduleModel
    public class ScheduleModel : INotifyPropertyChanged
    {
        private string _ID;
        public string ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (value != _ID)
                {
                    _ID = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _Website;
        public string Website
        {
            get
            {
                return _Website;
            }
            set
            {
                if (value != _Website)
                {
                    _Website = value;
                    NotifyPropertyChanged("Website");
                }
            }
        }

        private string _Time;
        public string Time
        {
            get
            {
                return _Time;
            }
            set
            {
                if (value != _Time)
                {
                    _Time = value;
                    NotifyPropertyChanged("Time");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    #endregion

    #region SearchResultModel
    public class SearchResultModel : INotifyPropertyChanged
    {
        private string _ID;
        public string ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (value != _ID)
                {
                    _ID = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _Type;
        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    NotifyPropertyChanged("Type");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    #endregion
}