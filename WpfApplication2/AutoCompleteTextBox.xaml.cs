using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFAutoCompleteTextbox
{
    /// <summary>
    /// Interaction logic for AutoCompleteTextBox.xaml
    /// </summary>    
    public partial class AutoCompleteTextBox : Canvas
    {
        #region Members
        private VisualCollection controls;
        private TextBox textBox;
        private ComboBox comboBox;
        private ObservableCollection<string> autoCompletionList;
        private System.Timers.Timer keypressTimer;
        private delegate void TextChangedCallback();
        private bool insertText;
        private int delayTime;
        private int searchThreshold;
        #endregion

        #region Constructor
        public AutoCompleteTextBox()
        {
            controls = new VisualCollection(this);
            InitializeComponent();

            autoCompletionList = new ObservableCollection<string>();
            searchThreshold = 2;        // default threshold to 2 char

            // set up the key press timer
            keypressTimer = new System.Timers.Timer();
            keypressTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);

            // set up the text box and the combo box
            comboBox = new ComboBox();
            comboBox.IsSynchronizedWithCurrentItem = true;
            comboBox.IsTabStop = false;
            comboBox.SelectionChanged += new SelectionChangedEventHandler(comboBox_SelectionChanged);


            textBox = new TextBox();
            textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
            textBox.VerticalContentAlignment = VerticalAlignment.Center;
            
            controls.Add(comboBox);
            controls.Add(textBox);
        }
        #endregion

        #region Methods
        public string Text
        {
            get { return textBox.Text; }
            set 
            {
                insertText = true;
                textBox.Text = value; 
            }
        }

        public TextBox TextBoxObject
        {
            get
            {
                return textBox;
            }
        }

        public ComboBox ComboBoxObject
        {
            get
            {
                return comboBox;
            }
        }

        public int DelayTime
        {
            get { return delayTime; }
            set { delayTime = value; }
        }

        public int Threshold
        {
            get { return searchThreshold; }
            set { searchThreshold = value; }
        }

        public void AddItem(string entry)
        {
            autoCompletionList.Add(entry);
        }

        public ObservableCollection<string> ItemSource
        {
            set
            {
                autoCompletionList = value;
            }
            get
            {
                return autoCompletionList;
            }
        }

        public double FrontSize
        {
            set
            {
                textBox.FontSize = value;
             }
        }

        public FontWeight FontWeight
        {
            set
            {
                textBox.FontWeight = value;
            }
        }

        public FontFamily FontFamily
        {

            set
            {
                textBox.FontFamily = value;
            }
        }

        public Brush TextBoxBackground
        {
            get
            {
                return textBox.Background;
            }

            set
            {
                textBox.Background = value;
            }
        }


        public Brush TextBoxForeground
        {
            get
            {
                return textBox.Foreground;
            }
            set
            {
                textBox.Foreground = value;
            }
        }

        public Brush ComboBoxBackground
        {
            get
            {
                return comboBox.Background;
            }
            set
            {
                comboBox.Background = value;
            }
        }

        public Brush ComboBoxForeground
        {
            get
            {
                return comboBox.Foreground;
            }
            set
            {
                comboBox.Foreground = value;
            }
        }

        public bool IsSuggestionOpen
        {
            get
            {
                return comboBox.IsDropDownOpen;
            }
            set
            {
                comboBox.IsDropDownOpen = value;
            }
        }

        public bool TextBoxFocus()
        {
            return textBox.Focus();
        }

        public bool ComboBoxFocus()
        {
            return comboBox.Focus();
        }

        //public void Undo()
        //{

        //}

        //public void Redo()
        //{

        //}

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != comboBox.SelectedItem)
            {
                insertText = true;
                ComboBoxItem cbItem = (ComboBoxItem)comboBox.SelectedItem;
                textBox.Text = cbItem.Content.ToString();
                textBox.Focus();
                textBox.Select(textBox.Text.Length, 0);
            }
        }


        private void TextChanged()
        {
            try
            {
                comboBox.Items.Clear();
                if (textBox.Text.Length >= searchThreshold)
                {
                    foreach (string word in autoCompletionList)
                    {
                        if (word.StartsWith(textBox.Text, StringComparison.CurrentCultureIgnoreCase))
                        {
                            ComboBoxItem cbItem = new ComboBoxItem();
                            cbItem.Content = word;
                            comboBox.Items.Add(cbItem);
                        }
                    }
                    comboBox.IsDropDownOpen = comboBox.HasItems;
                }
                else
                {
                    comboBox.IsDropDownOpen = false;
                }
            }
            catch { }
        }

        
        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            keypressTimer.Stop();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new TextChangedCallback(this.TextChanged));
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // text was not typed, do nothing and consume the flag
            if (insertText == true) insertText = false;
            
            // if the delay time is set, delay handling of text changed
            else
            {
                if (delayTime > 0)
                {
                    keypressTimer.Interval = delayTime;
                    keypressTimer.Start();
                }
                else TextChanged();
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            textBox.Arrange(new Rect(arrangeSize));
            comboBox.Arrange(new Rect(arrangeSize));
            return base.ArrangeOverride(arrangeSize);
        }

        protected override Visual GetVisualChild(int index)
        {
            return controls[index];
        }

        protected override int VisualChildrenCount
        {
            get { return controls.Count; }
        }
        #endregion
    }
}