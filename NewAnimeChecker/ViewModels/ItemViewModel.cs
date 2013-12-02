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
        private int _num;
        public int num
        {
            get
            {
                return _num;
            }
            set
            {
                if (value != _num)
                {
                    _num = value;
                    NotifyPropertyChanged("num");
                }
            }
        }

        private string _aid;
        public string aid
        {
            get
            {
                return _aid;
            }
            set
            {
                if (value != _aid)
                {
                    _aid = value;
                    NotifyPropertyChanged("aid");
                }
            }
        }

        private string _name;
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != name)
                {
                    _name = value;
                    NotifyPropertyChanged("name");
                }
            }
        }

        private string _status;
        public string status
        {
            get
            {
                return _status;
            }
            set
            {
                if (value != status)
                {
                    _status = value;
                    NotifyPropertyChanged("status");
                }
            }
        }

        private string _epi;
        public string epi
        {
            get
            {
                return _epi;
            }
            set
            {
                if (value != _epi)
                {
                    _epi = value;
                    NotifyPropertyChanged("epi");
                }
            }
        }

        private string _read;
        public string read
        {
            get
            {
                return _read;
            }
            set
            {
                if (value != _read)
                {
                    _read = value;
                    NotifyPropertyChanged("read");
                }
            }
        }

        private string _readText;
        public string readText
        {
            get
            {
                return _readText;
            }
            set
            {
                if (value != _readText)
                {
                    _readText = value;
                    NotifyPropertyChanged("readText");
                }
            }
        }

        private string _highlight;
        public string highlight
        {
            get
            {
                return _highlight;
            }
            set
            {
                if (value != _highlight)
                {
                    _highlight = value;
                    NotifyPropertyChanged("highlight");
                }
            }
        }

        private string _text;
        public string text
        {
            get
            {
                return _text;
            }
            set
            {
                if (value != _text)
                {
                    _text = value;
                    NotifyPropertyChanged("text");
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
        private int _num;
        public int num
        {
            get
            {
                return _num;
            }
            set
            {
                if (value != _num)
                {
                    _num = value;
                    NotifyPropertyChanged("num");
                }
            }
        }

        private string _aid;
        public string aid
        {
            get
            {
                return _aid;
            }
            set
            {
                if (value != _aid)
                {
                    _aid = value;
                    NotifyPropertyChanged("aid");
                }
            }
        }

        private string _name;
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("name");
                }
            }
        }

        private string _time;
        public string time
        {
            get
            {
                return _time;
            }
            set
            {
                if (value != _time)
                {
                    _time = value;
                    NotifyPropertyChanged("time");
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
        private int _Number;
        public int Number
        {
            get
            {
                return _Number;
            }
            set
            {
                if (value != _Number)
                {
                    _Number = value;
                    NotifyPropertyChanged("Number");
                }
            }
        }

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