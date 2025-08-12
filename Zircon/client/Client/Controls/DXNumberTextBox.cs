using Ray2D;
using System;
using System.Globalization;
using System.Threading;

//Cleaned
namespace Client.Controls
{
    public sealed class DXNumberTextBox : DXTextBox
    {
        #region Properties

        #region Value

        public long Value
        {
            get { return _Value; }
            set
            {
                if (_Value == value) return;

                long oldValue = _Value;
                _Value = value;

                OnValueChanged(oldValue, value);
            }
        }

        private long _Value;

        public event EventHandler<EventArgs> ValueChanged;

        public void OnValueChanged(long oValue, long nValue)
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);

            Text = Value.ToString("#,##0", Thread.CurrentThread.CurrentCulture.NumberFormat);
            SelectionStart = Text.Length;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region MaxValue

        public long MaxValue
        {
            get { return _MaxValue; }
            set
            {
                if (_MaxValue == value) return;

                long oldValue = _MaxValue;
                _MaxValue = value;

                OnMaxValueChanged(oldValue, value);
            }
        }

        private long _MaxValue;

        public event EventHandler<EventArgs> MaxValueChanged;

        public void OnMaxValueChanged(long oValue, long nValue)
        {
            MaxValueChanged?.Invoke(this, EventArgs.Empty);

            if (Value >= MaxValue)
                Text = MaxValue.ToString();
        }

        #endregion

        #region MinValue

        public long MinValue
        {
            get { return _MinValue; }
            set
            {
                if (_MinValue == value) return;

                long oldValue = _MinValue;
                _MinValue = value;

                OnMinValueChanged(oldValue, value);
            }
        }

        private long _MinValue;

        public event EventHandler<EventArgs> MinValueChanged;

        public void OnMinValueChanged(long oValue, long nValue)
        {
            MinValueChanged?.Invoke(this, EventArgs.Empty);

            if (Value < MinValue)
                Text = MinValue.ToString();
        }

        #endregion

        #endregion

        public DXNumberTextBox()
        {
            TextChanged += TextBox_TextChanged;
            KeyPress += TextBox_KeyPress;
            Text = "0";
        }

        #region Methods

        private void TextBox_KeyPress(object sender, KeyEvent e)
        {
            if (!char.IsControl((char)e.Char) && !char.IsDigit((char)e.Char))
                e.Handled = true;
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            long vol;

            if (long.TryParse(Text, NumberStyles.Integer | NumberStyles.AllowThousands, Thread.CurrentThread.CurrentCulture.NumberFormat, out vol))
            {
                if (vol < MinValue)
                    vol = MinValue;

                if (vol > MaxValue)
                    vol = MaxValue;

                Value = (long)vol;

                Text = Value.ToString("#,##0", Thread.CurrentThread.CurrentCulture.NumberFormat);
            }
            else
            {
                Value = MinValue;
                Text = Value.ToString("#,##0", Thread.CurrentThread.CurrentCulture.NumberFormat);
            }
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _Value = 0;
                _MaxValue = 0;
                _MinValue = 0;

                ValueChanged = null;
                MaxValueChanged = null;
                MinValueChanged = null;
            }
        }

        #endregion
    }
}