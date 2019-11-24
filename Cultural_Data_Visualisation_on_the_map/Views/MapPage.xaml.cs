using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Cultural_Data_Visualisation_on_the_map.Helpers;
using Cultural_Data_Visualisation_on_the_map.Services;

using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using QuickType;
using System.IO;

namespace Cultural_Data_Visualisation_on_the_map.Views
{
    public sealed partial class MapPage : Page, INotifyPropertyChanged
    {
        // TODO WTS: Set your preferred default zoom level
        private const double DefaultZoomLevel = 11;

        private readonly LocationService _locationService;

        // TODO WTS: Set your preferred default location if a geolock can't be found.
        private readonly BasicGeoposition _defaultPosition = new BasicGeoposition()
        {
            Latitude = 59.945225,
            Longitude = 30.3417
        };

        private double _zoomLevel;

        Welcome welcome;

        //ObservableCollection<string> list_of_persons;
        

        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set { Set(ref _zoomLevel, value); }
        }

        private Geopoint _center;

        public Geopoint Center
        {
            get { return _center; }
            set { Set(ref _center, value); }
        }

        PersonElement[] list_of_persons;
        IOrderedEnumerable<PersonElement> list_of_persons_sorted;
        Dictionary<string, Relation[]> coordinates;

        public MapPage()
        {
            Load_from_JSON();
            list_of_persons = welcome.Data.Persons;

            coordinates = new Dictionary<string, Relation[]>();

            foreach (PersonElement i in list_of_persons) {
                if (i.LastName.Ru != null)
                {
                    coordinates[i.LastName.Ru] = i.Relations;
                }
            }






            MapService.ServiceToken = "3ZD2VwJ9BPgSIZp42L4D~OXXDDKVQk9WYpGS42iLobg~AgoKqlQffKZXFrAED3UJc0sncgcqrtiUBXHHYIU9E6dtovz-Hpyl4Bfff5WyiaSi";
            _locationService = new LocationService();
            Center = new Geopoint(_defaultPosition);
            ZoomLevel = DefaultZoomLevel;
            InitializeComponent();

            /*list_of_persons_sorted = from u in list_of_persons
                                     orderby u.LastName.Ru
                                     select u;*/
            Persons_List.ItemsSource = list_of_persons;

            Persons_List.DisplayMemberPath = "LastName.Ru";
            Persons_List.SelectedValuePath = "LastName.Ru";


            //list_of_persons = new ObservableCollection<string>();




        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Cleanup();
        }

        public async Task InitializeAsync()
        {
            if (_locationService != null)
            {
                _locationService.PositionChanged += LocationService_PositionChanged;

                var initializationSuccessful = await _locationService.InitializeAsync();

                if (initializationSuccessful)
                {
                    await _locationService.StartListeningAsync();
                }

                if (initializationSuccessful && _locationService.CurrentPosition != null)
                {
                    Center = _locationService.CurrentPosition.Coordinate.Point;
                }
                else
                {
                    Center = new Geopoint(_defaultPosition);
                }
            }

            if (mapControl != null)
            {
                // TODO WTS: Set your map service token. If you don't have one, request from https://www.bingmapsportal.com/
                //MapService.ServiceToken = "3ZD2VwJ9BPgSIZp42L4D~OXXDDKVQk9WYpGS42iLobg~AgoKqlQffKZXFrAED3UJc0sncgcqrtiUBXHHYIU9E6dtovz-Hpyl4Bfff5WyiaSi";

                //AddMapIcon(Center, "Map_YourLocation".GetLocalized());
            }
        }

        public void Cleanup()
        {
            if (_locationService != null)
            {
                _locationService.PositionChanged -= LocationService_PositionChanged;
                _locationService.StopListening();
            }
        }

        private void LocationService_PositionChanged(object sender, Geoposition geoposition)
        {
            if (geoposition != null)
            {
                Center = geoposition.Coordinate.Point;
            }
        }

        private void AddMapIcon(Geopoint position, string title)
        {
            MapIcon mapIcon = new MapIcon()
            {
                Location = position,
                NormalizedAnchorPoint = new Point(0.5, 1.0),
                Title = title,
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/map.png")),
                ZIndex = 0
            };
            mapControl.MapElements.Add(mapIcon);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*List<List<double>> coords = new List<List<double>>();
            foreach (var i in Persons_List.SelectedItem.Relations) {
                coords[0] = i.Location.CoordinateX;
                coords[1] = i.Location.CoordinateX;
            }*/
            
            

            
            //PersonElement c = Persons_List.SelectedItem;
            if (Persons_List.SelectedValue != null)
            {
                string name = Persons_List.SelectedValue.ToString();
                foreach (Relation i in coordinates[name]) {
                   // if(i == null)
                    BasicGeoposition Position = new BasicGeoposition()
                    {
                        Latitude = i.Location.CoordinateX.Value,
                        Longitude = i.Location.CoordinateY.Value
                    };
                    Geopoint A = new Geopoint(Position);
                    if(i.Quote.Ru != null)
                    AddMapIcon(A, i.Quote.Ru.ToString());
                    else
                        AddMapIcon(A, "Описание отсутствует");

                }





                
            }
                
            
        }

       

        private void suggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent())
            {
                var term = suggestBox.Text.ToLower();
                
                var results = list_of_persons.Where(i => i.LastName.Ru != null &&
                                                i.LastName.Ru.ToLower().Contains(term)).ToList();
                suggestBox.ItemsSource = results;
                suggestBox.DisplayMemberPath = "LastName.Ru";
            }
        }

        private void suggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (Persons_List.SelectedValue != null)
            {
                suggestBox.DisplayMemberPath = "LastName.Ru";
                suggestBox.TextMemberPath = "LastName.Ru";
                suggestBox.Text = args.SelectedItem  .ToString();
                string name = suggestBox.Text;
                foreach (Relation i in coordinates[name])
                {
                    // if(i == null)
                    BasicGeoposition Position = new BasicGeoposition()
                    {
                        Latitude = i.Location.CoordinateX.Value,
                        Longitude = i.Location.CoordinateY.Value
                    };
                    Geopoint A = new Geopoint(Position);
                    if (i.Quote.Ru != null)
                        AddMapIcon(A, i.Quote.Ru.ToString());
                    else
                        AddMapIcon(A, "Описание отсутствует");

                }
            }
           // AddMapIcon(A, args.SelectedItem as string);
        }

        private void suggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            /*var term = args.QueryText.ToLower();
            var results = phones.Where(i => i.ToLower().Contains(term)).ToList();
            suggestBox.ItemsSource = results;
            suggestBox.IsSuggestionListOpen = true;*/


            BasicGeoposition Position = new BasicGeoposition()
            {
                Latitude = 59.945225,
                Longitude = 30.3417
            };

            Geopoint A = new Geopoint(Position);
            AddMapIcon(A, args.QueryText as string);
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void Name_textBlock_Loaded(object sender, RoutedEventArgs e)
        {
           Name_textBox.Text =  Persons_List.SelectedValue.ToString();
        }

        private void Description_textBlock_Loaded(object sender, RoutedEventArgs e)
        {
            Description_textBox.Text = Persons_List.SelectedValue.ToString();
        }

        private void Persons_List_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void suggestBox_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void Name_textBox2_Loaded(object sender, RoutedEventArgs e)
        {
            Name_textBox2.Text = suggestBox.Text;
        }

        private void Description_textBox2_Loaded(object sender, RoutedEventArgs e)
        {
            Description_textBox2.Text = suggestBox.Text;
        }

        private void Dates_textBox_Loaded(object sender, RoutedEventArgs e)
        {
            Dates_textBox.Text = Persons_List.SelectedValue.ToString();
        }

        private void Dates_textBox2_Loaded(object sender, RoutedEventArgs e)
        {
            Dates_textBox2.Text = suggestBox.Text;
        }

        private void Clear_button_Click(object sender, RoutedEventArgs e)
        {
            mapControl.MapElements.Clear();
        }


        private void Load_from_JSON()
        {
            string jsonString = File.ReadAllText("zhepa.json");
            welcome = Welcome.FromJson(jsonString);
            //Console.WriteLine(welcome.Data.Persons[0].FirstName.Ru);
        }
    }
}
