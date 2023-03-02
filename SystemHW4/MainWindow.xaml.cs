using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SystemHW4;

public partial class MainWindow : Window
{
    private int elapsedTime;
    ObservableCollection<Car> Cars { get; set; }
    CancellationTokenSource token;
    DispatcherTimer timer = new DispatcherTimer();

    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;
        Cars = new();
        CarsList.ItemsSource = Cars;

        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
    }

    private void StartBtn_Click(object sender, RoutedEventArgs e)
    {
        timer.Start();
        token = new();
        if (rBtn_Single.IsChecked == false)
            MultiCars(token.Token);
        else
            SingleCars(token.Token);
    }

    private void btn_CancelClick(object sender, RoutedEventArgs e)
    {
        timer.Stop();
        token.Cancel();
    }

    private void rBtn_Multi_Checked(object sender, RoutedEventArgs e) => rBtn_Single.IsChecked = false;

    private void rBtn_Single_Checked(object sender, RoutedEventArgs e)
    {
        if (rBtn_Single.IsChecked == true)
            rBtn_Multii.IsChecked = false;
    }

    private void SingleCars(CancellationToken token)
    {
        Cars.Clear();
        
        new Thread(() =>
        {
            var directory = new DirectoryInfo(@"..\..\..\Datas");
            foreach (var file in directory.GetFiles())
            {

                if (file.Extension == ".json")
                {
                    var text = File.ReadAllText(file.FullName);

                    var carlist = JsonSerializer.Deserialize<List<Car>>(text);

                    if (carlist is not null)
                        foreach (var car in carlist)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            Dispatcher.Invoke(() => Cars?.Add(car));
                            Dispatcher.Invoke(() => { btn_Start.IsEnabled = false; });
                            Thread.Sleep(200);
                        }
                }
            }
            timer.Stop();
        }).Start();
    }

    private void MultiCars(CancellationToken token)
    {
        Cars.Clear();
        var sync = new object();
        var dirInfo = new DirectoryInfo(@"..\..\..\Datas");
        foreach (var file in dirInfo.GetFiles())
        {
            if (file.Extension == ".json")
            {
                ThreadPool.QueueUserWorkItem(s =>
                {
                    var text = File.ReadAllText(file.FullName);
                    var carsList = JsonSerializer.Deserialize<List<Car>>(text);
                    if (carsList is not null)
                    {
                        foreach (var car in carsList)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }
                            lock (sync)
                                Dispatcher.Invoke(() => Cars?.Add(car));

                            Dispatcher.Invoke(() => { btn_Start.IsEnabled = false; });
                            Thread.Sleep(100);
                        }
                    }
                });
            }

        }
        timer.Stop();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        elapsedTime++;
        tBox_Timer.Text = elapsedTime.ToString();
    }
}
