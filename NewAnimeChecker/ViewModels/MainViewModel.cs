using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NewAnimeChecker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.SubscriptionItems = new ObservableCollection<SubscriptionModel>();
            this.ScheduleItems = new ObservableCollection<ScheduleModel>();
            this.SearchResultItems = new ObservableCollection<SearchResultModel>();
        }

        public ObservableCollection<SubscriptionModel> SubscriptionItems { get; private set; }
        public ObservableCollection<ScheduleModel> ScheduleItems { get; private set; }
        public ObservableCollection<SearchResultModel> SearchResultItems { get; private set; }


        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public void LoadData()
        {
            this.IsDataLoaded = true;
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
}