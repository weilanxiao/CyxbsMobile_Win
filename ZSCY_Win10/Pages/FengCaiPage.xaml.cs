using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.UI.Core;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Animation;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml.Media.Imaging;
using ZSCY_Win10;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using System.Text;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace ZSCY.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FengCaiPage : Page
    {
        private int pivot_index;
        private int zuzhi_listview_index;
        private double[] pivotitem1_ver_offest;
        private ZSCY_Win10.ViewModels.FengCaiViewModel viewmodel;
        public static FengCaiPage fengcaipage;

        List<string> zuzhilist = new List<string>();

        public FengCaiPage()
        {
            this.InitializeComponent();
            pivot_index = 0;
            zuzhi_listview_index = 0;

            viewmodel = new ZSCY_Win10.ViewModels.FengCaiViewModel();
            this.DataContext = viewmodel;
            fengcaipage = this;
            this.SizeChanged += FengCaiPage_SizeChanged;

            //手机物理返回键订阅事件
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += OnBackPressed;
            }
            else
            {
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += PC_BackRequested;
            }
        }
        private void PC_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            FirstPage.firstpage.Second_Page_Back();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;

        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                await First_Step();
                pivotitem1_ver_offest = new double[viewmodel.ZuZhi.Count];
                Dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PivotItem1_Add_Content(1);
                });
                await Task.Delay(100);
                //zuzhi_listview.SelectedIndex = pivot.SelectedIndex = 0;
            }
        }

        private void FengCaiPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            viewmodel.Page_Width = e.NewSize.Width;
            viewmodel.Page_Height = e.NewSize.Height;
        }

        private async Task First_Step()
        {
            StorageFile file;
            string json = "";
            JObject json_object;

            #region 得到Header列表
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Json/fengcai_header_lists.json", UriKind.Absolute));
            json = await FileIO.ReadTextAsync(file);
            json_object = (JObject)JsonConvert.DeserializeObject(json);
            JArray headers = (JArray)json_object["header_lists"];
            ObservableCollection<Models.fengcaiheaders> header_lists = new ObservableCollection<Models.fengcaiheaders>();
            for (int i = 0; i < headers.Count; i++)
            {
                Models.fengcaiheaders item = new Models.fengcaiheaders();
                item.header = headers[i]["header"].ToString();
                header_lists.Add(item);
            }
            viewmodel.Header = header_lists;
            #endregion

            #region 得到组织列表
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Json/fengcai_zuzhi_lists.json", UriKind.Absolute));
            json = await FileIO.ReadTextAsync(file);
            json_object = (JObject)JsonConvert.DeserializeObject(json);
            JArray zuzhis = (JArray)json_object["zuzhi_lists"];
            ObservableCollection<Models.zuzhi> zuzhi_lists = new ObservableCollection<Models.zuzhi>();
            for (int i = 0; i < zuzhis.Count; i++)
            {
                Models.zuzhi item = new Models.zuzhi();
                item.zuzhi_name = zuzhis[i]["zuzhi"].ToString();
                zuzhi_lists.Add(item);
            }
            viewmodel.ZuZhi = zuzhi_lists;
            #endregion

            #region 得到组织介绍
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Json/fengcai_zuzhi_intro.json", UriKind.Absolute));
            json = await FileIO.ReadTextAsync(file);
            json_object = (JObject)JsonConvert.DeserializeObject(json);
            JArray zuzhi_intros = (JArray)json_object["zuzhi_intro"];
            ObservableCollection<Models.zuzhi_intro> intro_lists = new ObservableCollection<Models.zuzhi_intro>();
            StringBuilder sb = new StringBuilder("");
            for (int i = 0; i < zuzhi_intros.Count; i++)
            {
                Models.zuzhi_intro item = new Models.zuzhi_intro();
                item.zuzhi = new ObservableCollection<string>();
                JArray zuzhi_item = (JArray)zuzhi_intros[i]["zuzhi"];
                for (int j = 0; j < zuzhi_item.Count; j++)
                {
                    //item.zuzhi.Add(zuzhi_item[j]["duanluo"].ToString());
                    sb.Append(zuzhi_item[j]["duanluo"].ToString());
                    sb.Append("\n");
                }
                //intro_lists.Add(item);
                zuzhilist.Add(sb.ToString());
                sb.Clear();
            }
            //viewmodel.Zuzhi_Intro = intro_lists;
            #endregion

            #region 得到原创重邮内容
            json = await ZSCY_Win10.Util.Request.YuanChuang_Request();
            if (json != null)
            {
                json_object = (JObject)JsonConvert.DeserializeObject(json);
                JArray data = (JArray)json_object["data"];
                ObservableCollection<Models.yuanchuang> yc_lists = new ObservableCollection<Models.yuanchuang>();
                for (int i = 0; i < data.Count; i++)
                {
                    Models.yuanchuang item = new Models.yuanchuang();
                    item.introduction = data[i]["introduction"].ToString();
                    item.name = data[i]["name"].ToString();
                    item.time = data[i]["time"].ToString();
                    item.video_url = data[i]["video_url"].ToString();
                    JArray photo = (JArray)data[i]["photo"];
                    for (int j = 0; j < photo.Count; j++)
                    {
                        item.photo_src = photo[j]["photo_src"].ToString();
                    }
                    yc_lists.Add(item);
                }
                viewmodel.YuanChuang = yc_lists;
            }
            #endregion

            #region 得到最美重邮文字内容
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Json/fengcai_zuimei.json", UriKind.Absolute));
            json = await FileIO.ReadTextAsync(file);
            json_object = (JObject)JsonConvert.DeserializeObject(json);
            JArray zuimei = (JArray)json_object["zuimei"];
            ObservableCollection<string> zuimei_lists = new ObservableCollection<string>();
            for (int i = 0; i < zuimei.Count; i++)
            {
                string content = zuimei[i]["content"].ToString();
                zuimei_lists.Add(content);
            }
            viewmodel.ZuiMei = zuimei_lists;
            #endregion

            #region 得到最美重邮图片
            json = await ZSCY_Win10.Util.Request.ZuiMei_Request();
            if (json != null)
            {
                json_object = (JObject)JsonConvert.DeserializeObject(json);
                JArray data = (JArray)json_object["data"];
                zuimei_lists = new ObservableCollection<string>();
                for (int i = 0; i < data.Count; i++)
                {
                    JArray photo = (JArray)data[i]["photo"];
                    zuimei_lists.Add(photo[0]["photo_src"].ToString());
                }
                viewmodel.ZuiMei_Photos = zuimei_lists;
            }
            #endregion

            #region 得到优秀学子内容
            json = await ZSCY_Win10.Util.Request.XueZi_Request();
            if (json != null)
            {
                json_object = (JObject)JsonConvert.DeserializeObject(json);
                JArray data = (JArray)json_object["data"];
                ObservableCollection<Models.xuezi> xuezi_lists = new ObservableCollection<Models.xuezi>();
                for (int i = 0; i < data.Count; i++)
                {
                    Models.xuezi item = new Models.xuezi();
                    item.description = data[i]["description"].ToString();
                    item.introduction = data[i]["introduction"].ToString();
                    item.name = data[i]["name"].ToString();
                    JArray photo = (JArray)data[i]["photo"];
                    item.photo_src = photo[0]["photo_src"].ToString();
                    item.photo_thumbnail_src = photo[0]["photo_thumbnail_src"].ToString();
                    xuezi_lists.Add(item);
                }
                viewmodel.XueZi = xuezi_lists;
            }
            #endregion

            #region 得到优秀教师内容
            json = await ZSCY_Win10.Util.Request.Teather_Request();
            if (json != null)
            {
                json_object = (JObject)JsonConvert.DeserializeObject(json);
                JArray data = (JArray)json_object["data"];
                ObservableCollection<Models.teacher> teacher_lists = new ObservableCollection<Models.teacher>();
                for (int i = 0; i < data.Count; i++)
                {
                    Models.teacher item = new Models.teacher();
                    item.name = data[i]["name"].ToString();
                    item.college = data[i]["college"].ToString();
                    JArray photo = (JArray)data[i]["photo"];
                    item.photo_src = photo[0]["photo_src"].ToString();
                    item.photo_thumbnail_src = photo[0]["photo_thumbnail_src"].ToString();
                    teacher_lists.Add(item);
                }
                viewmodel.Teacher = teacher_lists;
            }
            #endregion
        }

        private void PivotItem1_Add_Content(int p)
        {
            //zuzhi_content.Children.Clear();
            //if (p == 1)
            //{
            //    for (int i = 0; i < viewmodel.Zuzhi_Intro[0].zuzhi.Count; i++)
            //    {
            //        if (viewmodel.Zuzhi_Intro[0].zuzhi[i].Contains("【"))
            //        {
            //            zuzhi_content.Children.Add(New_TextBlock(1, viewmodel.Zuzhi_Intro[0].zuzhi[i]));
            //        }
            //        else
            //        {
            //            zuzhi_content.Children.Add(New_TextBlock(2, viewmodel.Zuzhi_Intro[0].zuzhi[i]));
            //        }
            //    }
            //}
            //else if (p == 2)
            //{
            //    for (int i = 0; i < viewmodel.Zuzhi_Intro[zuzhi_listview.SelectedIndex].zuzhi.Count; i++)
            //    {
            //        if (viewmodel.Zuzhi_Intro[zuzhi_listview.SelectedIndex].zuzhi[i].Contains("【"))
            //        {
            //            zuzhi_content.Children.Add(New_TextBlock(1, viewmodel.Zuzhi_Intro[zuzhi_listview.SelectedIndex].zuzhi[i]));
            //        }
            //        else
            //        {
            //            zuzhi_content.Children.Add(New_TextBlock(2, viewmodel.Zuzhi_Intro[zuzhi_listview.SelectedIndex].zuzhi[i]));
            //        }
            //    }
            //}
        }

        private TextBlock New_TextBlock(int p, string content)
        {
            TextBlock tb = new TextBlock();
            switch (p)
            {
                case 1: //较重标题
                    {
                        tb.Text = content.Substring(1, (content.LastIndexOf('】') - content.IndexOf('【') - 1));
                        //tb.Foreground = App.APPTheme.Content_Header_Color_Brush;
                        tb.FontSize = 16;
                        tb.Margin = new Thickness(0, 3, 0, 8);
                    }; break;
                case 2: //普通内容
                    {
                        tb.Text = content;
                        FontWeight weight = new FontWeight();
                        weight.Weight = 10;
                        tb.FontWeight = weight;
                        //tb.Foreground = App.APPTheme.Gary_Color_Brush;
                        tb.FontSize = 15;
                        tb.LineHeight = 26;
                    }; break;
            }
            tb.CharacterSpacing = 100;
            tb.TextWrapping = TextWrapping.Wrap;
            return tb;
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            //    if (pivot.SelectedIndex < 0)
            //    {
            //        pivot.SelectedIndex = pivot_index = 0;
            //    }
            //    (((pivot.Items[pivot_index] as PivotItem).Header as Grid).Children[0] as TextBlock).Foreground = App.APPTheme.Content_Header_Color_Brush;
            //    (((pivot.Items[pivot_index] as PivotItem).Header as Grid).Children[1] as Line).Visibility = Visibility.Collapsed;
            //    pivot_index = pivot.SelectedIndex;
            //    (((pivot.Items[pivot_index] as PivotItem).Header as Grid).Children[0] as TextBlock).Foreground = App.APPTheme.APP_Color_Brush;
            //    (((pivot.Items[pivot_index] as PivotItem).Header as Grid).Children[1] as Line).Visibility = Visibility.Visible;
            //}
            //catch (Exception)
            //{
            //    return;
            //}
        }

        private void zuzhi_listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //pivotitem1_ver_offest[zuzhi_listview_index] = zuzhi_sc.VerticalOffset;
            //PivotItem1_Add_Content(2);
            //if (pivotitem1_ver_offest[zuzhi_listview.SelectedIndex] != 0.0)
            //{
            //    zuzhi_sc.ChangeView(null, pivotitem1_ver_offest[zuzhi_listview.SelectedIndex], null, true);
            //}
            //else
            //{
            //    zuzhi_sc.ChangeView(null, 0.0, null, true);
            //}
            //zuzhi_listview_index = zuzhi_listview.SelectedIndex;
        }

        private async void yc_listview_ItemClick(object sender, ItemClickEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri((e.ClickedItem as Models.yuanchuang).video_url));
        }

        private void XueZi_Rectangle_Loaded(object sender, RoutedEventArgs e)
        {
            Binding binding1 = new Binding();
            binding1.Source = viewmodel;
            binding1.Path = new PropertyPath("XueZi_Height");
            (sender as Rectangle).SetBinding(Rectangle.HeightProperty, binding1);
            Binding binding2 = new Binding();
            binding2.Source = viewmodel;
            binding2.Path = new PropertyPath("XueZi_Width");
            (sender as Rectangle).SetBinding(Rectangle.WidthProperty, binding2);
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //if (e.ClickedItem is Models.xuezi)
            //{
            //    detail_img.ImageSource = new BitmapImage(new Uri((e.ClickedItem as Models.xuezi).photo_thumbnail_src, UriKind.Absolute));
            //    detail_title.Text = (e.ClickedItem as Models.xuezi).name;
            //    detail_content.Text = (e.ClickedItem as Models.xuezi).introduction;
            //}
            //else if (e.ClickedItem is Models.teacher)
            //{
            //    detail_img.ImageSource = new BitmapImage(new Uri((e.ClickedItem as Models.teacher).photo_thumbnail_src, UriKind.Absolute));
            //    detail_title.Text = (e.ClickedItem as Models.teacher).name;
            //    detail_content.Text = (e.ClickedItem as Models.teacher).college;
            //}
            //detail_sc.ChangeView(null, 0, null, true);
            //black_background.Visibility = Visibility.Visible;
            //black_background_sb.Begin();
            //detail_popup.IsOpen = true;
        }

        private void detail_popup_Closed(object sender, object e)
        {
            //detail_img.ImageSource = null;
            //detail_title.Text = "";
            //detail_content.Text = "";
            //black_background.Visibility = Visibility.Collapsed;
        }

        private void back_but_Click(object sender, RoutedEventArgs e)
        {
            FirstPage.firstpage.Second_Page_Back();
        }

        bool isExit = false;
        private void OnBackPressed(object sender, BackPressedEventArgs e)
        {
            FirstPage.firstpage.Second_Page_Back();
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                return;
            }//Frame内无内容
            if (rootFrame.CurrentSourcePageType.Name != "MainPage")
            {
                if (rootFrame.CanGoBack && e.Handled == false)
                {
                    e.Handled = true;
                    rootFrame.GoBack();
                }
            }//Frame不在MainPage页面并且可以返回则返回上一个页面并且事件未处理
            else if (e.Handled == false)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ShowAsync();
                statusBar.ProgressIndicator.Text = "再按一次返回键即将退出程序 ~\\(≧▽≦)/~"; // 状态栏显示文本
                statusBar.ProgressIndicator.ShowAsync();

                if (isExit)
                {
                    App.Current.Exit();
                }
                else
                {
                    isExit = true;
                    Task.Run(async () =>
                    {
                        await Task.Delay(1500);
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                        {
                            await statusBar.ProgressIndicator.HideAsync();
                            await statusBar.ShowAsync(); //此处不隐藏状态栏
                        });
                        isExit = false;
                    });
                    e.Handled = true;
                }//Frame在其他页面并且事件未处理
            }
        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("你点击了！");
            var x = e.ClickedItem as Models.fengcaiheaders;
            if (x.header == "学生组织")
            {
                ////StackPanel sp = (StackPanel)VisualTreeHelper.GetChild(navigation.LoadContent(), 0);
                //DataTemplate dt = (DataTemplate)VisualTreeHelper.GetChild(, 0);
                //StackPanel sp = (StackPanel)VisualTreeHelper.GetChild(dt, 0);
                //Line li = (Line)VisualTreeHelper.GetChild(sp, 1);
                //li.Visibility = Visibility.Visible;
                Students_.Visibility = Visibility.Visible;
                Original_.Visibility = Visibility.Collapsed;
                Beauty_.Visibility = Visibility.Collapsed;
                Teacher_.Visibility = Visibility.Collapsed;
                Student_.Visibility = Visibility.Collapsed;
                xuhua.Visibility = Visibility.Collapsed;

            }
            else if (x.header == "原创重邮")
            {
                Students_.Visibility = Visibility.Collapsed;
                Original_.Visibility = Visibility.Visible;
                Beauty_.Visibility = Visibility.Collapsed;
                Teacher_.Visibility = Visibility.Collapsed;
                Student_.Visibility = Visibility.Collapsed;
                xuhua.Visibility = Visibility.Collapsed;
            }
            else if (x.header == "美在重邮")
            {
                Students_.Visibility = Visibility.Collapsed;
                Original_.Visibility = Visibility.Collapsed;
                Beauty_.Visibility = Visibility.Visible;
                Teacher_.Visibility = Visibility.Collapsed;
                Student_.Visibility = Visibility.Collapsed;
                xuhua.Visibility = Visibility.Collapsed;
            }
            else if (x.header == "教师代表")
            {
                Students_.Visibility = Visibility.Collapsed;
                Original_.Visibility = Visibility.Collapsed;
                Beauty_.Visibility = Visibility.Collapsed;
                Teacher_.Visibility = Visibility.Visible;
                Student_.Visibility = Visibility.Collapsed;
                xuhua.Visibility = Visibility.Collapsed;
                teacherList.IsItemClickEnabled = true;
            }
            else if (x.header == "学生代表")
            {
                Students_.Visibility = Visibility.Collapsed;
                Original_.Visibility = Visibility.Collapsed;
                Beauty_.Visibility = Visibility.Collapsed;
                Teacher_.Visibility = Visibility.Collapsed;
                Student_.Visibility = Visibility.Visible;
                xuhua.Visibility = Visibility.Collapsed;
                studentList.IsItemClickEnabled = true;
            }
        }       

        Rectangle rect_old;
        Rectangle rect_new;
        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = 0;
            for (int i = 0; i < listView.Items.Count; i++)
            {
                if (listView.ContainerFromIndex(i) != null)
                {
                    var grid = (listView.ContainerFromIndex(i) as ListViewItem).ContentTemplateRoot as Grid;
                    var line = grid.FindName("line") as Line;                    

                    if (listView.SelectedIndex == i) // 当前选中项
                    {
                        index = i;
                        //(grid.FindName("namelist") as TextBlock).Foreground= App.APPTheme.APP_Color_Brush;
                        //rect_old = rect_new;
                        //rect_new = rect;                       
                    }
                    line.Visibility = listView.SelectedIndex == i ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            Debug.WriteLine("你点击了！");
            switch (index)
            {
                case 0:
                    Students_.Visibility = Visibility.Visible;
                    Original_.Visibility = Visibility.Collapsed;
                    Beauty_.Visibility = Visibility.Collapsed;
                    Teacher_.Visibility = Visibility.Collapsed;
                    Student_.Visibility = Visibility.Collapsed;
                    xuhua.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    Students_.Visibility = Visibility.Collapsed;
                    Original_.Visibility = Visibility.Visible;
                    Beauty_.Visibility = Visibility.Collapsed;
                    Teacher_.Visibility = Visibility.Collapsed;
                    Student_.Visibility = Visibility.Collapsed;
                    xuhua.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    Students_.Visibility = Visibility.Collapsed;
                    Original_.Visibility = Visibility.Collapsed;
                    Beauty_.Visibility = Visibility.Visible;
                    Teacher_.Visibility = Visibility.Collapsed;
                    Student_.Visibility = Visibility.Collapsed;
                    fangdasv.Visibility = Visibility.Collapsed;
                    fangdaImage.Visibility = Visibility.Collapsed;
                    xuhua.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    Students_.Visibility = Visibility.Collapsed;
                    Original_.Visibility = Visibility.Collapsed;
                    Beauty_.Visibility = Visibility.Collapsed;
                    Teacher_.Visibility = Visibility.Collapsed;
                    Student_.Visibility = Visibility.Visible;
                    xuhua.Visibility = Visibility.Collapsed;
                    studentList.IsItemClickEnabled = true;
                    break;                    
                case 4:
                    Students_.Visibility = Visibility.Collapsed;
                    Original_.Visibility = Visibility.Collapsed;
                    Beauty_.Visibility = Visibility.Collapsed;
                    Teacher_.Visibility = Visibility.Visible;
                    Student_.Visibility = Visibility.Collapsed;
                    xuhua.Visibility = Visibility.Collapsed;
                    teacherList.IsItemClickEnabled = true;
                    break;
            }
        }
        private void studentsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            //studentImage.Source = new BitmapImage(new Uri(@"ms-appx:///Assets.123.png"));
        }

        private void zuzhi_listview_ItemClick(object sender, ItemClickEventArgs e)
        {
            int index = zuzhi_listview.SelectedIndex;
            switch (index)
            {
                case 0:
                    zuzhiIntro.Text = zuzhilist[0];
                    break;
            }
        }
        private void initializeFrostedGlass(UIElement glassHost)//虚化方法
        {
            //初始化模糊效果
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(glassHost);
            Compositor compositor = hostVisual.Compositor;
            var glassEffect = new GaussianBlurEffect
            {
                BlurAmount = 15.0f,
                BorderMode = EffectBorderMode.Hard,
                Source = new ArithmeticCompositeEffect
                {
                    MultiplyAmount = 0,
                    Source1Amount = 0.5f,
                    Source2Amount = 0.5f,
                    Source1 = new CompositionEffectSourceParameter("backdropBrush"),
                    Source2 = new ColorSourceEffect
                    {
                        Color = Color.FromArgb(255, 245, 245, 245)
                    }

                }

            };
            var effectFactory = compositor.CreateEffectFactory(glassEffect);
            var backdropBrush = compositor.CreateBackdropBrush();
            var effectBrush = effectFactory.CreateBrush();
            effectBrush.SetSourceParameter("backdropBrush", backdropBrush);
            var glassVisual = compositor.CreateSpriteVisual();
            glassVisual.Brush = effectBrush;
            ElementCompositionPreview.SetElementChildVisual(glassHost, glassVisual);
            var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);
            glassVisual.StartAnimation("Size", bindSizeAnimation);
        }
        private void zuimeilist_ItemClick(object sender, ItemClickEventArgs e)
        {
            fangdaImage.Source = new BitmapImage(new Uri((e.ClickedItem as Models.teacher).photo_src));
        }
        /// <summary>
        /// 图片点击放大方法
        /// 滑动切换图片方法集合 
        /// 图片是集合 uri用集合索引
        /// 左滑下一张 右滑上一张
        /// </summary>
        bool switcher1 = false, switcher2 = false;
        double x1 = 0, x2 = 0;
        private void SwitchImage(double x1, double x2)
        {
            if (x2 - x1 > 66)
            {
                fangdaImage.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/主页-1-邮子攻略.png"));
            }
            else if (x2 - x1 < -66)
            {
                fangdaImage.Source = new BitmapImage(new Uri(@"ms-appx:///Assets/主页-2-重邮风采.png"));
            }
            else
            {
                Debug.WriteLine("滑动距离过短");
            }
        }
        private void fangdaImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            switcher1 = true;
            var x = e.GetCurrentPoint(fangdaImage);
            Debug.WriteLine("按下 内" + x.Position);
            x1 = x.Position.X;
        }

        private void fangdaImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!switcher2)
            {
                switcher2 = true;
                if (switcher1)
                {
                    var x = e.GetCurrentPoint(fangdaImage);
                    Debug.WriteLine("松开 内" + x.Position);
                    x2 = x.Position.X;
                    Debug.WriteLine(x2 - x1);
                    SwitchImage(x1, x2);
                }
            }
            switcher1 = false;
            switcher2 = false;
        }

        private void fangdaImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            switcher2 = true;
            var x = e.GetCurrentPoint(fangdaImage);
            Debug.WriteLine("松开 内" + x.Position);
            x2 = x.Position.X;
            if (switcher2 && switcher1)
            {
                SwitchImage(x1, x2);
            }
            Debug.WriteLine(x2 - x1);
        }
        /// <summary>
        /// 教师点击查看方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        string ToS = "";
        private void teacherList_ItemClick(object sender, ItemClickEventArgs e)
        {
            initializeFrostedGlass(GlassHost);
            xuhua.Visibility = Visibility.Visible;
            //GlassHost.Visibility = Visibility.Visible;
            teacherList.IsItemClickEnabled = false;
            //ShowContent.Visibility = Visibility.Visible;
            var x = e.ClickedItem as Models.teacher;
            Pic.ImageSource = new BitmapImage(new Uri(x.photo_src));
            _Name.Text = x.name;
            Content.Text = x.college;
            ToS = "T";
        }
        /// <summary>
        /// 学生点击查看方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void studentList_ItemClick(object sender, ItemClickEventArgs e)
        {
            initializeFrostedGlass(GlassHost);
            xuhua.Visibility = Visibility.Visible;
            //GlassHost.Visibility = Visibility.Visible;
            studentList.IsItemClickEnabled = false;
            //ShowContent.Visibility = Visibility.Visible;
            var x = e.ClickedItem as Models.xuezi;
            Pic.ImageSource = new BitmapImage(new Uri(x.photo_src));
            _Name.Text = x.name;
            Content.Text = x.introduction;
            ToS = "S";
        }
        private void waibu_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var o = sender as Models.teacher;
            var x = e.GetCurrentPoint(waibu);
            Debug.WriteLine("按下 rect" + x.Position);
        }

        private void waibu_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var x = e.GetCurrentPoint(waibu);
            Debug.WriteLine("松开 rect" + x.Position);
            if (!((x.Position.X > (waibu.ActualWidth / 2 - 150) && x.Position.X < (waibu.ActualWidth / 2 + 150))
                && (x.Position.Y > (waibu.ActualHeight / 2 - 250) && x.Position.Y < (waibu.ActualHeight / 2 + 250))))
            {
                Debug.WriteLine("点击在外部");
                xuhua.Visibility = Visibility.Collapsed;
                if (ToS == "T")
                {
                    teacherList.IsItemClickEnabled = true;
                }
                else if(ToS=="S")
                {
                    studentList.IsItemClickEnabled = true;
                }else
                {
                    Debug.WriteLine("未知错误");
                }
            }
            ToS = "";
        }
    }
}
