using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Android.Util;
using Android.Content.Res;
using Java.Interop;
using Android.Runtime;

namespace LightOrganApp.Droid.UI
{
    [Register("com.apps.kruszyn.lightorganapp.droid.NumberPickerPreference")]
    public class NumberPickerPreference: DialogPreference
    {
        private const int DefaultValue = 0;

        private NumberPicker numberPicker;
        private int? currentValue;

        public NumberPickerPreference(Context context, IAttributeSet attrs): base(context, attrs)
        {
            DialogLayoutResource = Resource.Layout.numberpicker_dialog;
            SetPositiveButtonText(Android.Resource.String.Ok);
            SetNegativeButtonText(Android.Resource.String.Cancel);

            DialogIcon = null;
        }

        protected override void OnBindDialogView(View view)
        {
            base.OnBindDialogView(view);

            numberPicker = view.FindViewById<NumberPicker>(Resource.Id.numberPicker);
            numberPicker.MinValue = 0;
            numberPicker.MaxValue = 65535;
            
            if (currentValue != null)
                numberPicker.Value = currentValue.Value;
        }

        protected override void OnDialogClosed(bool positiveResult)
        {
            if (positiveResult)
            {
                currentValue = numberPicker.Value;
                PersistInt(currentValue.Value);
            }
        }
        
        protected override void OnSetInitialValue(bool restorePersistedValue, Java.Lang.Object defaultValue)
        {
            if (restorePersistedValue)
            {
                currentValue = GetPersistedInt(DefaultValue);
            }
            else
            {
                currentValue = (int)defaultValue;
                PersistInt(currentValue.Value);
            }
        }

        protected override Java.Lang.Object OnGetDefaultValue(TypedArray a, int index)
        {
            return a.GetInteger(index, DefaultValue);
        }

        protected override IParcelable OnSaveInstanceState()
        {
            var superState = base.OnSaveInstanceState();

            if (Persistent)
            {
                return superState;
            }

            var myState = new SavedState(superState);
            myState.Value = numberPicker.Value;
            return myState;
        }

        protected override void OnRestoreInstanceState(IParcelable state)
        {
            if (state == null || state.GetType() != typeof(SavedState)) {
                base.OnRestoreInstanceState(state);
                return;
            }

            var myState = (SavedState)state;
            base.OnRestoreInstanceState(myState.SuperState);

            numberPicker.Value = myState.Value;
        }

        public class SavedState: BaseSavedState
        {
            public int Value { get; set; }

            public SavedState(IParcelable superState): base(superState)
            {                
            }

            public SavedState(Parcel source): base(source)
            {                
                Value = source.ReadInt();
            }

            public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
            {
                base.WriteToParcel(dest, flags);
                dest.WriteInt(Value);
            }

            [ExportField("CREATOR")]
            static SavedStateCreator InitializeCreator()
            {
                return new SavedStateCreator();
            }

            class SavedStateCreator : Java.Lang.Object, IParcelableCreator
            {
                public Java.Lang.Object CreateFromParcel(Parcel source)
                {
                    return new SavedState(source);
                }

                public Java.Lang.Object[] NewArray(int size)
                {
                    return new SavedState[size];
                }
            }
        }          
    }
}